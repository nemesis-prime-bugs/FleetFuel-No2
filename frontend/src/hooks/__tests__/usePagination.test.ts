import { renderHook, waitFor } from '@testing-library/react';
import { usePagination, usePaginatedQuery } from '../hooks/usePagination';

describe('usePagination', () => {
  it('initializes with default values', () => {
    const { result } = renderHook(() => usePagination());

    expect(result.current.page).toBe(1);
    expect(result.current.limit).toBe(20);
    expect(result.current.total).toBe(0);
    expect(result.current.totalPages).toBe(0);
    expect(result.current.hasNextPage).toBe(false);
    expect(result.current.hasPrevPage).toBe(false);
  });

  it('initializes with custom values', () => {
    const { result } = renderHook(() => usePagination(3, 50));

    expect(result.current.page).toBe(3);
    expect(result.current.limit).toBe(50);
  });

  it('limits max limit to 100', () => {
    const { result } = renderHook(() => usePagination(1, 200));

    expect(result.current.limit).toBe(100);
  });

  describe('nextPage', () => {
    it('increments page when has next page', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      result.current.actions.nextPage();

      expect(result.current.page).toBe(2);
      expect(result.current.hasNextPage).toBe(true);
    });

    it('does not increment when on last page', () => {
      const { result } = renderHook(() => usePagination(5, 20, 100));

      result.current.actions.nextPage();

      expect(result.current.page).toBe(5);
    });
  });

  describe('prevPage', () => {
    it('decrements page when has previous page', () => {
      const { result } = renderHook(() => usePagination(3, 20, 100));

      result.current.actions.prevPage();

      expect(result.current.page).toBe(2);
    });

    it('does not decrement when on first page', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      result.current.actions.prevPage();

      expect(result.current.page).toBe(1);
    });
  });

  describe('goToPage', () => {
    it('goes to valid page', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      result.current.actions.goToPage(5);

      expect(result.current.page).toBe(5);
    });

    it('clamps to first page for invalid page', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      result.current.actions.goToPage(0);

      expect(result.current.page).toBe(1);
    });

    it('clamps to last page for page beyond total', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      result.current.actions.goToPage(100);

      expect(result.current.page).toBe(5);
    });
  });

  describe('setTotal', () => {
    it('updates total and calculates pages', () => {
      const { result } = renderHook(() => usePagination(1, 20));

      result.current.actions.setTotal(100);

      expect(result.current.total).toBe(100);
      expect(result.current.totalPages).toBe(5);
      expect(result.current.hasNextPage).toBe(true);
    });

    it('adjusts page if beyond new total', () => {
      const { result } = renderHook(() => usePagination(5, 20, 200));

      result.current.actions.setTotal(50);

      expect(result.current.page).toBe(3);
    });
  });

  describe('computed values', () => {
    it('calculates startIndex correctly', () => {
      const { result } = renderHook(() => usePagination(3, 20, 100));

      expect(result.current.startIndex).toBe(40);
    });

    it('calculates endIndex correctly', () => {
      const { result } = renderHook(() => usePagination(2, 20, 100));

      expect(result.current.endIndex).toBe(40);
    });

    it('identifies first page correctly', () => {
      const { result } = renderHook(() => usePagination(1, 20, 100));

      expect(result.current.isFirstPage).toBe(true);
      expect(result.current.isLastPage).toBe(false);
    });

    it('identifies last page correctly', () => {
      const { result } = renderHook(() => usePagination(5, 20, 100));

      expect(result.current.isFirstPage).toBe(false);
      expect(result.current.isLastPage).toBe(true);
    });
  });
});

describe('usePaginatedQuery', () => {
  it('fetches data on mount', async () => {
    const mockQueryFn = jest.fn().mockResolvedValue({
      data: [{ id: 1 }, { id: 2 }],
      total: 2,
    });

    const { result } = renderHook(() => usePaginatedQuery(mockQueryFn));

    await waitFor(() => {
      expect(result.current.data).toHaveLength(2);
      expect(result.current.total).toBe(2);
    });
  });

  it('sets loading state initially', () => {
    const mockQueryFn = jest.fn().mockImplementation(() => 
      new Promise(resolve => setTimeout(() => resolve({ data: [], total: 0 }), 100))
    );

    const { result } = renderHook(() => usePaginatedQuery(mockQueryFn));

    expect(result.current.isLoading).toBe(true);
  });

  it('refetches when page changes', async () => {
    const mockQueryFn = jest.fn()
      .mockResolvedValueOnce({ data: [{ id: 1 }], total: 2 })
      .mockResolvedValueOnce({ data: [{ id: 2 }], total: 2 });

    const { result } = renderHook(() => usePaginatedQuery(mockQueryFn));

    await waitFor(() => {
      expect(result.current.data).toHaveLength(1);
    });

    result.current.actions.nextPage();

    await waitFor(() => {
      expect(mockQueryFn).toHaveBeenCalledTimes(2);
    });
  });
});