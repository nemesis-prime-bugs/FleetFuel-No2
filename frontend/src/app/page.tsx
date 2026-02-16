'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useAuth } from '@/contexts/AuthContext';
import { OnboardingGuide } from '@/components/OnboardingGuide';

export default function Home() {
  const { user, loading } = useAuth();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted || loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="animate-pulse text-gray-500">Loading...</div>
      </div>
    );
  }

  // Not logged in - show landing page
  if (!user) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 dark:bg-black">
        <main className="w-full max-w-3xl px-8 py-16 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 dark:text-white sm:text-6xl">
            FleetFuel
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-600 dark:text-gray-400">
            Professional fleet management made simple.
          </p>
          <div className="mt-10 flex items-center justify-center gap-x-6">
            <Link
              href="/login"
              className="rounded-md bg-blue-600 px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600"
            >
              Sign In
            </Link>
            <Link
              href="/register"
              className="text-sm font-semibold leading-6 text-gray-900 dark:text-white"
            >
              Create Account <span aria-hidden="true">→</span>
            </Link>
          </div>
        </main>
      </div>
    );
  }

  // Logged in - redirect to dashboard (onboarding handled there)
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-black">
      <header className="bg-white dark:bg-gray-900 shadow">
        <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8 flex items-center justify-between">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white">
            FleetFuel
          </h1>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-600 dark:text-gray-400">
              {user.email}
            </span>
          </div>
        </div>
      </header>
      <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <OnboardingGuide />
        <div className="bg-white dark:bg-gray-900 rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Welcome to FleetFuel
          </h2>
          <p className="text-gray-600 dark:text-gray-400">
            Your fleet management dashboard. Start by adding a vehicle to begin tracking.
          </p>
          <div className="mt-6 flex gap-4">
            <Link
              href="/vehicles/new"
              className="inline-flex items-center rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-500"
            >
              Add Vehicle
            </Link>
            <Link
              href="/vehicles"
              className="inline-flex items-center rounded-md border border-gray-300 dark:border-gray-700 px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
            >
              View Vehicles
            </Link>
          </div>
        </div>
      </main>
    </div>
  );
}
