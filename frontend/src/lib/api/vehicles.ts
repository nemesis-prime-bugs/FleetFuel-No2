import { createClient } from '@/lib/supabase/client';

export interface Vehicle {
  id: string;
  user_id: string;
  name: string;
  license_plate: string;
  initial_mileage: number;
  created_at: string;
  modified_at: string;
  is_deleted: boolean;
}

export interface CreateVehicleRequest {
  name: string;
  license_plate: string;
  initial_mileage: number;
}

export interface UpdateVehicleRequest {
  name: string;
  license_plate: string;
  initial_mileage: number;
}

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export async function getVehicles(): Promise<Vehicle[]> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/vehicles`, {
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to fetch vehicles');
  }

  return response.json();
}

export async function getVehicle(id: string): Promise<Vehicle> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to fetch vehicle');
  }

  return response.json();
}

export async function createVehicle(data: CreateVehicleRequest): Promise<Vehicle> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/vehicles`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to create vehicle');
  }

  return response.json();
}

export async function updateVehicle(id: string, data: UpdateVehicleRequest): Promise<Vehicle> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to update vehicle');
  }

  return response.json();
}

export async function deleteVehicle(id: string): Promise<void> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) throw new Error('Not authenticated');

  const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${session.access_token}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to delete vehicle');
  }
}