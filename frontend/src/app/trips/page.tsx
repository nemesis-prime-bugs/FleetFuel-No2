'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { OnboardingGuide } from '@/components/OnboardingGuide';
import { NoTripsEmptyState } from '@/components/ui/EmptyState';
import { getTrips, type Trip } from '@/lib/api/trips';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Plus } from 'lucide-react';

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
          <Link href="/trips/new">
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Log Trip
            </Button>
          </Link>
        </div>

        <OnboardingGuide />

        {trips.length === 0 ? (
          <NoTripsEmptyState />
        ) : (
          <Card>
            <CardHeader>
              <CardTitle>All Trips</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Vehicle</TableHead>
                    <TableHead>Distance</TableHead>
                    <TableHead>Purpose</TableHead>
                    <TableHead>Type</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {trips.map((trip) => (
                    <TableRow key={trip.id}>
                      <TableCell>{trip.date}</TableCell>
                      <TableCell>{trip.vehicle?.name || '-'}</TableCell>
                      <TableCell>{(trip.end_km - trip.start_km).toLocaleString()} km</TableCell>
                      <TableCell className="text-muted-foreground">
                        {trip.purpose || '-'}
                      </TableCell>
                      <TableCell>
                        <span
                          className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                            trip.is_business
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {trip.is_business ? 'Business' : 'Private'}
                        </span>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}