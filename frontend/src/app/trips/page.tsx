'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { OnboardingGuide } from '@/components/OnboardingGuide';
import { getTrips, type Trip } from '@/lib/api/trips';
import { useToast } from '@/components/ui/Toast';

export default function TripsPage() {
  const [trips, setTrips] = useState<Trip[]>([]);
  const [loading, setLoading] = useState(true);
  const { showToast } = useToast();

  useEffect(() => {
    async function fetchTrips() {
      try {
        const data = await getTrips();
        setTrips(data);
      } catch (error) {
        showToast('Failed to load trips', 'error');
      } finally {
        setLoading(false);
      }
    }

    fetchTrips();
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="max-w-4xl mx-auto px-4">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/4"></div>
            <div className="h-32 bg-gray-200 rounded"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-4xl mx-auto px-4">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Trips</h1>
          <Link
            href="/trips/new"
            className="inline-flex items-center px-4 py-2 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700"
          >
            + Log Trip
          </Link>
        </div>

        <OnboardingGuide />

        {trips.length === 0 ? (
          <div className="bg-white shadow rounded-lg p-12 text-center">
            <h3 className="text-lg font-medium text-gray-900">No trips yet</h3>
            <p className="mt-2 text-gray-500">Log your first trip to start tracking.</p>
            <div className="mt-6">
              <Link
                href="/trips/new"
                className="inline-flex items-center px-4 py-2 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700"
              >
                Log First Trip
              </Link>
            </div>
          </div>
        ) : (
          <div className="bg-white shadow rounded-lg overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Vehicle
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Distance
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Purpose
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Type
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {trips.map((trip) => (
                  <tr key={trip.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {trip.date}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {trip.vehicle?.name || '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {trip.end_km - trip.start_km} km
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {trip.purpose || '-'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          trip.is_business
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}
                      >
                        {trip.is_business ? 'Business' : 'Private'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
