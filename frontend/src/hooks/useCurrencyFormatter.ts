import { useCallback, useMemo } from 'react';

type Currency = 'EUR' | 'USD' | 'GBP' | 'CHF';

interface FormatCurrencyOptions {
  currency?: Currency;
  locale?: string;
  decimals?: number;
  showSymbol?: boolean;
  compact?: boolean;
}

interface CurrencyFormatter {
  format: (value: number | null | undefined, options?: Partial<FormatCurrencyOptions>) => string;
  formatCompact: (value: number | null | undefined) => string;
  parse: (value: string) => number;
}

const CURRENCY_SYMBOLS: Record<Currency, string> = {
  EUR: '€',
  USD: '$',
  GBP: '£',
  CHF: 'CHF ',
};

const DEFAULT_OPTIONS: Required<FormatCurrencyOptions> = {
  currency: 'EUR',
  locale: 'de-AT',
  decimals: 2,
  showSymbol: true,
  compact: false,
};

export function useCurrencyFormatter(
  initialOptions: Partial<FormatCurrencyOptions> = {}
): CurrencyFormatter {
  const options = { ...DEFAULT_OPTIONS, ...initialOptions };

  const format = useCallback((value: number | null | undefined, overrides?: Partial<FormatCurrencyOptions>): string => {
    if (value == null) return '-';
    
    const opts = { ...options, ...overrides };
    const { currency, locale, decimals, showSymbol, compact } = opts;
    
    if (compact) {
      return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency,
        notation: 'compact',
        maximumFractionDigits: 1,
      }).format(value);
    }

    const formatter = new Intl.NumberFormat(locale, {
      style: showSymbol ? 'currency' : 'decimal',
      currency,
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
      useGrouping: true,
    });

    return formatter.format(value);
  }, [options]);

  const formatCompact = useCallback((value: number | null | undefined): string => {
    return format(value, { compact: true });
  }, [format]);

  const parse = useCallback((value: string): number => {
    // Remove currency symbol and separators
    const cleaned = value
      .replace(/[^\d.,-]/g, '')
      .replace(/\./g, '')
      .replace(/,/g, '.');
    
    const parsed = parseFloat(cleaned);
    return isNaN(parsed) ? 0 : parsed;
  }, []);

  return {
    format,
    formatCompact,
    parse,
  };
}

// Format helpers for consistent usage
export function formatCurrency(value: number | null | undefined, currency: Currency = 'EUR'): string {
  if (value == null) return '-';
  
  return new Intl.NumberFormat('de-AT', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
    useGrouping: true,
  }).format(value);
}

export function formatKM(value: number | null | undefined): string {
  if (value == null) return '-';
  return `${value.toLocaleString('de-AT')} km`;
}

export function formatDistance(value: number): string {
  if (value < 1000) return `${value} m`;
  return `${(value / 1000).toFixed(1)} km`;
}

export function formatPercentage(value: number, decimals = 1): string {
  return `${value.toFixed(decimals)}%`;
}