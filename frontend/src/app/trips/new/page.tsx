'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useToast } from '@/components/ui/Toast';
import { useOnboarding } from '@/contexts/OnboardingContext';
import { getVehicles, type Vehicle } from '@/lib/api/vehicles';
import { createTrip } from '@/lib/api/trips';

const tripSchema = z.object({
  vehicle_id: z.string().min(1, 'Vehicle is required'),
  date: z.string().min(1, 'Date is required'),
  start_km: z.number().min(0, 'Must be 0 or higher'),
  end_km: z.number().min(1, 'End KM must be greater than 0'),
  purpose: z.string().optional(),
  is_business: z.boolean(),
}).refine((data) => data.end_km > data.start_km, {
  message: 'End KM must be greater than Start KM',
  path: ['end_km'],
});

type TripForm = z.infer<typeof tripSchema>;

export default function NewTripPage() {
  const router = useRouter();
  const { showToast } = useToast();
  const { markTripAdded } = useOnboarding();
  const [loading, setLoading] = useState(false);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);

  const formRef = useRef<HTMLFormElement>(null);
  const vehicleSelectRef = useRef<HTMLSelectElement>(null);
  const dateInputRef = useRef<HTMLInputElement>(null);
  const startKmInputRef = useRef<HTMLInputElement>(null);
  const endKmInputRef = useRef<HTMLInputElement>(null);
  const purposeInputRef = useRef<HTMLTextAreaElement>(null);
  const submitButtonRef = useRef<HTMLButtonElement>(null);

  const { register, handleSubmit, watch, setValue, formState: { errors, isValid } } = useForm<TripForm>({
    resolver: zodResolver(tripSchema),
    mode: 'onChange',
    defaultValues: {
      date: new Date().toISOString().split('T')[0],
      is_business: true,
    },
  });

  const watchVehicleId = watch('vehicle_id');
  const watchStartKm = watch('start_km');
  const watchEndKm = watch('end_km');

  // Load last used vehicle and business preference
  useEffect(() => {
    const lastVehicleId = localStorage.getItem('last_vehicle_id');
    const lastIsBusiness = localStorage.getItem('last_is_business');

    if (lastIsBusiness !== null) {
      setValue('is_business', lastIsBusiness === 'true');
    }
    if (lastVehicleId) {
      setValue('vehicle_id', lastVehicleId);
    }
  }, [setValue]);

  // Fetch vehicles
  useEffect(() => {
    getVehicles()
      .then(setVehicles)
      .catch((error) => showToast(error.message, 'error'));
  }, []);

  // Set default start km based on last trip or initial mileage
  useEffect(() => {
    if (watchVehicleId) {
      localStorage.setItem('last_vehicle_id', watchVehicleId);

      const lastEndKm = localStorage.getItem(`last_end_km_${watchVehicleId}`);
      if (lastEndKm) {
        setValue('start_km', parseInt(lastEndKm, 10));
      }
    }
  }, [watchVehicleId, setValue]);

  // Save end km preference when changed
  useEffect(() => {
    if (watchEndKm !== undefined && watchEndKm > 0 && watchVehicleId) {
      localStorage.setItem(`last_end_km_${watchVehicleId}`, watchEndKm.toString());
    }
  }, [watchEndKm, watchVehicleId]);

  // Persist business preference
  const handleBusinessChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    localStorage.setItem('last_is_business', e.target.checked.toString());
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Enter' && e.target instanceof HTMLElement && e.target.tagName !== 'TEXTAREA') {
        e.preventDefault();

        const target = e.target as HTMLElement;

        if (target === vehicleSelectRef.current) {
          dateInputRef.current?.focus();
        } else if (target === dateInputRef.current) {
          startKmInputRef.current?.focus();
        } else if (target === startKmInputRef.current) {
          endKmInputRef.current?.focus();
        } else if (target === endKmInputRef.current) {
          purposeInputRef.current?.focus();
        } else if (target === purposeInputRef.current) {
          submitButtonRef.current?.focus();
        } else if (target === submitButtonRef.current && isValid) {
          // Submit form
          formRef.current?.dispatchEvent(new Event('submit', { cancelable: true, bubbles: true }));
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isValid]);

  const onSubmit = async (data: TripForm) => {
    setLoading(true);
    try {
      await createTrip({
        vehicle_id: data.vehicle_id,
        date: data.date,
        start_km: data.start_km,
        end_km: data.end_km,
        purpose: data.purpose,
        is_business: data.is_business,
      });
      showToast('Trip logged successfully', 'success');
      markTripAdded();
      router.push('/trips');
    } catch (error) {
      showToast(error instanceof Error ? error.message : 'Failed to log trip', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-md mx-auto px-4">
        <div className="mb-8">
          <Link href="/trips" className="text-blue-600 hover:text-blue-500 text-sm">
            ← Back to trips
          </Link>
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Log Trip</h1>
        </div>

        <form
          ref={formRef}
          onSubmit={handleSubmit(onSubmit)}
          className="bg-white shadow rounded-lg p-6 space-y-5"
        >
          <div>
            <label htmlFor="vehicle_id" className="block text-sm font-medium text-gray-700">
              Vehicle *
            </label>
            <select
              {...register('vehicle_id')}
              ref={(e) => {
                register('vehicle_id').ref(e);
                vehicleSelectRef.current = e;
              }}
              id="vehicle_id"
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2.5 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              autoFocus
            >
              <option value="">Select a vehicle</option>
              {vehicles.map((vehicle) => (
                <option key={vehicle.id} value={vehicle.id}>
                  {vehicle.name} ({vehicle.license_plate})
                </option>
              ))}
            </select>
            {errors.vehicle_id && (
              <p className="mt-1 text-sm text-red-600">{errors.vehicle_id.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="date" className="block text-sm font-medium text-gray-700">
              Date *
            </label>
            <input
              {...register('date')}
              ref={(e) => {
                register('date').ref(e);
                dateInputRef.current = e;
              }}
              id="date"
              type="date"
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2.5 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            />
            {errors.date && (
              <p className="mt-1 text-sm text-red-600">{errors.date.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="start_km" className="block text-sm font-medium text-gray-700">
                Start KM *
              </label>
              <input
                {...register('start_km', { valueAsNumber: true })}
                ref={(e) => {
                  register('start_km').ref(e);
                  startKmInputRef.current = e;
                }}
                id="start_km"
                type="number"
                min="0"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2.5 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              />
              {errors.start_km && (
                <p className="mt-1 text-sm text-red-600">{errors.start_km.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="end_km" className="block text-sm font-medium text-gray-700">
                End KM *
              </label>
              <input
                {...register('end_km', { valueAsNumber: true })}
                ref={(e) => {
                  register('end_km').ref(e);
                  endKmInputRef.current = e;
                }}
                id="end_km"
                type="number"
                min="0"
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2.5 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              />
              {errors.end_km && (
                <p className="mt-1 text-sm text-red-600">{errors.end_km.message}</p>
              )}
            </div>
          </div>

          {watchEndKm !== undefined && watchStartKm !== undefined && watchEndKm > watchStartKm && (
            <p className="text-sm text-gray-600">
              Distance: <span className="font-medium">{(watchEndKm - watchStartKm).toLocaleString()} km</span>
            </p>
          )}

          <div>
            <label htmlFor="purpose" className="block text-sm font-medium text-gray-700">
              Purpose (optional)
            </label>
            <textarea
              {...register('purpose')}
              ref={(e) => {
                register('purpose').ref(e);
                purposeInputRef.current = e;
              }}
              id="purpose"
              rows={2}
              placeholder="Client meeting, delivery, etc."
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2.5 focus:border-blue-500 focus:ring-blue-500 sm:text-sm resize-none"
            />
          </div>

          <div className="flex items-center">
            <input
              {...register('is_business')}
              id="is_business"
              type="checkbox"
              className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              onChange={handleBusinessChange}
            />
            <label htmlFor="is_business" className="ml-2 block text-sm text-gray-700">
              Business trip
            </label>
          </div>

          <div className="flex justify-end space-x-4 pt-2">
            <Link
              href="/trips"
              className="px-4 py-2.5 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </Link>
            <button
              type="submit"
              ref={submitButtonRef}
              disabled={loading || !isValid}
              className="px-4 py-2.5 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Logging...' : 'Log Trip'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
