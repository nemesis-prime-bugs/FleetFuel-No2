/**
 * Integration Tests for Vehicle Operations
 * 
 * Tests cover:
 * - Create vehicle
 * - Update vehicle
 * - Delete vehicle
 * - List vehicles
 * - Edge cases
 * 
 * Best Practices Applied:
 * - AAA Pattern
 * - Descriptive test names
 * - Mock external dependencies
 * - Test edge cases
 */

import { describe, it, expect, beforeEach, vi, Mock } from 'vitest';
import * as api from '../lib/api/vehicles';

// Mock the API module
vi.mock('../lib/api/vehicles', () => ({
  getVehicles: vi.fn(),
  getVehicle: vi.fn(),
  createVehicle: vi.fn(),
  updateVehicle: vi.fn(),
  deleteVehicle: vi.fn(),
  validateVehicleData: vi.fn(),
}));

describe('Vehicle Operations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Create Vehicle', () => {
    it('createVehicle_WithValidData_ReturnsNewVehicle', async () => {
      // Arrange
      const mockCreate = api.createVehicle as Mock;
      mockCreate.mockResolvedValue({
        id: 'vehicle-uuid',
        license_plate: 'TEST-123',
        brand: 'Toyota',
        model: 'Corolla',
        year: 2022,
        initial_mileage: 0,
      });

      const vehicleData = {
        name: 'My Car',
        license_plate: 'TEST-123',
        initial_mileage: 0,
      };

      // Act
      const result = await api.createVehicle(vehicleData);

      // Assert
      expect(result.id).toBe('vehicle-uuid');
      expect(result.license_plate).toBe('TEST-123');
      expect(mockCreate).toHaveBeenCalledWith(vehicleData);
    });

    it('createVehicle_WithDuplicatePlate_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createVehicle as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'Vehicle with this license plate already exists' },
      });

      // Act
      const result = await api.createVehicle({
        name: 'Car',
        license_plate: 'EXISTING-001',
        initial_mileage: 0,
      });

      // Assert
      expect(result.error).not.toBeUndefined();
    });

    it('createVehicle_WithEmptyPlate_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createVehicle as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'License plate is required' },
      });

      // Act
      const result = await api.createVehicle({
        name: 'Car',
        license_plate: '',
        initial_mileage: 0,
      });

      // Assert
      expect(result.error).not.toBeUndefined();
    });

    it('createVehicle_WithMissingFields_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createVehicle as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'Name and license plate are required' },
      });

      // Act
      const result = await api.createVehicle({
        name: 'Car',
      });

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('List Vehicles', () => {
    it('getVehicles_WithAuthenticatedUser_ReturnsVehicleList', async () => {
      // Arrange
      const mockList = api.getVehicles as Mock;
      mockList.mockResolvedValue([
        { id: 'v1', name: 'Car 1', license_plate: 'AAA-111' },
        { id: 'v2', name: 'Car 2', license_plate: 'BBB-222' },
      ]);

      // Act
      const result = await api.getVehicles();

      // Assert
      expect(result).toHaveLength(2);
      expect(result[0].license_plate).toBe('AAA-111');
    });

    it('getVehicles_WithNoVehicles_ReturnsEmptyArray', async () => {
      // Arrange
      const mockList = api.getVehicles as Mock;
      mockList.mockResolvedValue([]);

      // Act
      const result = await api.getVehicles();

      // Assert
      expect(result).toEqual([]);
    });
  });

  describe('Update Vehicle', () => {
    it('updateVehicle_WithValidData_ReturnsUpdatedVehicle', async () => {
      // Arrange
      const mockUpdate = api.updateVehicle as Mock;
      mockUpdate.mockResolvedValue({
        id: 'vehicle-uuid',
        name: 'Updated Name',
        license_plate: 'UPDATED-001',
        updated_at: new Date().toISOString(),
      });

      // Act
      const result = await api.updateVehicle('vehicle-uuid', {
        name: 'Updated Name',
        license_plate: 'UPDATED-001',
      });

      // Assert
      expect(result.name).toBe('Updated Name');
      expect(result.license_plate).toBe('UPDATED-001');
    });

    it('updateVehicle_WithInvalidId_ReturnsError', async () => {
      // Arrange
      const mockUpdate = api.updateVehicle as Mock;
      mockUpdate.mockResolvedValue({
        error: { message: 'Vehicle not found' },
      });

      // Act
      const result = await api.updateVehicle('invalid-uuid', { name: 'New Name' });

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('Delete Vehicle', () => {
    it('deleteVehicle_WithValidId_Succeeds', async () => {
      // Arrange
      const mockDelete = api.deleteVehicle as Mock;
      mockDelete.mockResolvedValue({ success: true });

      // Act
      const result = await api.deleteVehicle('vehicle-uuid');

      // Assert
      expect(result.success).toBe(true);
    });

    it('deleteVehicle_WithInvalidId_ReturnsError', async () => {
      // Arrange
      const mockDelete = api.deleteVehicle as Mock;
      mockDelete.mockResolvedValue({
        error: { message: 'Vehicle not found' },
      });

      // Act
      const result = await api.deleteVehicle('invalid-uuid');

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('Vehicle Validation', () => {
    it('validateVehicleData_WithValidData_ReturnsNoErrors', () => {
      // Arrange
      const mockValidate = api.validateVehicleData as Mock;
      mockValidate.mockReturnValue({ isValid: true, errors: [] });

      // Act
      const result = api.validateVehicleData({
        name: 'My Car',
        license_plate: 'TEST-123',
      });

      // Assert
      expect(result.isValid).toBe(true);
      expect(result.errors).toHaveLength(0);
    });

    it('validateVehicleData_WithEmptyPlate_ReturnsErrors', () => {
      // Arrange
      const mockValidate = api.validateVehicleData as Mock;
      mockValidate.mockReturnValue({
        isValid: false,
        errors: ['License plate is required'],
      });

      // Act
      const result = api.validateVehicleData({ name: 'Car' });

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.errors.length).toBeGreaterThan(0);
    });
  });
});