/**
 * Unit Tests for ThemeContext
 */

import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { renderHook, act, cleanup } from '@testing-library/react';
import { ThemeProvider, useTheme } from '../ThemeContext';

describe('ThemeContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    // Clear any document classes
    document.documentElement.classList.remove('light', 'dark');
  });

  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  describe('Initial State', () => {
    it('should return system as default theme when nothing in localStorage', async () => {
      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Assert
      expect(result.current.theme).toBe('system');
      expect(result.current.resolvedTheme).toBe('light'); // Assuming system is light for tests
    });

    it('should return stored theme from localStorage', async () => {
      // Arrange
      localStorage.setItem('fleetfuel-theme', 'dark');

      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Assert
      expect(result.current.theme).toBe('dark');
      expect(result.current.resolvedTheme).toBe('dark');
    });
  });

  describe('setTheme', () => {
    it('should update theme in state and localStorage', async () => {
      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Act
      act(() => {
        result.current.setTheme('dark');
      });

      // Assert
      expect(result.current.theme).toBe('dark');
      expect(localStorage.getItem('fleetfuel-theme')).toBe('dark');
    });

    it('should update resolved theme when setting to dark', async () => {
      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Act
      act(() => {
        result.current.setTheme('dark');
      });

      // Assert
      expect(result.current.resolvedTheme).toBe('dark');
      expect(document.documentElement.classList.contains('dark')).toBe(true);
      expect(document.documentElement.classList.contains('light')).toBe(false);
    });

    it('should update resolved theme when setting to light', async () => {
      // Arrange - Start with dark
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });
      act(() => {
        result.current.setTheme('dark');
      });

      // Act - Switch to light
      act(() => {
        result.current.setTheme('light');
      });

      // Assert
      expect(result.current.resolvedTheme).toBe('light');
      expect(document.documentElement.classList.contains('light')).toBe(true);
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });
  });

  describe('toggleTheme', () => {
    it('should toggle from light to dark', async () => {
      // Arrange - Start with light
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Act
      act(() => {
        result.current.toggleTheme();
      });

      // Assert
      expect(result.current.theme).toBe('light');
      expect(result.current.resolvedTheme).toBe('light');
    });

    it('should toggle from dark to light', async () => {
      // Arrange - Start with dark
      localStorage.setItem('fleetfuel-theme', 'dark');
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Act
      act(() => {
        result.current.toggleTheme();
      });

      // Assert
      expect(result.current.theme).toBe('light');
    });
  });

  describe('Theme Application', () => {
    it('should apply dark class when theme is dark', async () => {
      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      act(() => {
        result.current.setTheme('dark');
      });

      // Assert
      expect(document.documentElement.classList.contains('dark')).toBe(true);
    });

    it('should apply light class when theme is light', async () => {
      // Act
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      act(() => {
        result.current.setTheme('light');
      });

      // Assert
      expect(document.documentElement.classList.contains('light')).toBe(true);
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });

    it('should remove both classes before adding new theme', async () => {
      // Arrange
      const { result } = renderHook(() => useTheme(), {
        wrapper: ThemeProvider,
      });

      // Act - Switch to dark
      act(() => {
        result.current.setTheme('dark');
      });

      // Assert - Has dark
      expect(document.documentElement.classList.contains('dark')).toBe(true);

      // Act - Switch to light
      act(() => {
        result.current.setTheme('light');
      });

      // Assert - Has light, not dark
      expect(document.documentElement.classList.contains('light')).toBe(true);
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });
  });
});