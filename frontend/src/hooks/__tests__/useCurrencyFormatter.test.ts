import { renderHook, act } from '@testing-library/react';
import { useCurrencyFormatter, formatCurrency, formatKM, formatDistance, formatPercentage } from '../hooks/useCurrencyFormatter';

describe('useCurrencyFormatter', () => {
  describe('format', () => {
    it('formats null as dash', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      expect(result.current.format(null)).toBe('-');
    });

    it('formats undefined as dash', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      expect(result.current.format(undefined)).toBe('-');
    });

    it('formats positive number with EUR symbol', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(1234.56);
      
      expect(formatted).toContain('€');
      expect(formatted).toContain('1.234,56');
    });

    it('formats zero correctly', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(0);
      
      expect(formatted).toContain('0,00');
    });

    it('formats with USD when specified', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(100, { currency: 'USD' });
      
      expect(formatted).toContain('$');
    });

    it('formats with GBP when specified', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(100, { currency: 'GBP' });
      
      expect(formatted).toContain('£');
    });

    it('formats with CHF when specified', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(100, { currency: 'CHF' });
      
      expect(formatted).toContain('CHF');
    });

    it('respects custom decimals setting', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(1234.567, { decimals: 3 });
      
      expect(formatted).toContain('1.234,567');
    });

    it('formats compact notation', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(1234567, { compact: true });
      
      expect(formatted).toMatch(/1,2.*€/);
    });

    it('formats without symbol when showSymbol false', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.format(100, { showSymbol: false });
      
      expect(formatted).not.toContain('€');
      expect(formatted).toContain('100');
    });
  });

  describe('formatCompact', () => {
    it('formats large numbers in compact notation', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const formatted = result.current.formatCompact(1000000);
      
      expect(formatted).toMatch(/1.*€/);
    });

    it('returns dash for null', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      expect(result.current.formatCompact(null)).toBe('-');
    });
  });

  describe('parse', () => {
    it('parses formatted currency back to number', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const parsed = result.current.parse('1.234,56 €');
      
      expect(parsed).toBe(1234.56);
    });

    it('handles USD format', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const parsed = result.current.parse('$1,234.56');
      
      expect(parsed).toBe(1234.56);
    });

    it('handles zero', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const parsed = result.current.parse('0,00 €');
      
      expect(parsed).toBe(0);
    });

    it('returns 0 for invalid input', () => {
      const { result } = renderHook(() => useCurrencyFormatter());
      const parsed = result.current.parse('not a number');
      
      expect(parsed).toBe(0);
    });
  });
});

describe('format helpers', () => {
  describe('formatCurrency', () => {
    it('formats EUR correctly', () => {
      expect(formatCurrency(1234.56)).toContain('€');
    });

    it('returns dash for null', () => {
      expect(formatCurrency(null)).toBe('-');
    });
  });

  describe('formatKM', () => {
    it('formats with km suffix', () => {
      expect(formatKM(12345)).toContain('km');
    });

    it('includes thousands separator', () => {
      expect(formatKM(10000)).toContain('10.000');
    });

    it('returns dash for null', () => {
      expect(formatKM(null)).toBe('-');
    });
  });

  describe('formatDistance', () => {
    it('shows meters for small values', () => {
      expect(formatDistance(500)).toContain('m');
    });

    it('shows km for larger values', () => {
      expect(formatDistance(1500)).toContain('km');
    });

    it('formats correctly', () => {
      expect(formatDistance(2000)).toBe('2,0 km');
    });
  });

  describe('formatPercentage', () => {
    it('formats with specified decimals', () => {
      expect(formatPercentage(67.89, 1)).toBe('67,9%');
    });

    it('defaults to 1 decimal', () => {
      expect(formatPercentage(50)).toBe('50,0%');
    });
  });
});