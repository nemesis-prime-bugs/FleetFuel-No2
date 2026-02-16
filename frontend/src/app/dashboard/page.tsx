'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { getTrips, type Trip } from '@/lib/api/trips';
import { getReceipts, type Receipt } from '@/lib/api/receipts';
import { getVehicles, type Vehicle } from '@/lib/api/vehicles';
import { useToast } from '@/components/ui/Toast';

interface DashboardStats {
  totalTrips: number;
  totalDistance: number;
  totalExpenses: number;
  totalVehicles: number;
}

export default function DashboardPage() {
  const { user, loading, signOut } = useAuth();
  const router = useRouter();
  const { showToast } = useToast();
  const [stats, setStats] = useState<DashboardStats>({
    totalTrips: 0,
    totalDistance: 0,
    totalExpenses: 0,
    totalVehicles: 0,
  });
  const [recentTrips, setRecentTrips] = useState<Trip[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [fetching, setFetching] = useState(true);

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    }
  }, [user, loading, router]);

  useEffect(() => {
    if (user) {
      fetchDashboardData();
    }
  }, [user]);

  const fetchDashboardData = async () => {
    try {
      const [tripsData, receiptsData, vehiclesData] = await Promise.all([
        getTrips().catch(() => []),
        getReceipts().catch(() => []),
        getVehicles().catch(() => []),
      ]);

      const totalDistance = tripsData.reduce((sum, trip) => sum + (trip.end_km - trip.start_km), 0);
      const totalExpenses = receiptsData.reduce((sum, receipt) => sum + Number(receipt.amount), 0);

      setStats({
        totalTrips: tripsData.length,
        totalDistance,
        totalExpenses,
        totalVehicles: vehiclesData.length,
      });

      setRecentTrips(tripsData.slice(0, 5));
      setVehicles(vehiclesData);
    } catch (error) {
      showToast('Failed to load dashboard data', 'error');
    } finally {
      setFetching(false);
    }
  };

  if (loading || fetching) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-900">FleetFuel</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-600">{user.email}</span>
              <button
                onClick={() => signOut()}
                className="text-sm text-red-600 hover:text-red-500"
              >
                Sign out
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-gray-900">Dashboard</h2>
            <Link
              href="/trips/new"
              className="inline-flex items-center px-4 py-2 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700"
            >
              + Log Trip
            </Link>
          </div>

          {/* KPI Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <div className="bg-white rounded-lg shadow p-6">
              <p className="text-sm font-medium text-gray-500">Total Vehicles</p>
              <p className="mt-2 text-3xl font-bold text-gray-900">{stats.totalVehicles}</p>
            </div>
            
            <div className="bg-white rounded-lg shadow p-6">
              <p className="text-sm font-medium text-gray-500">Total Trips</p>
              <p className="mt-2 text-3xl font-bold text-gray-900">{stats.totalTrips}</p>
            </div>
            
            <div className="bg-white rounded-lg shadow p-6">
              <p className="text-sm font-medium text-gray-500">Total Distance</p>
              <p className="mt-2 text-3xl font-bold text-gray-900">{stats.totalDistance.toLocaleString()} km</p>
            </div>
            
            <div className="bg-white rounded-lg shadow p-6">
              <p className="text-sm font-medium text-gray-500">Total Expenses</p>
              <p className="mt-2 text-3xl font-bold text-gray-900">€ {stats.totalExpenses.toFixed(2)}</p>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Recent Trips */}
            <div className="lg:col-span-2 bg-white rounded-lg shadow overflow-hidden">
              <div className="px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-medium text-gray-900">Recent Trips</h3>
              </div>
              {recentTrips.length === 0 ? (
                <div className="p-6 text-center text-gray-500">
                  No trips recorded yet
                </div>
              ) : (
                <div className="divide-y divide-gray-200">
                  {recentTrips.map((trip) => (
                    <div key={trip.id} className="px-6 py-4 flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-gray-900">
                          {trip.vehicle?.name || 'Unknown Vehicle'}
                        </p>
                        <p className="text-sm text-gray-500">
                          {trip.date} • {trip.purpose || 'No purpose'}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-medium text-gray-900">
                          {(trip.end_km - trip.start_km).toLocaleString()} km
                        </p>
                        <span
                          className={`inline-flex px-2 py-0.5 text-xs font-semibold rounded-full ${
                            trip.is_business
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {trip.is_business ? 'Business' : 'Private'}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
              <div className="px-6 py-4 bg-gray-50 border-t border-gray-200">
                <Link href="/trips" className="text-sm font-medium text-blue-600 hover:text-blue-500">
                  View all trips →
                </Link>
              </div>
            </div>

            {/* Quick Actions */}
            <div className="bg-white rounded-lg shadow overflow-hidden">
              <div className="px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-medium text-gray-900">Quick Actions</h3>
              </div>
              <div className="p-6 space-y-4">
                <Link
                  href="/vehicles/new"
                  className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <p className="text-sm font-medium text-gray-900">Add Vehicle</p>
                  <p className="text-sm text-gray-500">Register a new vehicle</p>
                </Link>
                <Link
                  href="/trips/new"
                  className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <p className="text-sm font-medium text-gray-900">Log Trip</p>
                  <p className="text-sm text-gray-500">Record a new trip</p>
                </Link>
                <Link
                  href="/receipts/new"
                  className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  <p className="text-sm font-medium text-gray-900">Upload Receipt</p>
                  <p className="text-sm text-gray-500">Add fuel receipt</p>
                </Link>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
