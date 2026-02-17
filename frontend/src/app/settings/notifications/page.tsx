'use client';

import { useState } from 'react';
import { useUserPreferences } from '@/hooks/useUserPreferences';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Bell, Mail, Shield, Wrench, FileText, Zap, Loader2, Check } from 'lucide-react';

interface ExtendedUserPreferences {
  currency: string;
  distanceUnit: string;
  volumeUnit: string;
  fuelEfficiencyUnit: string;
  temperatureUnit: string;
  dateFormat: string;
  theme: string;
  timezone: string;
  serviceReminders: boolean;
  tripReports: boolean;
  fuelPriceAlerts: boolean;
  productUpdates: boolean;
  securityAlerts: boolean;
  emailFrequency: string;
}

const EMAIL_FREQUENCY = [
  { value: 'realtime', label: 'Real-time' },
  { value: 'daily', label: 'Daily digest' },
  { value: 'weekly', label: 'Weekly digest' },
  { value: 'never', label: 'Never' },
];

const NOTIFICATION_CATEGORIES = [
  {
    id: 'serviceReminders',
    title: 'Service Reminders',
    description: 'Get notified about vehicle maintenance schedules',
    icon: Wrench,
    defaultOn: true,
  },
  {
    id: 'tripReports',
    title: 'Trip Reports',
    description: 'Receive weekly or monthly trip summaries',
    icon: FileText,
    defaultOn: false,
  },
  {
    id: 'fuelPriceAlerts',
    title: 'Fuel Price Alerts',
    description: 'Get notified when fuel prices change',
    icon: Zap,
    defaultOn: false,
  },
  {
    id: 'productUpdates',
    title: 'Product Updates',
    description: 'Learn about new features and improvements',
    icon: Bell,
    defaultOn: false,
  },
  {
    id: 'securityAlerts',
    title: 'Security Alerts',
    description: 'Get notified about login activity and security events',
    icon: Shield,
    defaultOn: true,
  },
];

export default function NotificationsPage() {
  const { preferences, loading, error, updatePreferences } = useUserPreferences() as {
    preferences: ExtendedUserPreferences;
    loading: boolean;
    error: string | null;
    updatePreferences: (updates: Partial<ExtendedUserPreferences>) => Promise<boolean>;
  };
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleToggle = async (key: keyof ExtendedUserPreferences, value: boolean) => {
    setSaving(true);
    setSuccessMessage(null);
    
    const success = await updatePreferences({ [key]: value });
    
    if (success) {
      setSuccessMessage(`${key} preference saved`);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
    
    setSaving(false);
  };

  const handleFrequencyChange = async (value: string) => {
    setSaving(true);
    setSuccessMessage(null);
    
    const success = await updatePreferences({ emailFrequency: value });
    
    if (success) {
      setSuccessMessage('Email frequency updated');
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

      {/* Email Frequency */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Mail className="h-5 w-5" />
            Email Frequency
          </CardTitle>
          <CardDescription>
            How often would you like to receive notification emails?
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Select
            value={preferences.emailFrequency}
            onValueChange={handleFrequencyChange}
            disabled={saving}
          >
            <SelectTrigger className="w-full md:w-64">
              <SelectValue placeholder="Select frequency" />
            </SelectTrigger>
            <SelectContent>
              {EMAIL_FREQUENCY.map((freq) => (
                <SelectItem key={freq.value} value={freq.value}>
                  {freq.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Notification Toggles */}
      <Card>
        <CardHeader>
          <CardTitle>Notification Types</CardTitle>
          <CardDescription>
            Choose which types of notifications you want to receive
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-6">
            {NOTIFICATION_CATEGORIES.map((category) => {
              const Icon = category.icon;
              const isOn = preferences[category.id as keyof ExtendedUserPreferences] as boolean;
              
              return (
                <div key={category.id} className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className={`p-2 rounded-lg ${
                      isOn 
                        ? 'bg-blue-100 dark:bg-blue-900/30 text-blue-600' 
                        : 'bg-gray-100 dark:bg-gray-800 text-gray-500'
                    }`}>
                      <Icon className="h-5 w-5" />
                    </div>
                    <div>
                      <Label className="text-base font-medium">{category.title}</Label>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {category.description}
                      </p>
                    </div>
                  </div>
                  <Switch
                    checked={isOn}
                    onCheckedChange={(checked) => handleToggle(category.id as keyof ExtendedUserPreferences, checked)}
                    disabled={saving}
                  />
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Info Note */}
      <Card className="bg-blue-50 dark:bg-blue-900/10 border-blue-200 dark:border-blue-800">
        <CardContent className="py-4">
          <p className="text-sm text-blue-700 dark:text-blue-300">
            <strong>Note:</strong> Security alerts cannot be disabled as they are important for 
            keeping your account safe. You will always be notified about suspicious activity 
            and login attempts.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}