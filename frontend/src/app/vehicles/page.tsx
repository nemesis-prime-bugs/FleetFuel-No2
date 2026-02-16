'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { getVehicles, deleteVehicle, type Vehicle } from '@/lib/api/vehicles';
import { useToast } from '@/components/ui/Toast';
import { NoVehiclesEmptyState } from '@/components/ui/EmptyState';
import { OnboardingGuide } from '@/components/OnboardingGuide';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Plus, Settings, Trash2 } from 'lucide-react';

export default function VehiclesPage() {
  const { user, loading } = useAuth();
  const router = useRouter();
  const { showToast } = useToast();
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [fetching, setFetching] = useState(true);
  const [deleting, setDeleting] = useState<string | null>(null);

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    }
  }, [user, loading, router]);

  useEffect(() => {
    if (user) {
      fetchVehicles();
    }
  }, [user]);

  const fetchVehicles = async () => {
    try {
      const data = await getVehicles();
      setVehicles(data);
    } catch (error) {
      showToast('Failed to load vehicles', 'error');
    } finally {
      setFetching(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this vehicle?')) return;

    setDeleting(id);
    try {
      await deleteVehicle(id);
      setVehicles(prev => prev.filter(v => v.id !== id));
      showToast('Vehicle deleted', 'success');
    } catch (error) {
      showToast('Failed to delete vehicle', 'error');
    } finally {
      setDeleting(null);
    }
  };

  if (loading || fetching) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!user) return null;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link href="/dashboard" className="text-xl font-bold text-gray-900">
                FleetFuel
              </Link>
            </div>
            <div className="flex items-center space-x-4">
              <Link href="/vehicles/new">
                <Button>
                  <Plus className="h-4 w-4 mr-2" />
                  Add Vehicle
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 sm:px-0">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Vehicles</h1>
          </div>

          <OnboardingGuide />

          {vehicles.length === 0 ? (
            <NoVehiclesEmptyState />
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {vehicles.map(vehicle => (
                <Card key={vehicle.id} className="hover:shadow-md transition-shadow">
                  <CardContent className="pt-6">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="text-lg font-semibold">
                          {vehicle.name}
                        </h3>
                        <p className="text-sm text-muted-foreground mt-1">
                          {vehicle.license_plate}
                        </p>
                      </div>
                      <div className="flex space-x-2">
                        <Link href={`/vehicles/${vehicle.id}`}>
                          <Button variant="ghost" size="icon">
                            <Settings className="h-4 w-4" />
                          </Button>
                        </Link>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleDelete(vehicle.id)}
                          disabled={deleting === vehicle.id}
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                          {deleting === vehicle.id ? (
                            <span className="text-xs">...</span>
                          ) : (
                            <Trash2 className="h-4 w-4" />
                          )}
                        </Button>
                      </div>
                    </div>
                    <div className="mt-4 pt-4 border-t">
                      <p className="text-sm text-muted-foreground">
                        Initial mileage: <span className="font-medium text-foreground">{vehicle.initial_mileage.toLocaleString()} km</span>
                      </p>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}