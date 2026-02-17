'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { useTheme } from '@/contexts/ThemeContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useUserPreferences, UserPreferences } from '@/hooks/useUserPreferences';

const CURRENCIES = [
  { value: 'USD', label: 'USD ($)' },
  { value: 'EUR', label: 'EUR (€)' },
  { value: 'GBP', label: 'GBP (£)' },
  { value: 'CAD', label: 'CAD ($)' },
  { value: 'AUD', label: 'AUD ($)' },
  { value: 'JPY', label: 'JPY (¥)' },
  { value: 'CHF', label: 'CHF (Fr)' },
];

const DISTANCE_UNITS = [
  { value: 'km', label: 'Kilometers (km)' },
  { value: 'mi', label: 'Miles (mi)' },
];

const VOLUME_UNITS = [
  { value: 'L', label: 'Liters (L)' },
  { value: 'gal_us', label: 'Gallons (US)' },
  { value: 'gal_uk', label: 'Gallons (UK)' },
];

const FUEL_EFFICIENCY_UNITS = [
  { value: 'L/100km', label: 'Liters per 100km' },
  { value: 'mpg_us', label: 'MPG (US)' },
  { value: 'mpg_uk', label: 'MPG (UK)' },
];

const TEMPERATURE_UNITS = [
  { value: 'C', label: 'Celsius (°C)' },
  { value: 'F', label: 'Fahrenheit (°F)' },
];

const DATE_FORMATS = [
  { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY' },
  { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY' },
  { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD' },
];

const THEME_OPTIONS = [
  { value: 'system', label: 'System' },
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
];

export default function SettingsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, signOut } = useAuth();
  const { theme, setTheme, resolvedTheme } = useTheme();
  const { preferences, loading, error, updatePreferences } = useUserPreferences();
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handlePreferenceChange = async (key: keyof UserPreferences, value: string) => {
    setSaving(true);
    setSuccessMessage(null);
    
    // Update local state immediately for better UX
    const updatedPrefs = { ...preferences, [key]: value };
    
    // Update theme immediately if it's a theme change
    if (key === 'theme') {
      setTheme(value as 'light' | 'dark' | 'system');
    }
    
    const success = await updatePreferences({ [key]: value });
    
    if (success) {
      setSuccessMessage(`${key} preference saved`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
    
    setSaving(false);
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-950">
      <header className="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16 items-center">
            <h1 className="text-xl font-bold text-gray-900 dark:text-white">Settings</h1>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {user?.email || 'Not logged in'}
              </span>
              <Button variant="outline" size="sm" onClick={() => signOut()}>
                Sign Out
              </Button>
            </div>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          {/* Navigation */}
          <nav className="space-y-1">
            <Link
              href="/settings/preferences"
              className={`block px-3 py-2 rounded-md text-sm font-medium ${
                pathname === '/settings/preferences'
                  ? 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white'
                  : 'text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
              }`}
            >
              Preferences
            </Link>
            <Link
              href="/settings/profile"
              className={`block px-3 py-2 rounded-md text-sm font-medium ${
                pathname === '/settings/profile'
                  ? 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white'
                  : 'text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
              }`}
            >
              Profile
            </Link>
            <Link
              href="/settings/security"
              className={`block px-3 py-2 rounded-md text-sm font-medium ${
                pathname === '/settings/security'
                  ? 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white'
                  : 'text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
              }`}
            >
              Security
            </Link>
          </nav>

          {/* Content */}
          <div className="md:col-span-3">
            {children}
          </div>
        </div>
      </div>
    </div>
  );
}