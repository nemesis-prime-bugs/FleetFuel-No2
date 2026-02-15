'use client';

import { useAuth } from '@/contexts/AuthContext';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export default function DashboardPage() {
  const { user, loading, signOut } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
    }
  }, [user, loading, router]);

  if (loading) {
    return <div className="min-h-screen flex items-center justify-center">Loading...</div>;
  }

  if (!user) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
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
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Link href="/vehicles" className="block p-6 bg-white rounded-lg shadow hover:shadow-md transition-shadow">
              <h3 className="text-lg font-medium text-gray-900">Vehicles</h3>
              <p className="mt-2 text-gray-600">Manage your vehicles</p>
            </Link>
            
            <Link href="/trips" className="block p-6 bg-white rounded-lg shadow hover:shadow-md transition-shadow">
              <h3 className="text-lg font-medium text-gray-900">Trips</h3>
              <p className="mt-2 text-gray-600">Log and track trips</p>
            </Link>
            
            <Link href="/summary" className="block p-6 bg-white rounded-lg shadow hover:shadow-md transition-shadow">
              <h3 className="text-lg font-medium text-gray-900">Summary</h3>
              <p className="mt-2 text-gray-600">View yearly reports</p>
            </Link>
          </div>
        </div>
      </main>
    </div>
  );
}
