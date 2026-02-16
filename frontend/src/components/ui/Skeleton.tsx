import { cn } from '@/lib/utils';

interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'text' | 'circular' | 'rectangular';
}

export function Skeleton({ 
  className, 
  variant = 'rectangular',
  ...props 
}: SkeletonProps) {
  const variantClasses = {
    text: 'h-4 rounded',
    circular: 'rounded-full',
    rectangular: 'rounded-lg',
  };

  return (
    <div
      className={cn(
        'animate-pulse bg-muted',
        variantClasses[variant],
        className
      )}
      {...props}
    />
  );
}

// Skeleton card for dashboard stats
export function SkeletonCard({ className }: { className?: string }) {
  return (
    <div className={cn('p-6 space-y-3', className)}>
      <Skeleton variant="text" className="h-4 w-24" />
      <Skeleton variant="rectangular" className="h-8 w-full" />
      <Skeleton variant="text" className="h-3 w-16" />
    </div>
  );
}

// Skeleton table row
export function SkeletonTableRow({ columns = 5 }: { columns?: number }) {
  return (
    <tr className="border-b">
      {Array.from({ length: columns }).map((_, i) => (
        <td key={i} className="p-4">
          <Skeleton variant="rectangular" className="h-4 w-full" />
        </td>
      ))}
    </tr>
  );
}

// Skeleton table
export function SkeletonTable({ 
  rows = 5, 
  columns = 5,
  showHeader = true 
}: { 
  rows?: number; 
  columns?: number;
  showHeader?: boolean;
}) {
  return (
    <div className="space-y-4">
      {showHeader && (
        <div className="flex gap-4 p-4 border-b">
          {Array.from({ length: columns }).map((_, i) => (
            <Skeleton key={i} variant="text" className="h-4 w-20" />
          ))}
        </div>
      )}
      <div className="divide-y">
        {Array.from({ length: rows }).map((_, i) => (
          <SkeletonTableRow key={i} columns={columns} />
        ))}
      </div>
    </div>
  );
}

// Skeleton list
export function SkeletonList({ items = 3 }: { items?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: items }).map((_, i) => (
        <div key={i} className="flex items-center gap-3 p-3">
          <Skeleton variant="circular" className="h-10 w-10" />
          <div className="flex-1 space-y-2">
            <Skeleton variant="text" className="h-4 w-3/4" />
            <Skeleton variant="text" className="h-3 w-1/2" />
          </div>
        </div>
      ))}
    </div>
  );
}

// Skeleton form
export function SkeletonForm({ fields = 4 }: { fields?: number }) {
  return (
    <div className="space-y-4 p-6">
      {Array.from({ length: fields }).map((_, i) => (
        <div key={i} className="space-y-2">
          <Skeleton variant="text" className="h-4 w-24" />
          <Skeleton variant="rectangular" className="h-10 w-full" />
        </div>
      ))}
    </div>
  );
}

// Loading overlay
export function LoadingOverlay({ 
  children,
  isLoading,
  overlayClassName 
}: { 
  children: React.ReactNode;
  isLoading: boolean;
  overlayClassName?: string;
}) {
  return (
    <div className="relative">
      {children}
      {isLoading && (
        <div className={cn(
          'absolute inset-0 bg-background/80 backdrop-blur-sm',
          'flex items-center justify-center',
          overlayClassName
        )}>
          <LoadingSpinner />
        </div>
      )}
    </div>
  );
}

// Loading spinner
export function LoadingSpinner({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center justify-center', className)}>
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
    </div>
  );
}