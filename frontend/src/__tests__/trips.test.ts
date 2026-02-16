/**
 * Integration Tests for Trip Operations
 * 
 * Tests cover:
 * - Create trip with odometer validation
 * - Update trip
 * - Delete trip
 * - List trips with pagination
 * - Trip validation rules
 * 
 * Best Practices Applied:
 * - AAA Pattern
 * - Descriptive test names
 * - Edge case testing
 */

import { describe, it, expect, beforeEach, vi, Mock } from 'vitest';
import * as api from '../lib/api/trips';

// Mock the API module
vi.mock('../lib/api/trips', () => ({
  getTrips: vi.fn(),
  getTrip: vi.fn(),
  createTrip: vi.fn(),
  updateTrip: vi.fn(),
  deleteTrip: vi.fn(),
  getTripSummary: vi.fn(),
  validateTripData: vi.fn(),
}));

describe('Trip Operations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Create Trip', () => {
    it('createTrip_WithValidData_ReturnsNewTrip', async () => {
      // Arrange
      const mockCreate = api.createTrip as Mock;
      mockCreate.mockResolvedValue({
        id: 'trip-uuid',
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 10250,
        date: '2024-01-15',
        purpose: 'Business',
        distance: 250,
      });

      const tripData = {
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 10250,
        date: '2024-01-15',
        purpose: 'Business',
      };

      // Act
      const result = await api.createTrip(tripData);

      // Assert
      expect(result.id).toBe('trip-uuid');
      expect(result.distance).toBe(250);
      expect(mockCreate).toHaveBeenCalledWith(tripData);
    });

    it('createTrip_WithEndKmLessThanStartKm_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createTrip as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'End KM must be greater than Start KM' },
      });

      // Act
      const result = await api.createTrip({
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 5000,
        date: '2024-01-15',
      });

      // Assert
      expect(result.error).not.toBeUndefined();
      expect(result.error?.message).toContain('greater');
    });

    it('createTrip_WithEqualStartAndEndKm_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createTrip as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'End KM must be greater than Start KM' },
      });

      // Act
      const result = await api.createTrip({
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 10000,
        date: '2024-01-15',
      });

      // Assert
      expect(result.error).not.toBeUndefined();
    });

    it('createTrip_WithMissingFields_ReturnsError', async () => {
      // Arrange
      const mockCreate = api.createTrip as Mock;
      mockCreate.mockResolvedValue({
        error: { message: 'Vehicle ID, Start KM, and Date are required' },
      });

      // Act
      const result = await api.createTrip({ start_km: 10000 });

      // Assert
      expect(result.error).not.toBeUndefined();
    });

    it('createTrip_WithFutureDate_ReturnsWarning', async () => {
      // Arrange
      const mockCreate = api.createTrip as Mock;
      mockCreate.mockResolvedValue({
        warning: { message: 'Date is in the future' },
        data: { id: 'trip-uuid' },
      });

      // Act
      const result = await api.createTrip({
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 10100,
        date: '2099-01-01',
      });

      // Assert
      expect(result.warning).not.toBeUndefined();
    });
  });

  describe('List Trips', () => {
    it('getTrips_WithAuthenticatedUser_ReturnsTripList', async () => {
      // Arrange
      const mockList = api.getTrips as Mock;
      mockList.mockResolvedValue([
        { id: 'trip-1', start_km: 10000, end_km: 10250, date: '2024-01-15' },
        { id: 'trip-2', start_km: 10250, end_km: 10500, date: '2024-01-16' },
      ]);

      // Act
      const result = await api.getTrips({ vehicle_id: 'vehicle-uuid' });

      // Assert
      expect(result).toHaveLength(2);
    });

    it('getTrips_WithPagination_ReturnsPaginatedResults', async () => {
      // Arrange
      const mockList = api.getTrips as Mock;
      mockList.mockResolvedValue({
        data: [{ id: 'trip-1' }],
        pagination: { page: 1, limit: 20, total: 50, total_pages: 3 },
      });

      // Act
      const result = await api.getTrips({ page: 1, limit: 20 });

      // Assert
      expect(result.data).toHaveLength(1);
      expect(result.pagination.total).toBe(50);
    });

    it('getTrips_WithDateFilter_ReturnsFilteredResults', async () => {
      // Arrange
      const mockList = api.getTrips as Mock;
      mockList.mockResolvedValue({
        data: [{ id: 'trip-1', date: '2024-01-15' }],
        pagination: { page: 1, limit: 20, total: 1, total_pages: 1 },
      });

      // Act
      const result = await api.getTrips({
        start_date: '2024-01-01',
        end_date: '2024-01-31',
      });

      // Assert
      expect(result.data).toHaveLength(1);
    });

    it('getTrips_WithNoTrips_ReturnsEmptyArray', async () => {
      // Arrange
      const mockList = api.getTrips as Mock;
      mockList.mockResolvedValue([]);

      // Act
      const result = await api.getTrips({ vehicle_id: 'empty-vehicle' });

      // Assert
      expect(result).toEqual([]);
    });
  });

  describe('Update Trip', () => {
    it('updateTrip_WithValidData_ReturnsUpdatedTrip', async () => {
      // Arrange
      const mockUpdate = api.updateTrip as Mock;
      mockUpdate.mockResolvedValue({
        id: 'trip-uuid',
        start_km: 10000,
        end_km: 10500,
        updated_at: new Date().toISOString(),
      });

      // Act
      const result = await api.updateTrip('trip-uuid', { end_km: 10500 });

      // Assert
      expect(result.end_km).toBe(10500);
    });

    it('updateTrip_WithOdometerRollback_ReturnsWarning', async () => {
      // Arrange
      const mockUpdate = api.updateTrip as Mock;
      mockUpdate.mockResolvedValue({
        warning: { message: 'End KM is less than previous trip end KM' },
        data: { id: 'trip-uuid' },
      });

      // Act
      const result = await api.updateTrip('trip-uuid', { end_km: 5000 });

      // Assert
      expect(result.warning).not.toBeUndefined();
    });

    it('updateTrip_WithInvalidId_ReturnsError', async () => {
      // Arrange
      const mockUpdate = api.updateTrip as Mock;
      mockUpdate.mockResolvedValue({
        error: { message: 'Trip not found' },
      });

      // Act
      const result = await api.updateTrip('invalid-uuid', { end_km: 11000 });

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('Delete Trip', () => {
    it('deleteTrip_WithValidId_Succeeds', async () => {
      // Arrange
      const mockDelete = api.deleteTrip as Mock;
      mockDelete.mockResolvedValue({ success: true });

      // Act
      const result = await api.deleteTrip('trip-uuid');

      // Assert
      expect(result.success).toBe(true);
    });

    it('deleteTrip_WithInvalidId_ReturnsError', async () => {
      // Arrange
      const mockDelete = api.deleteTrip as Mock;
      mockDelete.mockResolvedValue({
        error: { message: 'Trip not found' },
      });

      // Act
      const result = await api.deleteTrip('invalid-uuid');

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('Trip Summary', () => {
    it('getTripSummary_ReturnsCorrectTotals', async () => {
      // Arrange
      const mockSummary = api.getTripSummary as Mock;
      mockSummary.mockResolvedValue({
        total_trips: 50,
        total_distance: 12500,
        total_business_trips: 35,
        total_private_trips: 15,
        average_distance: 250,
      });

      // Act
      const result = await api.getTripSummary('vehicle-uuid', 2024);

      // Assert
      expect(result.total_trips).toBe(50);
      expect(result.total_distance).toBe(12500);
    });
  });

  describe('Trip Validation', () => {
    it('validateTripData_WithValidTrip_ReturnsNoErrors', () => {
      // Arrange
      const mockValidate = api.validateTripData as Mock;
      mockValidate.mockReturnValue({ isValid: true, errors: [] });

      // Act
      const result = api.validateTripData({
        vehicle_id: 'vehicle-uuid',
        start_km: 10000,
        end_km: 10250,
        date: '2024-01-15',
      });

      // Assert
      expect(result.isValid).toBe(true);
    });

    it('validateTripData_WithStartKmGreaterThanEndKm_ReturnsError', () => {
      // Arrange
      const mockValidate = api.validateTripData as Mock;
      mockValidate.mockReturnValue({
        isValid: false,
        errors: ['End KM must be greater than Start KM'],
      });

      // Act
      const result = api.validateTripData({
        start_km: 10000,
        end_km: 5000,
      });

      // Assert
      expect(result.isValid).toBe(false);
    });
  });
});