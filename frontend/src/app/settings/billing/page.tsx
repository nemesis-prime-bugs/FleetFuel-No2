'use client';

import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Loader2, Check, CreditCard, Zap, Building2, AlertCircle } from 'lucide-react';

interface SubscriptionPlan {
  id: string;
  name: string;
  price: number;
  currency: string;
  interval: string;
  features: string[];
  maxVehicles: number;
  maxTripsPerMonth: number;
}

interface SubscriptionInfo {
  tier: number;
  currentPeriodStart: string | null;
  currentPeriodEnd: string | null;
  isActive: boolean;
}

interface UsageStats {
  vehicleCount: number;
  tripCountCurrentMonth: number;
  receiptCountCurrentMonth: number;
  vehicleLimit: number;
  tripLimit: number;
  vehicleUsagePercent: number;
  tripUsagePercent: number;
}

const TIERS = {
  0: { name: 'Free', color: 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200' },
  1: { name: 'Pro', color: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200' },
  2: { name: 'Business', color: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200' },
};

export default function BillingPage() {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [subscription, setSubscription] = useState<SubscriptionInfo | null>(null);
  const [usage, setUsage] = useState<UsageStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);

  useEffect(() => {
    fetchBillingData();
  }, []);

  const fetchBillingData = async () => {
    try {
      const [plansRes, subRes, usageRes] = await Promise.all([
        fetch('/api/subscriptions/plans'),
        fetch('/api/subscriptions/current'),
        fetch('/api/subscriptions/usage'),
      ]);

      if (plansRes.ok) {
        const data = await plansRes.json();
        setPlans(data.data || []);
      }
      if (subRes.ok) {
        const data = await subRes.json();
        setSubscription(data.data);
      }
      if (usageRes.ok) {
        const data = await usageRes.json();
        setUsage(data.data);
      }
    } catch (error) {
      console.error('Error fetching billing data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleUpgrade = async (tier: number) => {
    setProcessing(tier.toString());
    try {
      const response = await fetch('/api/subscriptions/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newTier: tier }),
      });

      if (response.ok) {
        const data = await response.json();
        if (data.data.checkoutUrl) {
          // In production, redirect to Stripe checkout
          // window.location.href = data.data.checkoutUrl;
          alert('Checkout would redirect to: ' + data.data.checkoutUrl);
          fetchBillingData();
        }
      }
    } catch (error) {
      console.error('Error creating checkout:', error);
    } finally {
      setProcessing(null);
    }
  };

  const handleManageBilling = async () => {
    setProcessing('portal');
    try {
      const response = await fetch('/api/subscriptions/portal', {
        method: 'POST',
      });

      if (response.ok) {
        const data = await response.json();
        if (data.data.customerPortalUrl) {
          // In production, redirect to Stripe portal
          alert('Portal would redirect to: ' + data.data.customerPortalUrl);
        }
      }
    } catch (error) {
      console.error('Error creating portal:', error);
    } finally {
      setProcessing(null);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
      </div>
    );
  }

  const currentTier = subscription?.tier || 0;
  const tierInfo = TIERS[currentTier as keyof typeof TIERS] || TIERS[0];

  return (
    <div className="space-y-6">
      {/* Current Subscription */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Current Subscription</CardTitle>
              <CardDescription>
                Manage your subscription and billing
              </CardDescription>
            </div>
            <Badge className={tierInfo.color}>
              {tierInfo.name} Plan
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {usage && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Vehicles</span>
                  <span>{usage.vehicleCount} / {usage.vehicleLimit}</span>
                </div>
                <Progress value={usage.vehicleUsagePercent} />
                <p className="text-xs text-gray-500">
                  {Math.round(usage.vehicleUsagePercent)}% used
                </p>
              </div>
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Trips (this month)</span>
                  <span>{usage.tripCountCurrentMonth} / {usage.tripLimit}</span>
                </div>
                <Progress value={usage.tripUsagePercent} />
                <p className="text-xs text-gray-500">
                  {Math.round(usage.tripUsagePercent)}% used
                </p>
              </div>
            </div>
          )}

          {subscription?.currentPeriodEnd && (
            <p className="text-sm text-gray-500">
              Billing period ends: {new Date(subscription.currentPeriodEnd).toLocaleDateString()}
            </p>
          )}

          <Button onClick={handleManageBilling} disabled={processing !== null}>
            {processing === 'portal' ? (
              <Loader2 className="h-4 w-4 animate-spin mr-2" />
            ) : (
              <CreditCard className="h-4 w-4 mr-2" />
            )}
            Manage Billing
          </Button>
        </CardContent>
      </Card>

      {/* Available Plans */}
      <div>
        <h2 className="text-lg font-semibold mb-4">Available Plans</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {plans.map((plan) => {
            const isCurrent = plan.id === 'free' && currentTier === 0;
            const isPro = plan.id === 'pro' && currentTier === 1;
            const isBusiness = plan.id === 'business' && currentTier === 2;
            const isCurrentPlan = isCurrent || isPro || isBusiness;
            const isUpgrade = (currentTier === 0 && (plan.id === 'pro' || plan.id === 'business')) ||
                             (currentTier === 1 && plan.id === 'business');

            return (
              <Card key={plan.id} className={isCurrentPlan ? 'border-blue-500 border-2' : ''}>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      {plan.id === 'free' && <Zap className="h-5 w-5" />}
                      {plan.id === 'pro' && <Building2 className="h-5 w-5 text-blue-500" />}
                      {plan.id === 'business' && <Building2 className="h-5 w-5 text-purple-500" />}
                      {plan.name}
                    </CardTitle>
                    {isCurrentPlan && (
                      <Badge variant="secondary">Current</Badge>
                    )}
                  </div>
                  <CardDescription>
                    {plan.price === 0 ? (
                      <span className="text-2xl font-bold">Free</span>
                    ) : (
                      <span className="text-2xl font-bold">
                        ${plan.price.toFixed(2)}
                        <span className="text-sm font-normal text-gray-500">/{plan.interval}</span>
                      </span>
                    )}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-2">
                    {plan.features.map((feature, index) => (
                      <li key={index} className="flex items-center gap-2 text-sm">
                        <Check className="h-4 w-4 text-green-500 flex-shrink-0" />
                        <span>{feature}</span>
                      </li>
                    ))}
                  </ul>
                </CardContent>
                <CardFooter>
                  {isCurrentPlan ? (
                    <Button className="w-full" disabled>
                      Current Plan
                    </Button>
                  ) : (
                    <Button
                      className="w-full"
                      variant={isUpgrade ? 'default' : 'outline'}
                      onClick={() => handleUpgrade(
                        plan.id === 'pro' ? 1 : plan.id === 'business' ? 2 : 0
                      )}
                      disabled={processing !== null}
                    >
                      {processing === (plan.id === 'pro' ? '1' : '2') ? (
                        <Loader2 className="h-4 w-4 animate-spin mr-2" />
                      ) : isUpgrade ? (
                        'Upgrade'
                      ) : (
                        'Downgrade'
                      )}
                    </Button>
                  )}
                </CardFooter>
              </Card>
            );
          })}
        </div>
      </div>

      {/* Info Note */}
      <Card className="bg-blue-50 dark:bg-blue-900/10 border-blue-200 dark:border-blue-800">
        <CardContent className="py-4">
          <div className="flex items-start gap-3">
            <AlertCircle className="h-5 w-5 text-blue-500 mt-0.5" />
            <div className="text-sm text-blue-700 dark:text-blue-300">
              <p className="font-medium">Stripe Integration Coming Soon</p>
              <p className="mt-1">
                This is a demo UI. In production, clicking "Upgrade" would redirect 
                you to a secure Stripe checkout page to complete your purchase.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}