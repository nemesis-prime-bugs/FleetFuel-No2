import { useCallback, useEffect } from 'react';

type KeyboardHandler = (event: KeyboardEvent) => void;

/**
 * Hook to handle keyboard navigation within a component.
 * Implements typical arrow key navigation patterns.
 */
export function useKeyboardNavigation(
  options: {
    onArrowUp?: () => void;
    onArrowDown?: () => void;
    onArrowLeft?: () => void;
    onArrowRight?: () => void;
    onEnter?: () => void;
    onEscape?: () => void;
    onTab?: () => void;
    disabled?: boolean;
  }
) {
  const {
    onArrowUp,
    onArrowDown,
    onArrowLeft,
    onArrowRight,
    onEnter,
    onEscape,
    onTab,
    disabled = false,
  } = options;

  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (disabled) return;

      switch (event.key) {
        case 'ArrowUp':
          onArrowUp?.();
          event.preventDefault();
          break;
        case 'ArrowDown':
          onArrowDown?.();
          event.preventDefault();
          break;
        case 'ArrowLeft':
          onArrowLeft?.();
          event.preventDefault();
          break;
        case 'ArrowRight':
          onArrowRight?.();
          event.preventDefault();
          break;
        case 'Enter':
          onEnter?.();
          event.preventDefault();
          break;
        case 'Escape':
          onEscape?.();
          event.preventDefault();
          break;
        case 'Tab':
          onTab?.();
          break;
      }
    },
    [onArrowUp, onArrowDown, onArrowLeft, onArrowRight, onEnter, onEscape, onTab, disabled]
  );

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);
}

/**
 * Hook to manage focus trap within a container.
 */
export function useFocusTrap(containerRef: React.RefObject<HTMLElement>, active = true) {
  useEffect(() => {
    if (!active || !containerRef.current) return;

    const container = containerRef.current;
    const focusableElements = container.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return;

      if (e.shiftKey) {
        if (document.activeElement === firstElement) {
          lastElement?.focus();
          e.preventDefault();
        }
      } else {
        if (document.activeElement === lastElement) {
          firstElement?.focus();
          e.preventDefault();
        }
      }
    };

    container.addEventListener('keydown', handleKeyDown);
    firstElement?.focus();

    return () => container.removeEventListener('keydown', handleKeyDown);
  }, [containerRef, active]);
}

/**
 * Hook for roving tabindex in list-like components.
 */
export function useRovingTabIndex(
  items: { id: string }[],
  initialIndex = 0
) {
  const getTabIndex = useCallback(
    (index: number, currentIndex: number) => (index === currentIndex ? 0 : -1),
    []
  );

  return { getTabIndex, initialIndex };
}

/**
 * Hook to announce changes to screen readers.
 */
export function useAnnounce() {
  const announce = useCallback((message: string, priority = 'polite') => {
    const announcement = document.createElement('div');
    announcement.setAttribute('role', 'status');
    announcement.setAttribute('aria-live', priority);
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;
    
    document.body.appendChild(announcement);
    
    setTimeout(() => {
      document.body.removeChild(announcement);
    }, 1000);
  }, []);

  return announce;
}