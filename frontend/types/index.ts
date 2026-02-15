// User types
export interface User {
  id: string;
  email: string;
  createdAt: string;
  updatedAt: string;
}

// Vehicle types
export interface Vehicle {
  id: string;
  userId: string;
  name: string;
  licensePlate: string;
  initialMileage: number;
  createdAt: string;
  modifiedAt: string;
  isDeleted: boolean;
}

export interface CreateVehicleRequest {
  name: string;
  licensePlate: string;
  initialMileage: number;
}

// Trip types
export interface Trip {
  id: string;
  userId: string;
  vehicleId: string;
  date: string;
  startKm: number;
  endKm: number;
  calculatedKm: number;
  purpose?: string;
  isBusiness: boolean;
  createdAt: string;
  modifiedAt: string;
  isDeleted: boolean;
}

export interface CreateTripRequest {
  vehicleId: string;
  date: string;
  startKm: number;
  endKm: number;
  purpose?: string;
  isBusiness: boolean;
}

// Receipt types
export interface Receipt {
  id: string;
  userId: string;
  vehicleId: string;
  date: string;
  amount: number;
  fuelLiters?: number;
  fuelType?: string;
  stationName?: string;
  imagePath?: string;
  createdAt: string;
  modifiedAt: string;
  isDeleted: boolean;
}

export interface CreateReceiptRequest {
  vehicleId: string;
  date: string;
  amount: number;
  fuelLiters?: number;
  fuelType?: string;
  stationName?: string;
  image?: File;
}

// Summary types
export interface VehicleSummary {
  vehicleId: string;
  vehicleName: string;
  totalKm: number;
  totalBusinessKm: number;
  totalFuelCost: number;
  receiptCount: number;
}

export interface YearlySummary {
  year: number;
  vehicles: VehicleSummary[];
}

// API Response types
export interface ApiResponse<T> {
  data?: T;
  error?: string;
  statusCode?: number;
}
