'use client';

import { createContext, useContext, useEffect, useState } from 'react';
import { useAuth } from './AuthContext';

interface OnboardingContextType {
  hasCompletedOnboarding: boolean;
  hasVehicle: boolean | null;
  hasTrip: boolean | null;
  markVehicleAdded: () => void;
  markTripAdded: () => void;
  completeOnboarding: () => void;
  refreshOnboardingState: () => Promise<void>;
}

const OnboardingContext = createContext<OnboardingContextType | undefined>(undefined);

export function OnboardingProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const [hasCompletedOnboarding, setHasCompletedOnboarding] = useState(false);
  const [hasVehicle, setHasVehicle] = useState<boolean | null>(null);
  const [hasTrip, setHasTrip] = useState<boolean | null>(null);

  const refreshOnboardingState = async () => {
    if (!user) {
      setHasVehicle(null);
      setHasTrip(null);
      setHasCompletedOnboarding(false);
      return;
    }

    // Check localStorage for onboarding state
    const stored = localStorage.getItem('onboarding_state');
    if (stored) {
      const state = JSON.parse(stored);
      setHasCompletedOnboarding(state.completed ?? false);
      setHasVehicle(state.hasVehicle ?? null);
      setHasTrip(state.hasTrip ?? null);
    } else {
      // Default: new user has no vehicle/trip
      setHasVehicle(false);
      setHasTrip(false);
      setHasCompletedOnboarding(false);
    }
  };

  useEffect(() => {
    refreshOnboardingState();
  }, [user]);

  const markVehicleAdded = () => {
    const newState = {
      completed: false,
      hasVehicle: true,
      hasTrip: false,
    };
    localStorage.setItem('onboarding_state', JSON.stringify(newState));
    setHasVehicle(true);
    setHasTrip(false);
  };

  const markTripAdded = () => {
    const newState = {
      completed: false,
      hasVehicle: true,
      hasTrip: true,
    };
    localStorage.setItem('onboarding_state', JSON.stringify(newState));
    setHasTrip(true);
  };

  const completeOnboarding = () => {
    const newState = {
      completed: true,
      hasVehicle: true,
      hasTrip: true,
    };
    localStorage.setItem('onboarding_state', JSON.stringify(newState));
    setHasCompletedOnboarding(true);
    setHasVehicle(true);
    setHasTrip(true);
  };

  return (
    <OnboardingContext.Provider
      value={{
        hasCompletedOnboarding,
        hasVehicle,
        hasTrip,
        markVehicleAdded,
        markTripAdded,
        completeOnboarding,
        refreshOnboardingState,
      }}
    >
      {children}
    </OnboardingContext.Provider>
  );
}

export function useOnboarding() {
  const context = useContext(OnboardingContext);
  if (context === undefined) {
    throw new Error('useOnboarding must be used within an OnboardingProvider');
  }
  return context;
}
