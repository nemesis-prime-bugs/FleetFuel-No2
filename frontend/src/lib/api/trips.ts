import { supabase } from '@/lib/supabase';

export interface Trip {
  id: string;
  user_id: string;
  vehicle_id: string;
  vehicle?: {
    id: string;
    name: string;
    license_plate: string;
  };
  date: string;
  start_km: number;
  end_km: number;
  purpose?: string;
  is_business: boolean;
  created_at: string;
  modified_at: string;
  is_deleted: boolean;
}

export interface CreateTripRequest {
  vehicle_id: string;
  date: string;
  start_km: number;
  end_km: number;
  purpose?: string;
  is_business: boolean;
}

export interface UpdateTripRequest {
  date: string;
  start_km: number;
  end_km: number;
  purpose?: string;
  is_business: boolean;
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

export async function getTrips(): Promise<Trip[]> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch trips');
  }

  return response.json();
}

export async function getTrip(id: string): Promise<Trip> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips/${id}`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch trip');
  }

  return response.json();
}

export async function getTripsByVehicle(vehicleId: string): Promise<Trip[]> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips/vehicle/${vehicleId}`, {
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to fetch trips for vehicle');
  }

  return response.json();
}

export async function createTrip(data: CreateTripRequest): Promise<Trip> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips`, {
    method: 'POST',
    headers,
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to create trip');
  }

  return response.json();
}

export async function updateTrip(id: string, data: UpdateTripRequest): Promise<Trip> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips/${id}`, {
    method: 'PUT',
    headers,
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to update trip');
  }

  return response.json();
}

export async function deleteTrip(id: string): Promise<void> {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_URL}/api/v1/trips/${id}`, {
    method: 'DELETE',
    headers,
  });

  if (!response.ok) {
    throw new Error('Failed to delete trip');
  }
}
