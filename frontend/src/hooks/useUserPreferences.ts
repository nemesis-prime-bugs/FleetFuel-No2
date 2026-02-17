import { useState, useEffect, useCallback } from 'react';

export interface UserPreferences {
  currency: string;
  distanceUnit: string;
  volumeUnit: string;
  fuelEfficiencyUnit: string;
  temperatureUnit: string;
  dateFormat: string;
  theme: string;
  timezone: string;
}

const DEFAULT_PREFERENCES: UserPreferences = {
  currency: 'USD',
  distanceUnit: 'km',
  volumeUnit: 'L',
  fuelEfficiencyUnit: 'L/100km',
  temperatureUnit: 'C',
  dateFormat: 'DD/MM/YYYY',
  theme: 'system',
  timezone: 'UTC',
};

export function useUserPreferences() {
  const [preferences, setPreferences] = useState<UserPreferences | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPreferences = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await fetch('/api/user/preferences');
      
      if (!response.ok) {
        if (response.status === 401) {
          // Not authenticated - use defaults
          setPreferences(DEFAULT_PREFERENCES);
          return;
        }
        throw new Error('Failed to fetch preferences');
      }
      
      const data = await response.json();
      setPreferences(data.data || DEFAULT_PREFERENCES);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      setPreferences(DEFAULT_PREFERENCES);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchPreferences();
  }, [fetchPreferences]);

  const updatePreferences = async (updates: Partial<UserPreferences>) => {
    try {
      setError(null);
      
      const response = await fetch('/api/user/preferences', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ preferences: { ...preferences, ...updates } }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to update preferences');
      }
      
      const data = await response.json();
      setPreferences(data.data);
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
      return false;
    }
  };

  return {
    preferences: preferences || DEFAULT_PREFERENCES,
    loading,
    error,
    refresh: fetchPreferences,
    updatePreferences,
  };
}