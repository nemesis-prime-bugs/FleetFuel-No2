'use client';

import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';
import { useToast } from '@/components/ui/Toast';
import { Shield, Cookie, Share2, User, Download, Trash2, AlertTriangle, Loader2, Check } from 'lucide-react';

interface PrivacySettings {
  shareAnalytics: boolean;
  allowPersonalizedAds: boolean;
  enableCookies: boolean;
  allowThirdPartySharing: boolean;
  showProfilePublicly: boolean;
}

export default function PrivacyPage() {
  const [settings, setSettings] = useState<PrivacySettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState('');
  const { toast } = useToast();

  useEffect(() => {
    fetchPrivacySettings();
  }, []);

  const fetchPrivacySettings = async () => {
    try {
      const response = await fetch('/api/privacy/settings');
      if (response.ok) {
        const data = await response.json();
        setSettings(data.data);
      }
    } catch (error) {
      console.error('Error fetching privacy settings:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSettingChange = async (key: keyof PrivacySettings, value: boolean) => {
    setSaving(true);
    try {
      const response = await fetch('/api/privacy/settings', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ settings: { ...settings, [key]: value } }),
      });

      if (response.ok) {
        setSettings((prev) => prev ? { ...prev, [key]: value } : null);
        toast.success(`${key} updated`);
      }
    } catch (error) {
      toast.error('Failed to update settings');
    } finally {
      setSaving(false);
    }
  };

  const handleExportData = async () => {
    setExporting(true);
    try {
      const response = await fetch('/api/privacy/export', {
        method: 'POST',
      });

      if (response.ok) {
        const data = await response.json();
        if (data.data.downloadUrl) {
          toast.success('Export ready! Download will start shortly.');
          // In production, would redirect to downloadUrl
        }
      }
    } catch (error) {
      toast.error('Failed to create export');
    } finally {
      setExporting(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (deleteConfirm.toLowerCase() !== 'delete my account') {
      toast.error('Please type "delete my account" to confirm');
      return;
    }

    setDeleting(true);
    try {
      const response = await fetch('/api/privacy/delete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ confirmation: deleteConfirm, exportDataFirst: true }),
      });

      if (response.ok) {
        const data = await response.json();
        toast.success(data.data.message);
      } else {
        const error = await response.json();
        toast.error(error.error || 'Failed to delete account');
      }
    } catch (error) {
      toast.error('Failed to delete account');
    } finally {
      setDeleting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Data Sharing */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Share2 className="h-5 w-5" />
            Data Sharing
          </CardTitle>
          <CardDescription>
            Control how your data is used and shared
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Share Usage Analytics</Label>
              <p className="text-sm text-gray-500">
                Help us improve by sharing anonymous usage data
              </p>
            </div>
            <Switch
              checked={settings?.shareAnalytics || false}
              onCheckedChange={(checked) => handleSettingChange('shareAnalytics', checked)}
              disabled={saving}
            />
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Personalized Ads</Label>
              <p className="text-sm text-gray-500">
                Allow personalized advertisements based on your usage
              </p>
            </div>
            <Switch
              checked={settings?.allowPersonalizedAds || false}
              onCheckedChange={(checked) => handleSettingChange('allowPersonalizedAds', checked)}
              disabled={saving}
            />
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Third-Party Sharing</Label>
              <p className="text-sm text-gray-500">
                Allow sharing data with integrated services
              </p>
            </div>
            <Switch
              checked={settings?.allowThirdPartySharing || false}
              onCheckedChange={(checked) => handleSettingChange('allowThirdPartySharing', checked)}
              disabled={saving}
            />
          </div>
        </CardContent>
      </Card>

      {/* Cookies & Tracking */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Cookie className="h-5 w-5" />
            Cookies & Tracking
          </CardTitle>
          <CardDescription>
            Manage cookie preferences and tracking
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Enable Cookies</Label>
              <p className="text-sm text-gray-500">
                Allow cookies for better user experience
              </p>
            </div>
            <Switch
              checked={settings?.enableCookies || true}
              onCheckedChange={(checked) => handleSettingChange('enableCookies', checked)}
              disabled={saving}
            />
          </div>
        </CardContent>
      </Card>

      {/* Profile Visibility */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="h-5 w-5" />
            Profile Visibility
          </CardTitle>
          <CardDescription>
            Control your profile visibility
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Public Profile</Label>
              <p className="text-sm text-gray-500">
                Allow others to view your profile
              </p>
            </div>
            <Switch
              checked={settings?.showProfilePublicly || false}
              onCheckedChange={(checked) => handleSettingChange('showProfilePublicly', checked)}
              disabled={saving}
            />
          </div>
        </CardContent>
      </Card>

      {/* Data Management */}
      <Card>
        <CardHeader>
          <CardTitle>Your Data</CardTitle>
          <CardDescription>
            Download or delete your personal data
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Button onClick={handleExportData} disabled={exporting} variant="outline">
            {exporting ? (
              <Loader2 className="h-4 w-4 animate-spin mr-2" />
            ) : (
              <Download className="h-4 w-4 mr-2" />
            )}
            Download My Data
          </Button>

          <p className="text-sm text-gray-500">
            You will receive a download link for all your data including vehicles, trips, and receipts.
          </p>
        </CardContent>
      </Card>

      {/* Danger Zone */}
      <Card className="border-red-200 dark:border-red-900">
        <CardHeader className="border-b border-red-200 dark:border-red-900">
          <CardTitle className="text-red-600 dark:text-red-400 flex items-center gap-2">
            <AlertTriangle className="h-5 w-5" />
            Danger Zone
          </CardTitle>
          <CardDescription className="text-red-500">
            Permanently delete your account and all associated data
          </CardDescription>
        </CardHeader>
        <CardContent className="pt-6">
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="destructive">
                <Trash2 className="h-4 w-4 mr-2" />
                Delete Account
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                <AlertDialogDescription>
                  This action <strong>cannot</strong> be undone. This will permanently delete your
                  account, remove all your vehicles, trips, and receipts from our servers.
                </AlertDialogDescription>
              </AlertDialogHeader>
              
              <div className="py-4">
                <Label htmlFor="confirm-delete">
                  Type <code className="bg-gray-100 dark:bg-gray-800 px-2 py-1 rounded">delete my account</code> to confirm
                </Label>
                <Input
                  id="confirm-delete"
                  value={deleteConfirm}
                  onChange={(e) => setDeleteConfirm(e.target.value)}
                  placeholder="delete my account"
                  className="mt-2"
                />
              </div>

              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={handleDeleteAccount}
                  disabled={deleteConfirm.toLowerCase() !== 'delete my account' || deleting}
                  className="bg-red-600 hover:bg-red-700"
                >
                  {deleting ? (
                    <Loader2 className="h-4 w-4 animate-spin mr-2" />
                  ) : (
                    <Trash2 className="h-4 w-4 mr-2" />
                  )}
                  Delete My Account
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </CardContent>
      </Card>
    </div>
  );
}