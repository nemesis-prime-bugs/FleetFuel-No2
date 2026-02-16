'use client';

import { useEffect, useState } from 'react';
import { useOnboarding } from '@/contexts/OnboardingContext';
import { useRouter } from 'next/navigation';

interface OnboardingGuideProps {
  onActionComplete?: () => void;
}

export function OnboardingGuide({ onActionComplete }: OnboardingGuideProps) {
  const { hasVehicle, hasTrip, refreshOnboardingState } = useOnboarding();
  const router = useRouter();
  const [currentPrompt, setCurrentPrompt] = useState<'vehicle' | 'trip' | 'receipt' | null>(null);
  const [dismissed, setDismissed] = useState(false);

  useEffect(() => {
    refreshOnboardingState().then(() => {
      // Determine current prompt based on state
      if (hasVehicle === false) {
        setCurrentPrompt('vehicle');
        setDismissed(false);
      } else if (hasVehicle === true && hasTrip === false) {
        setCurrentPrompt('trip');
        setDismissed(false);
      } else if (hasTrip === true) {
        setCurrentPrompt('receipt');
        setDismissed(false);
      } else {
        setCurrentPrompt(null);
      }
    });
  }, [hasVehicle, hasTrip]);

  const handleAction = () => {
    if (currentPrompt === 'vehicle') {
      router.push('/vehicles/new');
    } else if (currentPrompt === 'trip') {
      router.push('/trips/new');
    } else if (currentPrompt === 'receipt') {
      router.push('/receipts/new');
    }
    onActionComplete?.();
  };

  const handleDismiss = () => {
    setDismissed(true);
  };

  if (dismissed || currentPrompt === null) {
    return null;
  }

  const prompts = {
    vehicle: {
      title: 'Add Your First Vehicle',
      description: 'Start by adding a vehicle to track your fleet.',
      action: 'Add Vehicle',
    },
    trip: {
      title: 'Log Your First Trip',
      description: 'Vehicle added! Now log your first trip to start tracking.',
      action: 'Log Trip',
    },
    receipt: {
      title: 'Upload a Receipt',
      description: 'Trips tracked! Upload a fuel receipt to complete your expense record.',
      action: 'Upload Receipt',
    },
  };

  const prompt = prompts[currentPrompt];

  return (
    <div className="mb-6 p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-sm font-semibold text-blue-900 dark:text-blue-100">
            {prompt.title}
          </h3>
          <p className="mt-1 text-sm text-blue-700 dark:text-blue-300">
            {prompt.description}
          </p>
        </div>
        <button
          onClick={handleDismiss}
          className="ml-4 text-blue-400 hover:text-blue-600 dark:hover:text-blue-200"
          aria-label="Dismiss"
        >
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
      <div className="mt-4">
        <button
          onClick={handleAction}
          className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-900"
        >
          {prompt.action}
        </button>
      </div>
    </div>
  );
}
