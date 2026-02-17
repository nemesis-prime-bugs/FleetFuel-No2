import { supabase } from '@/lib/supabase';

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

// Use production API URL, fallback to localhost for development
const getApiUrl = () => {
  if (typeof window !== 'undefined') {
    // In browser, use production URL or current origin for API calls
    const prodUrl = 'https://fleetfuel.onrender.com';
    const envUrl = process.env.NEXT_PUBLIC_API_URL;
    const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    
    // If we're on localhost and no env URL, use localhost
    if (isLocalhost && (!envUrl || envUrl.includes('localhost'))) {
      return envUrl || 'http://localhost:5000';
    }
    
    // Otherwise use the production URL or the env URL
    return envUrl || prodUrl;
  }
  return 'http://localhost:5000';
};

const API_URL = getApiUrl();

async function getAuthHeaders(): Promise<HeadersInit> {
  const { data: { session } } = await supabase.auth.getSession();
  
  if (!session) {
    console.warn('No Supabase session found - user may not be logged in');
    throw new Error('Session not found. Please log in again and retry.');
  }

  if (!session.access_token) {
    console.warn('Session exists but no access_token');
    throw new Error('Authentication token missing. Please log in again.');
  }

  return {
    'Authorization': `Bearer ${session.access_token}`,
    'Content-Type': 'application/json',
  };
}

function getErrorMessage(response: Response): string {
  try {
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      const error = await response.json();
      // Try different error formats
      if (error.detail) return error.detail;
      if (error.error) return typeof error.error === 'string' ? error.error : error.error.message || 'An error occurred';
      if (error.message) return error.message;
      if (error.Data?.Error) return error.Data.Error;
      return JSON.stringify(error);
    }
  } catch {
    // Fall through to default message
  }
  
  const statusMessages: Record<number, string> = {
    400: 'Invalid request. Please check your input.',
    401: 'Your session has expired. Please log in again.',
    403: 'You do not have permission to perform this action.',
    404: 'The requested resource was not found.',
    422: 'Validation error. Please check your input.',
    429: 'Too many requests. Please try again later.',
    500: 'Server error. Please try again later.',
    503: 'Service temporarily unavailable.',
  };
  
  return statusMessages[response.status] || `Request failed (${response.status})`;
}

export async function getVehicles(): Promise<Vehicle[]> {
  try {
    const response = await fetch(`${API_URL}/api/v1/vehicles`, {
      headers: await getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(getErrorMessage(response));
    }

    const result = await response.json();
    return result.Data || [];
  } catch (error) {
    console.error('Failed to fetch vehicles:', error);
    throw error instanceof Error ? error : new Error('Failed to load vehicles');
  }
}

export async function getVehicle(id: string): Promise<Vehicle> {
  try {
    const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
      headers: await getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(getErrorMessage(response));
    }

    const result = await response.json();
    return result.Data;
  } catch (error) {
    console.error('Failed to fetch vehicle:', error);
    throw error instanceof Error ? error : new Error('Failed to load vehicle');
  }
}

export async function createVehicle(data: CreateVehicleRequest): Promise<Vehicle> {
  try {
    const response = await fetch(`${API_URL}/api/v1/vehicles`, {
      method: 'POST',
      headers: await getAuthHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorMsg = getErrorMessage(response);
      console.error('Create vehicle failed:', { status: response.status, error: errorMsg });
      throw new Error(errorMsg);
    }

    const result = await response.json();
    console.log('Vehicle created successfully:', result.Data);
    return result.Data;
  } catch (error) {
    console.error('Failed to create vehicle:', error);
    if (error instanceof Error) {
      throw error;
    }
    throw new Error('Failed to create vehicle. Please check your connection and try again.');
  }
}

export async function updateVehicle(id: string, data: UpdateVehicleRequest): Promise<Vehicle> {
  try {
    const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
      method: 'PUT',
      headers: await getAuthHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(getErrorMessage(response));
    }

    const result = await response.json();
    return result.Data;
  } catch (error) {
    console.error('Failed to update vehicle:', error);
    throw error instanceof Error ? error : new Error('Failed to update vehicle');
  }
}

export async function deleteVehicle(id: string): Promise<void> {
  try {
    const response = await fetch(`${API_URL}/api/v1/vehicles/${id}`, {
      method: 'DELETE',
      headers: await getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(getErrorMessage(response));
    }
  } catch (error) {
    console.error('Failed to delete vehicle:', error);
    throw error instanceof Error ? error : new Error('Failed to delete vehicle');
  }
}
