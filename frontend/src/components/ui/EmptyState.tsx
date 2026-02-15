import { ReactNode } from 'react';
import Link from 'next/link';

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
    <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
      {icon && (
        <div className="mb-4 text-gray-400">
          {icon}
        </div>
      )}
      <h3 className="text-lg font-medium text-gray-900 mb-2">
        {title}
      </h3>
      <p className="text-sm text-gray-500 mb-6 max-w-sm">
        {description}
      </p>
      {action && (
        action.href ? (
          <Link
            href={action.href}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
          >
            {action.label}
          </Link>
        ) : (
          <button
            onClick={action.onClick}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
          >
            {action.label}
          </button>
        )
      )}
    </div>
  );
}

// Pre-defined empty states for common use cases
export function NoVehiclesEmptyState() {
  return (
    <EmptyState
      title="No vehicles yet"
      description="Add your first vehicle to start tracking trips and fuel expenses."
      action={{ label: '+ Add Vehicle', href: '/vehicles/new' }}
    />
  );
}

export function NoTripsEmptyState() {
  return (
    <EmptyState
      title="No trips yet"
      description="Log your first trip to begin tracking your vehicle usage."
      action={{ label: '+ Log Trip', href: '/trips/new' }}
    />
  );
}

export function NoReceiptsEmptyState() {
  return (
    <EmptyState
      title="No receipts yet"
      description="Upload your first fuel receipt to begin tracking expenses."
      action={{ label: '+ Upload Receipt', href: '/receipts/upload' }}
    />
  );
}