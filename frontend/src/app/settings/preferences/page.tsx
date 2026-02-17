'use client';

import { useState } from 'react';
import { useTheme } from '@/contexts/ThemeContext';
import { useUserPreferences, UserPreferences } from '@/hooks/useUserPreferences';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Sun, Moon, Monitor, Loader2, Check } from 'lucide-react';

const CURRENCIES = [
  { value: 'USD', label: 'USD ($)', symbol: '$' },
  { value: 'EUR', label: 'EUR (€)', symbol: '€' },
  { value: 'GBP', label: 'GBP (£)', symbol: '£' },
  { value: 'CAD', label: 'CAD ($)', symbol: '$' },
  { value: 'AUD', label: 'AUD ($)', symbol: '$' },
  { value: 'JPY', label: 'JPY (¥)', symbol: '¥' },
  { value: 'CHF', label: 'CHF (Fr)', symbol: 'Fr' },
];

const DISTANCE_UNITS = [
  { value: 'km', label: 'Kilometers', abbreviation: 'km' },
  { value: 'mi', label: 'Miles', abbreviation: 'mi' },
];

const VOLUME_UNITS = [
  { value: 'L', label: 'Liters', abbreviation: 'L' },
  { value: 'gal_us', label: 'Gallons (US)', abbreviation: 'gal' },
  { value: 'gal_uk', label: 'Gallons (UK)', abbreviation: 'gal' },
];

const FUEL_EFFICIENCY_UNITS = [
  { value: 'L/100km', label: 'Liters per 100km', abbreviation: 'L/100km' },
  { value: 'mpg_us', label: 'MPG (US)', abbreviation: 'mpg' },
  { value: 'mpg_uk', label: 'MPG (UK)', abbreviation: 'mpg' },
];

const TEMPERATURE_UNITS = [
  { value: 'C', label: 'Celsius', symbol: '°C' },
  { value: 'F', label: 'Fahrenheit', symbol: '°F' },
];

const DATE_FORMATS = [
  { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY', example: '17/02/2026' },
  { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY', example: '02/17/2026' },
  { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD', example: '2026-02-17' },
];

const TIMEZONES = [
  { value: 'UTC', label: 'UTC' },
  { value: 'Europe/Vienna', label: 'Europe/Vienna' },
  { value: 'America/New_York', label: 'America/New_York' },
  { value: 'America/Los_Angeles', label: 'America/Los_Angeles' },
  { value: 'Europe/London', label: 'Europe/London' },
  { value: 'Asia/Tokyo', label: 'Asia/Tokyo' },
];

export default function PreferencesPage() {
  const { theme, setTheme, resolvedTheme } = useTheme();
  const { preferences, loading, error, updatePreferences } = useUserPreferences();
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handlePreferenceChange = async (key: keyof UserPreferences, value: string) => {
    setSaving(true);
    setSuccessMessage(null);
    
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

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
      </div>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="py-8">
          <p className="text-red-600 text-center">Error loading preferences: {error}</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Success Message */}
      {successMessage && (
        <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-4 flex items-center">
          <Check className="h-5 w-5 text-green-600 mr-2" />
          <p className="text-green-700 dark:text-green-400">{successMessage}</p>
        </div>
      )}

      {/* Theme */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {resolvedTheme === 'dark' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
            Theme
          </CardTitle>
          <CardDescription>
            Choose your preferred color scheme
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-3 gap-4">
            <button
              onClick={() => handlePreferenceChange('theme', 'light')}
              className={`p-4 rounded-lg border-2 transition-all ${
                preferences.theme === 'light' || (preferences.theme === 'system' && resolvedTheme === 'light')
                  ? 'border-blue-500 bg-white dark:bg-gray-800'
                  : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              <Sun className="h-8 w-8 mx-auto mb-2 text-yellow-500" />
              <p className="font-medium text-sm">Light</p>
            </button>
            
            <button
              onClick={() => handlePreferenceChange('theme', 'dark')}
              className={`p-4 rounded-lg border-2 transition-all ${
                preferences.theme === 'dark' || (preferences.theme === 'system' && resolvedTheme === 'dark')
                  ? 'border-blue-500 bg-white dark:bg-gray-800'
                  : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              <Moon className="h-8 w-8 mx-auto mb-2 text-blue-500" />
              <p className="font-medium text-sm">Dark</p>
            </button>
            
            <button
              onClick={() => handlePreferenceChange('theme', 'system')}
              className={`p-4 rounded-lg border-2 transition-all ${
                preferences.theme === 'system'
                  ? 'border-blue-500 bg-white dark:bg-gray-800'
                  : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              <Monitor className="h-8 w-8 mx-auto mb-2 text-gray-500" />
              <p className="font-medium text-sm">System</p>
            </button>
          </div>
        </CardContent>
      </Card>

      {/* Currency */}
      <Card>
        <CardHeader>
          <CardTitle>Currency</CardTitle>
          <CardDescription>
            Choose your preferred currency for displaying costs
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Select
            value={preferences.currency}
            onValueChange={(value) => handlePreferenceChange('currency', value)}
            disabled={saving}
          >
            <SelectTrigger className="w-full md:w-64">
              <SelectValue placeholder="Select currency" />
            </SelectTrigger>
            <SelectContent>
              {CURRENCIES.map((currency) => (
                <SelectItem key={currency.value} value={currency.value}>
                  {currency.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Units */}
      <Card>
        <CardHeader>
          <CardTitle>Units</CardTitle>
          <CardDescription>
            Choose your preferred measurement units
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <Label>Distance</Label>
              <Select
                value={preferences.distanceUnit}
                onValueChange={(value) => handlePreferenceChange('distanceUnit', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {DISTANCE_UNITS.map((unit) => (
                    <SelectItem key={unit.value} value={unit.value}>
                      {unit.label} ({unit.abbreviation})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Volume</Label>
              <Select
                value={preferences.volumeUnit}
                onValueChange={(value) => handlePreferenceChange('volumeUnit', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {VOLUME_UNITS.map((unit) => (
                    <SelectItem key={unit.value} value={unit.value}>
                      {unit.label} ({unit.abbreviation})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Fuel Efficiency</Label>
              <Select
                value={preferences.fuelEfficiencyUnit}
                onValueChange={(value) => handlePreferenceChange('fuelEfficiencyUnit', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {FUEL_EFFICIENCY_UNITS.map((unit) => (
                    <SelectItem key={unit.value} value={unit.value}>
                      {unit.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Temperature</Label>
              <Select
                value={preferences.temperatureUnit}
                onValueChange={(value) => handlePreferenceChange('temperatureUnit', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {TEMPERATURE_UNITS.map((unit) => (
                    <SelectItem key={unit.value} value={unit.value}>
                      {unit.label} {unit.symbol}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Date & Time */}
      <Card>
        <CardHeader>
          <CardTitle>Date & Time</CardTitle>
          <CardDescription>
            Choose your preferred date format and timezone
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <Label>Date Format</Label>
              <Select
                value={preferences.dateFormat}
                onValueChange={(value) => handlePreferenceChange('dateFormat', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {DATE_FORMATS.map((format) => (
                    <SelectItem key={format.value} value={format.value}>
                      {format.label} ({format.example})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Timezone</Label>
              <Select
                value={preferences.timezone}
                onValueChange={(value) => handlePreferenceChange('timezone', value)}
                disabled={saving}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {TIMEZONES.map((tz) => (
                    <SelectItem key={tz.value} value={tz.value}>
                      {tz.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Reset to Defaults */}
      <div className="flex justify-end">
        <Button
          variant="outline"
          onClick={() => {
            handlePreferenceChange('currency', 'USD');
            handlePreferenceChange('distanceUnit', 'km');
            handlePreferenceChange('volumeUnit', 'L');
            handlePreferenceChange('fuelEfficiencyUnit', 'L/100km');
            handlePreferenceChange('temperatureUnit', 'C');
            handlePreferenceChange('dateFormat', 'DD/MM/YYYY');
            handlePreferenceChange('timezone', 'UTC');
            handlePreferenceChange('theme', 'system');
          }}
          disabled={saving}
        >
          Reset to Defaults
        </Button>
      </div>
    </div>
  );
}