'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { getVehicle, updateVehicle } from '@/lib/api/vehicles';
import { useToast } from '@/components/ui/Toast';
import type { Vehicle } from '@/lib/api/vehicles';

const vehicleSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  license_plate: z.string().min(1, 'License plate is required').max(20),
  initial_mileage: z.number().min(0, 'Initial mileage must be positive'),
});

type VehicleForm = z.infer<typeof vehicleSchema>;

export default function EditVehiclePage() {
  const router = useRouter();
  const params = useParams();
  const { showToast } = useToast();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [vehicle, setVehicle] = useState<Vehicle | null>(null);

  const vehicleId = params.id as string;

  const { register, handleSubmit, formState: { errors }, reset } = useForm<VehicleForm>({
    resolver: zodResolver(vehicleSchema),
  });

  useEffect(() => {
    if (vehicleId) {
      fetchVehicle();
    }
  }, [vehicleId]);

  const fetchVehicle = async () => {
    try {
      const data = await getVehicle(vehicleId);
      setVehicle(data);
      reset({
        name: data.name,
        license_plate: data.license_plate,
        initial_mileage: data.initial_mileage,
      });
    } catch (error) {
      showToast('Failed to load vehicle', 'error');
      router.push('/vehicles');
    } finally {
      setFetching(false);
    }
  };

  const onSubmit = async (data: VehicleForm) => {
    if (!vehicle) return;

    setLoading(true);
    try {
      await updateVehicle(vehicleId, {
        name: data.name,
        license_plate: data.license_plate.toUpperCase(),
        initial_mileage: data.initial_mileage,
      });
      showToast('Vehicle updated successfully', 'success');
      router.push('/vehicles');
    } catch (error) {
      showToast(error instanceof Error ? error.message : 'Failed to update vehicle', 'error');
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!vehicle) return null;

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-md mx-auto px-4">
        <div className="mb-8">
          <Link href="/vehicles" className="text-blue-600 hover:text-blue-500 text-sm">
            ← Back to vehicles
          </Link>
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Edit Vehicle</h1>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="bg-white shadow rounded-lg p-6 space-y-6">
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700">
              Vehicle Name
            </label>
            <input
              {...register('name')}
              id="name"
              type="text"
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            />
            {errors.name && (
              <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="license_plate" className="block text-sm font-medium text-gray-700">
              License Plate
            </label>
            <input
              {...register('license_plate')}
              id="license_plate"
              type="text"
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              onChange={(e) => {
                e.target.value = e.target.value.toUpperCase();
              }}
            />
            {errors.license_plate && (
              <p className="mt-1 text-sm text-red-600">{errors.license_plate.message}</p>
            )}
          </div>

          <div>
            <label htmlFor="initial_mileage" className="block text-sm font-medium text-gray-700">
              Initial Mileage (km)
            </label>
            <input
              {...register('initial_mileage', { valueAsNumber: true })}
              id="initial_mileage"
              type="number"
              min="0"
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            />
            {errors.initial_mileage && (
              <p className="mt-1 text-sm text-red-600">{errors.initial_mileage.message}</p>
            )}
          </div>

          <div className="flex justify-end space-x-4">
            <Link
              href="/vehicles"
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </Link>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {loading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
