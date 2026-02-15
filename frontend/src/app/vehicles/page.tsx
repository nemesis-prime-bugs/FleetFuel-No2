'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { getVehicles, deleteVehicle, type Vehicle } from '@/lib/api/vehicles';
import { NoVehiclesEmptyState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';

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
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link href="/dashboard" className="text-xl font-bold text-gray-900">
                FleetFuel
              </Link>
            </div>
            <div className="flex items-center space-x-4">
              <Link
                href="/vehicles/new"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                + Add Vehicle
              </Link>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 sm:px-0">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Vehicles</h1>
          </div>

          {vehicles.length === 0 ? (
            <div className="bg-white rounded-lg shadow">
              <NoVehiclesEmptyState />
            </div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {vehicles.map(vehicle => (
                <div
                  key={vehicle.id}
                  className="bg-white rounded-lg shadow p-6 hover:shadow-md transition-shadow"
                >
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="text-lg font-medium text-gray-900">
                        {vehicle.name}
                      </h3>
                      <p className="text-sm text-gray-500 mt-1">
                        {vehicle.license_plate}
                      </p>
                    </div>
                    <div className="flex space-x-2">
                      <Link
                        href={`/vehicles/${vehicle.id}`}
                        className="text-blue-600 hover:text-blue-500 text-sm font-medium"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(vehicle.id)}
                        disabled={deleting === vehicle.id}
                        className="text-red-600 hover:text-red-500 text-sm font-medium disabled:opacity-50"
                      >
                        {deleting === vehicle.id ? '...' : 'Delete'}
                      </button>
                    </div>
                  </div>
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <p className="text-sm text-gray-600">
                      Initial mileage: <span className="font-medium">{vehicle.initial_mileage.toLocaleString()} km</span>
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
