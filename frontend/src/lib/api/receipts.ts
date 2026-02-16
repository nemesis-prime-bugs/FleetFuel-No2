import { supabase } from '@/lib/supabase';

export interface Receipt {
  id: string;
  user_id: string;
  vehicle_id: string;
  vehicle?: {
    id: string;
    name: string;
    license_plate: string;
  };
  date: string;
  amount: number;
  fuel_liters?: number;
  fuel_type?: string;
  station_name?: string;
  image_path?: string;
  created_at: string;
  modified_at: string;
  is_deleted: boolean;
}

export interface CreateReceiptRequest {
  vehicle_id: string;
  date: string;
  amount: number;
  fuel_liters?: number;
  fuel_type?: string;
  station_name?: string;
}

export interface UpdateReceiptRequest {
  date: string;
  amount: number;
  fuel_liters?: number;
  fuel_type?: string;
  station_name?: string;
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

export async function getReceipts(): Promise<Receipt[]> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch receipts');
  }

  return response.json();
}

export async function getReceipt(id: string): Promise<Receipt> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts/${id}`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch receipt');
  }

  return response.json();
}

export async function getReceiptsByVehicle(vehicleId: string): Promise<Receipt[]> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts/vehicle/${vehicleId}`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch receipts for vehicle');
  }

  return response.json();
}

export async function createReceipt(data: CreateReceiptRequest): Promise<Receipt> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts`, {
    method: 'POST',
    headers,
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to create receipt');
  }

  return response.json();
}

export async function updateReceipt(id: string, data: UpdateReceiptRequest): Promise<Receipt> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts/${id}`, {
    method: 'PUT',
    headers,
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to update receipt');
  }

  return response.json();
}

export async function deleteReceipt(id: string): Promise<void> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/receipts/${id}`, {
    method: 'DELETE',
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to delete receipt');
  }
}
