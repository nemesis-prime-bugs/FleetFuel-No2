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
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { ChevronLeft, Car } from 'lucide-react';

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
  const [distance, setDistance] = useState<number | null>(null);

  const formRef = useRef<HTMLFormElement>(null);
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

  // Calculate distance
  useEffect(() => {
    if (watchStartKm !== undefined && watchEndKm !== undefined && watchEndKm > watchStartKm) {
      setDistance(watchEndKm - watchStartKm);
    } else {
      setDistance(null);
    }
  }, [watchStartKm, watchEndKm]);

  // Persist business preference
  const handleBusinessChange = (checked: boolean) => {
    localStorage.setItem('last_is_business', checked.toString());
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Enter' && e.target instanceof HTMLElement && e.target.tagName !== 'TEXTAREA') {
        e.preventDefault();

        const target = e.target as HTMLElement;

        if (submitButtonRef.current && isValid) {
          submitButtonRef.current.focus();
        } else if (submitButtonRef.current && isValid) {
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
        <Link
          href="/trips"
          className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground mb-6"
        >
          <ChevronLeft className="h-4 w-4 mr-1" />
          Back to trips
        </Link>

        <Card>
          <CardHeader>
            <CardTitle>Log Trip</CardTitle>
            <CardDescription>
              Record a vehicle trip for tracking
            </CardDescription>
          </CardHeader>
          <form ref={formRef} onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="vehicle_id">Vehicle *</Label>
                <Select
                  onValueChange={(value) => setValue('vehicle_id', value)}
                  defaultValue={watchVehicleId}
                >
                  <SelectTrigger autoFocus>
                    <SelectValue placeholder="Select a vehicle" />
                  </SelectTrigger>
                  <SelectContent>
                    {vehicles.map((vehicle) => (
                      <SelectItem key={vehicle.id} value={vehicle.id}>
                        {vehicle.name} ({vehicle.license_plate})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.vehicle_id && (
                  <p className="text-sm text-red-600">{errors.vehicle_id.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="date">Date *</Label>
                <Input
                  id="date"
                  type="date"
                  {...register('date')}
                />
                {errors.date && (
                  <p className="text-sm text-red-600">{errors.date.message}</p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="start_km">Start KM *</Label>
                  <Input
                    id="start_km"
                    type="number"
                    min="0"
                    {...register('start_km', { valueAsNumber: true })}
                  />
                  {errors.start_km && (
                    <p className="text-sm text-red-600">{errors.start_km.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="end_km">End KM *</Label>
                  <Input
                    id="end_km"
                    type="number"
                    min="0"
                    {...register('end_km', { valueAsNumber: true })}
                  />
                  {errors.end_km && (
                    <p className="text-sm text-red-600">{errors.end_km.message}</p>
                  )}
                </div>
              </div>

              {distance !== null && (
                <div className="bg-muted rounded-md p-3 text-center">
                  <p className="text-sm text-muted-foreground">Distance</p>
                  <p className="text-2xl font-bold">{distance.toLocaleString()} km</p>
                </div>
              )}

              <div className="space-y-2">
                <Label htmlFor="purpose">Purpose (optional)</Label>
                <textarea
                  {...register('purpose')}
                  id="purpose"
                  rows={2}
                  placeholder="Client meeting, delivery, etc."
                  className="flex min-h-[60px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 resize-none"
                />
              </div>

              <div className="flex items-center space-x-2">
                <Checkbox
                  id="is_business"
                  defaultChecked
                  onCheckedChange={(checked) => handleBusinessChange(checked as boolean)}
                />
                <Label htmlFor="is_business" className="text-sm font-normal">
                  Business trip
                </Label>
              </div>

              <div className="flex justify-end space-x-4 pt-4">
                <Link href="/trips">
                  <Button variant="outline" type="button">
                    Cancel
                  </Button>
                </Link>
                <Button
                  type="submit"
                  ref={submitButtonRef}
                  disabled={loading || !isValid}
                >
                  {loading ? 'Logging...' : 'Log Trip'}
                </Button>
              </div>
            </CardContent>
          </form>
        </Card>
      </div>
    </div>
  );
}