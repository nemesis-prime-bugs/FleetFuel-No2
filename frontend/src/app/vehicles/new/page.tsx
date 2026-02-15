'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createVehicle } from '@/lib/api/vehicles';
import { useToast } from '@/components/ui/Toast';

const vehicleSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  license_plate: z.string().min(1, 'License plate is required').max(20),
  initial_mileage: z.number().min(0, 'Initial mileage must be positive'),
});

type VehicleForm = z.infer<typeof vehicleSchema>;

export default function NewVehiclePage() {
  const router = useRouter();
  const { showToast } = useToast();
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<VehicleForm>({
    resolver: zodResolver(vehicleSchema),
    defaultValues: {
      initial_mileage: 0,
    },
  });

  const onSubmit = async (data: VehicleForm) => {
    setLoading(true);
    try {
      await createVehicle({
        name: data.name,
        license_plate: data.license_plate.toUpperCase(),
        initial_mileage: data.initial_mileage,
      });
      showToast('Vehicle created successfully', 'success');
      router.push('/vehicles');
    } catch (error) {
      showToast(error instanceof Error ? error.message : 'Failed to create vehicle', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-md mx-auto px-4">
        <div className="mb-8">
          <Link href="/vehicles" className="text-blue-600 hover:text-blue-500 text-sm">
            ← Back to vehicles
          </Link>
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Add Vehicle</h1>
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
              placeholder="e.g., Company Car"
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
              placeholder="e.g., AB-123-CD"
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
              {loading ? 'Creating...' : 'Create Vehicle'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
