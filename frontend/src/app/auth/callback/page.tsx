'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { supabase } from '@/lib/supabase';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

function AuthCallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState('Processing confirmation...');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleAuthCallback = async () => {
      try {
        // Get code from URL params
        const code = searchParams.get('code');
        const errorParam = searchParams.get('error');
        const errorDescription = searchParams.get('error_description');

        if (errorParam) {
          setError(`Authentication error: ${errorDescription || errorParam}`);
          return;
        }

        if (code) {
          // Exchange the code for a session
          const { data, error: exchangeError } = await supabase.auth.exchangeCodeForSession(code);

          if (exchangeError) {
            setError(exchangeError.message);
            return;
          }

          if (data.session) {
            setStatus('Email confirmed successfully!');
            
            // Redirect to dashboard after a short delay
            setTimeout(() => {
              router.push('/dashboard');
            }, 2000);
          }
        } else {
          setStatus('No confirmation code found. Please check your email link.');
        }
      } catch (err) {
        setError('An unexpected error occurred during confirmation.');
        console.error('Auth callback error:', err);
      }
    };

    handleAuthCallback();
  }, [searchParams, router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold text-center">Email Confirmation</CardTitle>
          <CardDescription className="text-center">
            {error ? 'Confirmation Failed' : 'Please wait...'}
          </CardDescription>
        </CardHeader>
        <CardContent className="text-center">
          {error ? (
            <div className="text-red-600 bg-red-50 border border-red-200 rounded-md p-4">
              <p className="font-medium">❌ {error}</p>
              <p className="text-sm mt-2">
                Please try registering again or contact support.
              </p>
            </div>
          ) : (
            <div className="text-green-600 bg-green-50 border border-green-200 rounded-md p-4">
              <p className="font-medium">✅ {status}</p>
              <p className="text-sm mt-2">
                Redirecting to dashboard...
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function LoadingFallback() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold text-center">Email Confirmation</CardTitle>
          <CardDescription className="text-center">Please wait...</CardDescription>
        </CardHeader>
        <CardContent className="text-center">
          <div className="text-blue-600 bg-blue-50 border border-blue-200 rounded-md p-4">
            <p className="font-medium">⏳ Processing confirmation...</p>
            <p className="text-sm mt-2">Please wait while we verify your email.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function AuthCallbackPage() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <AuthCallbackContent />
    </Suspense>
  );
}