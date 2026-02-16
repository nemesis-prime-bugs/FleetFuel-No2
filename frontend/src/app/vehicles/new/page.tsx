'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { createVehicle } from '@/lib/api/vehicles';
import { useToast } from '@/components/ui/Toast';
import { useOnboarding } from '@/contexts/OnboardingContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ChevronLeft } from 'lucide-react';

const vehicleSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  license_plate: z.string().min(1, 'License plate is required').max(20),
  initial_mileage: z.number().min(0, 'Initial mileage must be positive'),
});

type VehicleForm = z.infer<typeof vehicleSchema>;

export default function NewVehiclePage() {
  const router = useRouter();
  const { showToast } = useToast();
  const { markVehicleAdded } = useOnboarding();
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
      markVehicleAdded();
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
        <Link
          href="/vehicles"
          className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground mb-6"
        >
          <ChevronLeft className="h-4 w-4 mr-1" />
          Back to vehicles
        </Link>

        <Card>
          <CardHeader>
            <CardTitle>Add Vehicle</CardTitle>
            <CardDescription>
              Register a new vehicle to your fleet
            </CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="name">Vehicle Name</Label>
                <Input
                  id="name"
                  placeholder="e.g., Company Car"
                  {...register('name')}
                />
                {errors.name && (
                  <p className="text-sm text-red-600">{errors.name.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="license_plate">License Plate</Label>
                <Input
                  id="license_plate"
                  placeholder="e.g., AB-123-CD"
                  {...register('license_plate')}
                  onChange={(e) => {
                    e.target.value = e.target.value.toUpperCase();
                  }}
                />
                {errors.license_plate && (
                  <p className="text-sm text-red-600">{errors.license_plate.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="initial_mileage">Initial Mileage (km)</Label>
                <Input
                  id="initial_mileage"
                  type="number"
                  min="0"
                  {...register('initial_mileage', { valueAsNumber: true })}
                />
                {errors.initial_mileage && (
                  <p className="text-sm text-red-600">{errors.initial_mileage.message}</p>
                )}
              </div>

              <div className="flex justify-end space-x-4 pt-4">
                <Link href="/vehicles">
                  <Button variant="outline" type="button">
                    Cancel
                  </Button>
                </Link>
                <Button type="submit" disabled={loading}>
                  {loading ? 'Creating...' : 'Create Vehicle'}
                </Button>
              </div>
            </CardContent>
          </form>
        </Card>
      </div>
    </div>
  );
}