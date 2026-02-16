import { useState, useCallback } from 'react';

interface PaginationState {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

interface PaginationActions {
  nextPage: () => void;
  prevPage: () => void;
  goToPage: (page: number) => void;
  setTotal: (total: number) => void;
}

interface PaginationReturn extends PaginationState {
  hasNextPage: boolean;
  hasPrevPage: boolean;
  isFirstPage: boolean;
  isLastPage: boolean;
  startIndex: number;
  endIndex: number;
  actions: PaginationActions;
}

const DEFAULT_LIMIT = 20;
const MAX_LIMIT = 100;

export function usePagination(
  initialPage = 1,
  initialLimit = DEFAULT_LIMIT
): PaginationReturn {
  const [page, setPage] = useState(initialPage);
  const [limit, setLimit] = useState(Math.min(initialLimit, MAX_LIMIT));
  const [total, setTotal] = useState(0);

  const totalPages = Math.ceil(total / limit) || 0;

  const hasNextPage = page < totalPages;
  const hasPrevPage = page > 1;
  const isFirstPage = page === 1;
  const isLastPage = page >= totalPages;
  const startIndex = (page - 1) * limit;
  const endIndex = Math.min(page * limit, total);

  const nextPage = useCallback(() => {
    if (hasNextPage) {
      setPage((p) => p + 1);
    }
  }, [hasNextPage]);

  const prevPage = useCallback(() => {
    if (hasPrevPage) {
      setPage((p) => p - 1);
    }
  }, [hasPrevPage]);

  const goToPage = useCallback((newPage: number) => {
    const validPage = Math.max(1, Math.min(newPage, totalPages));
    setPage(validPage);
  }, [totalPages]);

  const setTotalCallback = useCallback((newTotal: number) => {
    setTotal(newTotal);
    // Adjust page if it's now beyond total pages
    setPage((currentPage) => 
      Math.min(currentPage, Math.ceil(newTotal / limit)) || 1
    );
  }, [limit]);

  return {
    page,
    limit,
    total,
    totalPages,
    hasNextPage,
    hasPrevPage,
    isFirstPage,
    isLastPage,
    startIndex,
    endIndex,
    actions: {
      nextPage,
      prevPage,
      goToPage: goToPage,
      setTotal: setTotalCallback,
    },
  };
}

// Hook for fetching paginated data
export function usePaginatedQuery<T>(
  queryFn: (page: number, limit: number) => Promise<{ data: T[]; total: number }>,
  deps: unknown[] = []
) {
  const pagination = usePagination();
  const [data, setData] = useState<T[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await queryFn(pagination.page, pagination.limit);
      setData(result.data);
      pagination.actions.setTotal(result.total);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Unknown error'));
    } finally {
      setIsLoading(false);
    }
  }, [queryFn, pagination.page, pagination.limit]);

  return {
    ...pagination,
    data,
    isLoading,
    error,
    refetch: fetchData,
  };
}