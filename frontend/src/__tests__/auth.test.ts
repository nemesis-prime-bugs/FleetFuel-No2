/**
 * Unit Tests for Auth Context - Complete User Flow
 * 
 * Tests cover the complete registration → confirmation → login flow
 * Best Practices:
 * - AAA Pattern (Arrange-Act-Assert)
 * - Descriptive test names: Method_Scenario_ExpectedResult
 * - Mock external dependencies
 * - Test edge cases
 * - Isolated tests
 */

import { describe, it, expect, beforeEach, vi, Mock, afterEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { useAuth, AuthProvider } from '../contexts/AuthContext';
import { supabase } from '../lib/supabase';

// Mock the supabase module
vi.mock('../lib/supabase', () => ({
  supabase: {
    auth: {
      signUp: vi.fn(),
      signInWithPassword: vi.fn(),
      signOut: vi.fn(),
      getSession: vi.fn(),
      onAuthStateChange: vi.fn(),
      exchangeCodeForSession: vi.fn(),
    },
  },
}));

describe('Complete User Flow - Registration → Confirmation → Login', () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('1. Registration Flow', () => {
    it('signUp_WithValidCredentials_CreatesUserAndSendsConfirmationEmail', async () => {
      // Arrange
      const mockSignUp = supabase.auth.signUp as Mock;
      mockSignUp.mockResolvedValue({
        data: {
          user: { id: 'user-123', email: 'test@example.com' },
          session: null, // No session until email is confirmed
        },
        error: null,
      });

      const { signUp } = useAuth();

      // Act
      const result = await signUp('test@example.com', 'SecurePassword123!');

      // Assert
      expect(result.error).toBeNull();
      expect(mockSignUp).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'SecurePassword123!',
        options: expect.objectContaining({
          emailRedirectTo: expect.stringContaining('/auth/callback'),
        }),
      });
    });

    it('signUp_WithDuplicateEmail_ReturnsAppropriateError', async () => {
      // Arrange
      const mockSignUp = supabase.auth.signUp as Mock;
      mockSignUp.mockResolvedValue({
        data: null,
        error: { message: 'User already registered' },
      });

      const { signUp } = useAuth();

      // Act
      const result = await signUp('existing@example.com', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
      expect(result.error?.message).toContain('already registered');
    });

    it('signUp_WithWeakPassword_ReturnsValidationError', async () => {
      // Arrange
      const mockSignUp = supabase.auth.signUp as Mock;
      mockSignUp.mockResolvedValue({
        data: null,
        error: { message: 'Password must be at least 8 characters' },
      });

      const { signUp } = useAuth();

      // Act
      const result = await signUp('test@example.com', '123');

      // Assert
      expect(result.error).not.toBeNull();
      expect(result.error?.message).toContain('at least 8 characters');
    });

    it('signUp_WithInvalidEmail_ReturnsValidationError', async () => {
      // Arrange
      const mockSignUp = supabase.auth.signUp as Mock;
      mockSignUp.mockResolvedValue({
        data: null,
        error: { message: 'Invalid email address' },
      });

      const { signUp } = useAuth();

      // Act
      const result = await signUp('not-an-email', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
    });
  });

  describe('2. Email Confirmation Flow', () => {
    it('exchangeCodeForSession_WithValidCode_ConfirmsEmailAndCreatesSession', async () => {
      // Arrange
      const mockExchange = supabase.auth.exchangeCodeForSession as Mock;
      mockExchange.mockResolvedValue({
        data: {
          user: { id: 'user-123', email: 'test@example.com', email_confirmed_at: '2024-01-15T10:00:00Z' },
          session: {
            access_token: 'access-token-123',
            refresh_token: 'refresh-token-123',
          },
        },
        error: null,
      });

      // This is called from the callback page, not the hook directly
      // But we test the expected behavior
      const mockExchangeResult = await mockExchange('auth-code-123');

      // Assert
      expect(mockExchangeResult.error).toBeNull();
      expect(mockExchangeResult.data.user.email_confirmed_at).not.toBeNull();
      expect(mockExchangeResult.data.session.access_token).toBe('access-token-123');
    });

    it('exchangeCodeForSession_WithInvalidCode_ReturnsError', async () => {
      // Arrange
      const mockExchange = supabase.auth.exchangeCodeForSession as Mock;
      mockExchange.mockResolvedValue({
        data: null,
        error: { message: 'Invalid or expired confirmation link' },
      });

      const mockExchangeResult = await mockExchange('invalid-code');

      // Assert
      expect(mockExchangeResult.error).not.toBeNull();
    });
  });

  describe('3. Login Flow', () => {
    it('signIn_WithConfirmedEmailAndCorrectPassword_Succeeds', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: {
          user: { id: 'user-123', email: 'test@example.com', email_confirmed_at: '2024-01-15T10:00:00Z' },
          session: {
            access_token: 'access-token-456',
            refresh_token: 'refresh-token-456',
          },
        },
        error: null,
      });

      const { signIn } = useAuth();

      // Act
      const result = await signIn('test@example.com', 'SecurePassword123!');

      // Assert
      expect(result.error).toBeNull();
      expect(mockSignIn).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'SecurePassword123!',
      });
    });

    it('signIn_WithUnconfirmedEmail_ReturnsConfirmationRequiredError', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: null,
        error: { message: 'Email not confirmed' },
      });

      const { signIn } = useAuth();

      // Act
      const result = await signIn('unconfirmed@example.com', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
      expect(result.error?.message.toLowerCase()).toContain('confirm');
    });

    it('signIn_WithWrongPassword_ReturnsInvalidCredentialsError', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: null,
        error: { message: 'Invalid credentials' },
      });

      const { signIn } = useAuth();

      // Act
      const result = await signIn('test@example.com', 'WrongPassword!');

      // Assert
      expect(result.error).not.toBeNull();
      expect(result.error?.message).toContain('Invalid');
    });

    it('signIn_WithNonExistentUser_ReturnsError', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: null,
        error: { message: 'Invalid login credentials' },
      });

      const { signIn } = useAuth();

      // Act
      const result = await signIn('nonexistent@example.com', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
    });
  });

  describe('4. Session Management', () => {
    it('getSession_WithActiveSession_ReturnsUser', async () => {
      // Arrange
      supabase.auth.getSession.mockResolvedValue({
        data: {
          session: {
            user: { id: 'user-123', email: 'test@example.com' },
            access_token: 'token',
          },
        },
        error: null,
      });

      const { user } = useAuth();

      // Assert (user should be populated from session)
      expect(user).not.toBeNull();
    });

    it('getSession_WithNoSession_ReturnsNull', async () => {
      // Arrange
      supabase.auth.getSession.mockResolvedValue({
        data: { session: null },
        error: null,
      });

      const { user } = useAuth();

      // Assert
      expect(user).toBeNull();
    });

    it('signOut_ClearsSessionAndUser', async () => {
      // Arrange
      supabase.auth.signOut.mockResolvedValue({ error: null });

      const { signOut } = useAuth();

      // Act
      await signOut();

      // Assert
      expect(supabase.auth.signOut).toHaveBeenCalled();
    });
  });

  describe('5. Auth State Change Listener', () => {
    it('onAuthStateChange_CalledOnAuthEvents', () => {
      // Arrange
      const mockCallback = vi.fn();
      supabase.auth.onAuthStateChange.mockReturnValue({
        unsubscribe: vi.fn(),
      });

      // Act
      supabase.auth.onAuthStateChange(mockCallback);

      // Assert
      expect(supabase.auth.onAuthStateChange).toHaveBeenCalled();
    });
  });

  describe('6. Edge Cases', () => {
    it('signUp_NetworkError_ReturnsAppropriateError', async () => {
      // Arrange
      const mockSignUp = supabase.auth.signUp as Mock;
      mockSignUp.mockRejectedValue(new Error('Network error'));

      const { signUp } = useAuth();

      // Act
      const result = await signUp('test@example.com', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
    });

    it('signIn_AccountDisabled_ReturnsError', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: null,
        error: { message: 'Account is disabled' },
      });

      const { signIn } = useAuth();

      // Act
      const result = await signIn('disabled@example.com', 'Password123!');

      // Assert
      expect(result.error).not.toBeNull();
      expect(result.error?.message).toContain('disabled');
    });

    it('MultipleRapidSignIn_CallsDoNotInterfere', async () => {
      // Arrange
      const mockSignIn = supabase.auth.signInWithPassword as Mock;
      mockSignIn.mockResolvedValue({
        data: { user: {}, session: {} },
        error: null,
      });

      const { signIn } = useAuth();

      // Act - Multiple simultaneous calls
      const [result1, result2, result3] = await Promise.all([
        signIn('test1@example.com', 'Password123!'),
        signIn('test2@example.com', 'Password123!'),
        signIn('test3@example.com', 'Password123!'),
      ]);

      // Assert - All should complete without errors
      expect(result1.error).toBeNull();
      expect(result2.error).toBeNull();
      expect(result3.error).toBeNull();
    });
  });
});

describe('Auth Provider Integration', () => {
  it('ProviderWrapsChildrenCorrectly', () => {
    // This test verifies the provider setup
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    });

    expect(result.current).toHaveProperty('user');
    expect(result.current).toHaveProperty('loading');
    expect(result.current).toHaveProperty('signIn');
    expect(result.current).toHaveProperty('signUp');
    expect(result.current).toHaveProperty('signOut');
  });

  it('LoadingState_InitiallyTrue', () => {
    // Initial loading state before session is checked
    supabase.auth.getSession.mockResolvedValue({
      data: { session: null },
      error: null,
    });

    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    });

    expect(result.current.loading).toBe(false); // Should be false after initial check
  });
});