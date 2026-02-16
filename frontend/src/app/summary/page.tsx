'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { getYearlySummary, exportYearlySummaryCsv, type YearlySummary, type VehicleSummary } from '@/lib/api/summary';
import { useToast } from '@/components/ui/Toast';

export default function SummaryPage() {
  const { user, loading } = useAuth();
  const router = useRouter();
  const { showToast } = useToast();
  const [summary, setSummary] = useState<YearlySummary | null>(null);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [fetching, setFetching] = useState(true);
  const [exporting, setExporting] = useState(false);

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 10 }, (_, i) => currentYear - i);

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    }
  }, [user, loading, router]);

  useEffect(() => {
    if (user) {
      fetchSummary();
    }
  }, [user, selectedYear]);

  const fetchSummary = async () => {
    setFetching(true);
    try {
      const data = await getYearlySummary(selectedYear);
      setSummary(data);
    } catch (error) {
      showToast('Failed to load summary', 'error');
    } finally {
      setFetching(false);
    }
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      const blob = await exportYearlySummaryCsv(selectedYear);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `fleetfuel-summary-${selectedYear}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
      showToast('CSV exported successfully', 'success');
    } catch (error) {
      showToast('Failed to export CSV', 'error');
    } finally {
      setExporting(false);
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
              <Link href="/dashboard" className="text-xl font-bold text-gray-900">
                FleetFuel
              </Link>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-600">{user.email}</span>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">Yearly Summary</h2>
              <p className="text-sm text-gray-500 mt-1">
                Tax-ready report for your accountant (Steuerberater)
              </p>
            </div>
            <div className="flex items-center gap-4">
              <select
                value={selectedYear}
                onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                className="block w-32 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-blue-500"
              >
                {years.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
              <button
                onClick={handleExport}
                disabled={exporting || !summary}
                className="inline-flex items-center px-4 py-2 bg-green-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
              >
                {exporting ? 'Exporting...' : 'Export CSV'}
              </button>
            </div>
          </div>

          {summary === null ? (
            <div className="bg-white rounded-lg shadow p-12 text-center">
              <h3 className="text-lg font-medium text-gray-900">No data for {selectedYear}</h3>
              <p className="mt-2 text-gray-500">Start logging trips and receipts to see your summary.</p>
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
            <>
              {/* Total Summary Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                <div className="bg-white rounded-lg shadow p-6">
                  <p className="text-sm font-medium text-gray-500">Total Trips</p>
                  <p className="mt-2 text-3xl font-bold text-gray-900">{summary.total_trips}</p>
                  <p className="text-sm text-gray-500 mt-1">
                    {summary.total_business_trips} business
                  </p>
                </div>
                
                <div className="bg-white rounded-lg shadow p-6">
                  <p className="text-sm font-medium text-gray-500">Total Distance</p>
                  <p className="mt-2 text-3xl font-bold text-gray-900">{summary.total_km.toLocaleString()} km</p>
                  <p className="text-sm text-green-600 mt-1">
                    {summary.total_business_km.toLocaleString()} km business
                  </p>
                </div>
                
                <div className="bg-white rounded-lg shadow p-6">
                  <p className="text-sm font-medium text-gray-500">Total Expenses</p>
                  <p className="mt-2 text-3xl font-bold text-gray-900">€ {summary.total_expenses.toFixed(2)}</p>
                  <p className="text-sm text-gray-500 mt-1">
                    {summary.total_receipts} receipts
                  </p>
                </div>
                
                <div className="bg-white rounded-lg shadow p-6">
                  <p className="text-sm font-medium text-gray-500">Vehicles Used</p>
                  <p className="mt-2 text-3xl font-bold text-gray-900">{summary.vehicle_summaries.length}</p>
                  <p className="text-sm text-gray-500 mt-1">
                    Active this year
                  </p>
                </div>
              </div>

              {/* Per-Vehicle Breakdown */}
              <div className="bg-white rounded-lg shadow overflow-hidden">
                <div className="px-6 py-4 border-b border-gray-200">
                  <h3 className="text-lg font-medium text-gray-900">Per Vehicle Breakdown</h3>
                </div>
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Vehicle
                        </th>
                        <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Trips
                        </th>
                        <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Distance
                        </th>
                        <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Business KM
                        </th>
                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Expenses
                        </th>
                        <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Receipts
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {summary.vehicle_summaries.map((vehicle) => (
                        <tr key={vehicle.vehicle_id}>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-medium text-gray-900">
                              {vehicle.vehicle_name}
                            </div>
                            <div className="text-sm text-gray-500">
                              {vehicle.license_plate}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-900">
                            {vehicle.trip_count}
                            {vehicle.business_trip_count > 0 && (
                              <span className="ml-1 text-xs text-green-600">
                                ({vehicle.business_trip_count})
                              </span>
                            )}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-900">
                            {vehicle.total_km.toLocaleString()} km
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-900">
                            {vehicle.business_km.toLocaleString()} km
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium text-gray-900">
                            € {vehicle.total_expenses.toFixed(2)}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500">
                            {vehicle.receipt_count}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Export Info */}
              <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-blue-700">
                      This summary is ready for your accountant. Click "Export CSV" to download a detailed report with all vehicle breakdowns and totals.
                    </p>
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
      </main>
    </div>
  );
}