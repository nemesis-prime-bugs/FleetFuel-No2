import { ReactNode } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Plus } from 'lucide-react';

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description: string;
  action?: {
    label: string;
    href?: string;
    onClick?: () => void;
  };
}

export function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <Card className="border-dashed">
      <CardContent className="flex flex-col items-center justify-center py-12 px-4 text-center">
        {icon && (
          <div className="mb-4 text-muted-foreground">
            {icon}
          </div>
        )}
        <h3 className="text-lg font-semibold mb-2">
          {title}
        </h3>
        <p className="text-sm text-muted-foreground mb-6 max-w-sm">
          {description}
        </p>
        {action && (
          action.href ? (
            <Link href={action.href}>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                {action.label}
              </Button>
            </Link>
          ) : (
            <Button onClick={action.onClick}>
              <Plus className="h-4 w-4 mr-2" />
              {action.label}
            </Button>
          )
        )}
      </CardContent>
    </Card>
  );
}

// Pre-defined empty states for common use cases
export function NoVehiclesEmptyState() {
  return (
    <EmptyState
      title="No vehicles yet"
      description="Add your first vehicle to start tracking trips and fuel expenses."
      action={{ label: 'Add Vehicle', href: '/vehicles/new' }}
    />
  );
}

export function NoTripsEmptyState() {
  return (
    <EmptyState
      title="No trips yet"
      description="Log your first trip to begin tracking your vehicle usage."
      action={{ label: 'Log Trip', href: '/trips/new' }}
    />
  );
}

export function NoReceiptsEmptyState() {
  return (
    <EmptyState
      title="No receipts yet"
      description="Upload your first fuel receipt to begin tracking expenses."
      action={{ label: 'Upload Receipt', href: '/receipts/new' }}
    />
  );
}