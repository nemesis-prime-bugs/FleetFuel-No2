import { supabase } from '@/lib/supabase';

export interface YearlySummary {
  year: number;
  vehicle_summaries: VehicleSummary[];
  total_trips: number;
  total_business_trips: number;
  total_km: number;
  total_business_km: number;
  total_expenses: number;
  total_receipts: number;
}

export interface VehicleSummary {
  vehicle_id: string;
  vehicle_name: string;
  license_plate: string;
  trip_count: number;
  business_trip_count: number;
  total_km: number;
  business_km: number;
  total_expenses: number;
  receipt_count: number;
}

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

async function getAuthHeaders(): Promise<HeadersInit> {
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  return {
    'Authorization': `Bearer ${session.access_token}`,
    'Content-Type': 'application/json',
  };
}

export async function getYearlySummary(year: number): Promise<YearlySummary> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/summary?year=${year}`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch yearly summary');
  }

  return response.json();
}

export async function exportYearlySummaryCsv(year: number): Promise<Blob> {
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/summary/export?year=${year}`, {
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to export CSV');
  }

  return response.blob();
}