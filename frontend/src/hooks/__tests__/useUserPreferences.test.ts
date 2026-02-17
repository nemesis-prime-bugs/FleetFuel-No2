/**
 * Unit Tests for useUserPreferences hook
 */

import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useUserPreferences } from '../useUserPreferences';

// Mock fetch
global.fetch = vi.fn();

describe('useUserPreferences', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Clear localStorage
    localStorage.clear();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('Loading State', () => {
    it('should start with loading true', async () => {
      // Arrange
      (global.fetch as any).mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve({ ok: true, json: () => Promise.resolve({ data: {} }) }), 100))
      );

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert - initially loading
      expect(result.current.loading).toBe(true);
    });

    it('should have loading false after data loads', async () => {
      // Arrange
      (global.fetch as any).mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ data: { currency: 'USD', theme: 'dark' } }),
      });

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert
      await waitFor(() => {
        expect(result.current.loading).toBe(false);
      });
    });
  });

  describe('Default Preferences', () => {
    it('should return default preferences when not authenticated', async () => {
      // Arrange
      (global.fetch as any).mockResolvedValue({
        ok: false,
        status: 401,
      });

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert
      await waitFor(() => {
        expect(result.current.preferences.currency).toBe('USD');
        expect(result.current.preferences.distanceUnit).toBe('km');
        expect(result.current.preferences.theme).toBe('system');
      });
    });

    it('should have correct default values', async () => {
      // Arrange
      (global.fetch as any).mockResolvedValue({
        ok: false,
        status: 401,
      });

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert
      await waitFor(() => {
        expect(result.current.preferences).toEqual({
          currency: 'USD',
          distanceUnit: 'km',
          volumeUnit: 'L',
          fuelEfficiencyUnit: 'L/100km',
          temperatureUnit: 'C',
          dateFormat: 'DD/MM/YYYY',
          theme: 'system',
          timezone: 'UTC',
        });
      });
    });
  });

  describe('Fetching Preferences', () => {
    it('should fetch preferences from API', async () => {
      // Arrange
      const mockPreferences = {
        currency: 'EUR',
        distanceUnit: 'mi',
        theme: 'dark',
      };
      (global.fetch as any).mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ data: mockPreferences }),
      });

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert
      await waitFor(() => {
        expect(result.current.preferences.currency).toBe('EUR');
        expect(result.current.preferences.distanceUnit).toBe('mi');
        expect(result.current.preferences.theme).toBe('dark');
      });

      expect(global.fetch).toHaveBeenCalledWith('/api/user/preferences');
    });

    it('should handle fetch error gracefully', async () => {
      // Arrange
      (global.fetch as any).mockRejectedValue(new Error('Network error'));

      // Act
      const { result } = renderHook(() => useUserPreferences());

      // Assert
      await waitFor(() => {
        expect(result.current.error).toBe('Network error');
        expect(result.current.preferences.currency).toBe('USD'); // Defaults
      });
    });
  });

  describe('Updating Preferences', () => {
    it('should call API when updating preferences', async () => {
      // Arrange
      (global.fetch as any)
        .mockResolvedValueOnce({
          ok: true,
          json: () => Promise.resolve({ data: { currency: 'USD' } }),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: () => Promise.resolve({ data: { currency: 'EUR' } }),
        });

      const { result } = renderHook(() => useUserPreferences());

      // Wait for initial load
      await waitFor(() => {
        expect(result.current.loading).toBe(false);
      });

      // Act
      const success = await result.current.updatePreferences({ currency: 'EUR' });

      // Assert
      expect(success).toBe(true);
      expect(result.current.preferences.currency).toBe('EUR');
      expect(global.fetch).toHaveBeenCalledTimes(2);
      expect(global.fetch).toHaveBeenLastCalledWith(
        '/api/user/preferences',
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify({ preferences: expect.objectContaining({ currency: 'EUR' }) }),
        })
      );
    });

    it('should return false when update fails', async () => {
      // Arrange
      (global.fetch as any)
        .mockResolvedValueOnce({
          ok: true,
          json: () => Promise.resolve({ data: { currency: 'USD' } }),
        })
        .mockResolvedValueOnce({
          ok: false,
          status: 500,
        });

      const { result } = renderHook(() => useUserPreferences());

      await waitFor(() => {
        expect(result.current.loading).toBe(false);
      });

      // Act
      const success = await result.current.updatePreferences({ currency: 'EUR' });

      // Assert
      expect(success).toBe(false);
      expect(result.current.error).toBeDefined();
    });

    it('should update multiple preferences at once', async () => {
      // Arrange
      (global.fetch as any)
        .mockResolvedValueOnce({
          ok: true,
          json: () => Promise.resolve({ data: { currency: 'USD', distanceUnit: 'km' } }),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: () => Promise.resolve({ 
            data: { currency: 'EUR', distanceUnit: 'mi', theme: 'dark' } 
          }),
        });

      const { result } = renderHook(() => useUserPreferences());

      await waitFor(() => {
        expect(result.current.loading).toBe(false);
      });

      // Act
      const success = await result.current.updatePreferences({
        currency: 'EUR',
        distanceUnit: 'mi',
        theme: 'dark',
      });

      // Assert
      expect(success).toBe(true);
      expect(result.current.preferences.currency).toBe('EUR');
      expect(result.current.preferences.distanceUnit).toBe('mi');
      expect(result.current.preferences.theme).toBe('dark');
    });
  });

  describe('Refresh', () => {
    it('should refetch preferences when refresh is called', async () => {
      // Arrange
      (global.fetch as any).mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ data: { currency: 'USD' } }),
      });

      const { result, rerender } = renderHook(() => useUserPreferences());

      await waitFor(() => {
        expect(result.current.loading).toBe(false);
      });

      // Update mock for second call
      (global.fetch as any).mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ data: { currency: 'GBP' } }),
      });

      // Act
      await result.current.refresh();

      // Rerender to pick up new fetch
      rerender();

      await waitFor(() => {
        expect(global.fetch).toHaveBeenCalledTimes(2);
      });
    });
  });
});