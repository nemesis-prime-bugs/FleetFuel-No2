import { useEffect, useState } from 'react';
import { Button } from '@/components/ui/button';

export function SkipLink() {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Tab') {
        setIsVisible(true);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  return (
    <a
      href="#main-content"
      className={`
        fixed top-4 left-4 z-50
        px-4 py-2 bg-primary text-primary-foreground
        rounded-md font-medium
        transform transition-all duration-200
        focus:outline-none focus:ring-2 focus:ring-offset-2
        ${isVisible ? 'translate-y-0 opacity-100' : '-translate-y-12 opacity-0'}
      `}
      aria-label="Skip to main content"
    >
      Skip to main content
    </a>
  );
}

// Focus trap for modals
export function FocusTrap({ 
  children,
  active = true 
}: { 
  children: React.ReactNode;
  active?: boolean;
}) {
  return (
    <div tabIndex={-1} aria-hidden={!active}>
      {children}
    </div>
  );
}

// Live region for announcements
export function LiveRegion({ 
  children,
  priority = 'polite' 
}: { 
  children: React.ReactNode;
  priority?: 'polite' | 'assertive';
}) {
  return (
    <div 
      role="status" 
      aria-live={priority}
      aria-atomic="true"
      className="sr-only"
    >
      {children}
    </div>
  );
}

// Error message with proper ARIA
export function ErrorMessage({ 
  id,
  children 
}: { 
  id: string;
  children: React.ReactNode;
}) {
  return (
    <p 
      id={id} 
      role="alert" 
      className="text-sm text-destructive mt-1"
    >
      {children}
    </p>
  );
}

// Success message with announcement
export function SuccessAnnouncement({ children }: { children: React.ReactNode }) {
  return (
    <div role="status" aria-live="polite" className="sr-only">
      {children}
    </div>
  );
}