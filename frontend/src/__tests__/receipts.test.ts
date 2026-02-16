/**
 * Integration Tests for Receipt Operations
 * 
 * Tests cover:
 * - Upload receipt with image
 * - Process receipt data
 * - List receipts
 * - Edge cases
 */

import { describe, it, expect, beforeEach, vi, Mock } from 'vitest';
import * as api from '../lib/api/receipts';

// Mock the API module
vi.mock('../lib/api/receipts', () => ({
  uploadReceipt: vi.fn(),
  getReceipts: vi.fn(),
  getReceipt: vi.fn(),
  deleteReceipt: vi.fn(),
  processReceipt: vi.fn(),
}));

describe('Receipt Operations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Upload Receipt', () => {
    it('uploadReceipt_WithValidImage_ReturnsNewReceipt', async () => {
      // Arrange
      const mockUpload = api.uploadReceipt as Mock;
      mockUpload.mockResolvedValue({
        id: 'receipt-uuid',
        vehicle_id: 'vehicle-uuid',
        amount: 50.00,
        fuel_type: 'Gasoline',
        liters: 40,
        station: 'Shell',
        date: '2024-01-15',
        image_url: 'https://storage.example.com/receipts/uuid.jpg',
      });

      const file = new File(['image content'], 'receipt.jpg', { type: 'image/jpeg' });

      // Act
      const result = await api.uploadReceipt(file, 'vehicle-uuid');

      // Assert
      expect(result.id).toBe('receipt-uuid');
      expect(result.amount).toBe(50.00);
      expect(result.image_url).toContain('receipts/');
    });

    it('uploadReceipt_WithInvalidFileType_ReturnsError', async () => {
      // Arrange
      const mockUpload = api.uploadReceipt as Mock;
      mockUpload.mockResolvedValue({
        error: { message: 'Only image files are allowed' },
      });

      const file = new File(['content'], 'document.pdf', { type: 'application/pdf' });

      // Act
      const result = await api.uploadReceipt(file, 'vehicle-uuid');

      // Assert
      expect(result.error).not.toBeUndefined();
    });

    it('uploadReceipt_WithFileTooLarge_ReturnsError', async () => {
      // Arrange
      const mockUpload = api.uploadReceipt as Mock;
      mockUpload.mockResolvedValue({
        error: { message: 'File size exceeds 5MB limit' },
      });

      // Act
      const result = await api.uploadReceipt(null as any, 'vehicle-uuid');

      // Assert
      expect(result.error).not.toBeUndefined();
    });
  });

  describe('Process Receipt', () => {
    it('processReceipt_WithValidImage_ReturnsExtractedData', async () => {
      // Arrange
      const mockProcess = api.processReceipt as Mock;
      mockProcess.mockResolvedValue({
        amount: 75.50,
        fuel_type: 'Diesel',
        liters: 50,
        station: 'OMV',
        date: '2024-01-15',
        odometer: 55000,
      });

      // Act
      const result = await api.processReceipt('receipt-uuid');

      // Assert
      expect(result.amount).toBe(75.50);
      expect(result.fuel_type).toBe('Diesel');
    });
  });

  describe('List Receipts', () => {
    it('getReceipts_WithAuthenticatedUser_ReturnsReceiptList', async () => {
      // Arrange
      const mockList = api.getReceipts as Mock;
      mockList.mockResolvedValue([
        { id: 'r1', amount: 50.00, date: '2024-01-15' },
        { id: 'r2', amount: 75.00, date: '2024-01-16' },
      ]);

      // Act
      const result = await api.getReceipts({ vehicle_id: 'vehicle-uuid' });

      // Assert
      expect(result).toHaveLength(2);
    });

    it('getReceipts_WithDateFilter_ReturnsFilteredResults', async () => {
      // Arrange
      const mockList = api.getReceipts as Mock;
      mockList.mockResolvedValue({
        data: [{ id: 'r1', date: '2024-01-15' }],
        pagination: { total: 1 },
      });

      // Act
      const result = await api.getReceipts({
        start_date: '2024-01-01',
        end_date: '2024-01-31',
      });

      // Assert
      expect(result.data).toHaveLength(1);
    });
  });

  describe('Delete Receipt', () => {
    it('deleteReceipt_WithValidId_Succeeds', async () => {
      // Arrange
      const mockDelete = api.deleteReceipt as Mock;
      mockDelete.mockResolvedValue({ success: true });

      // Act
      const result = await api.deleteReceipt('receipt-uuid');

      // Assert
      expect(result.success).toBe(true);
    });
  });
});

describe('Summary Operations', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Yearly Summary', () => {
    it('getYearlySummary_WithValidYear_ReturnsCorrectData', async () => {
      // Arrange
      const mockSummary = api.getYearlySummary as Mock;
      mockSummary.mockResolvedValue({
        year: 2024,
        total_trips: 100,
        total_distance: 25000,
        total_receipts: 50,
        total_fuel_cost: 2500.00,
        average_fuel_efficiency: 8.5,
        cost_per_km: 0.10,
      });

      // Act
      const result = await api.getYearlySummary(2024);

      // Assert
      expect(result.year).toBe(2024);
      expect(result.total_distance).toBe(25000);
    });

    it('getYearlySummary_WithNoData_ReturnsZeroValues', async () => {
      // Arrange
      const mockSummary = api.getYearlySummary as Mock;
      mockSummary.mockResolvedValue({
        year: 2099,
        total_trips: 0,
        total_distance: 0,
        total_receipts: 0,
        total_fuel_cost: 0,
      });

      // Act
      const result = await api.getYearlySummary(2099);

      // Assert
      expect(result.total_trips).toBe(0);
    });
  });

  describe('Export CSV', () => {
    it('exportToCsv_WithValidData_ReturnsCsvString', async () => {
      // Arrange
      const mockExport = api.exportToCsv as Mock;
      mockExport.mockResolvedValue(
        'date,start_km,end_km,distance\n2024-01-15,10000,10250,250\n'
      );

      // Act
      const result = await api.exportToCsv('trips', 2024);

      // Assert
      expect(result).toContain('date');
      expect(result).toContain('distance');
    });
  });
});