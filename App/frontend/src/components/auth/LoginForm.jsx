import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { loginUser } from '../../api/authenticationApi';
import { LogIn } from 'lucide-react';

function LoginForm({ onSuccess, onSwitchToRegister }) {
  const [formData, setFormData] = useState({ username: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const result = await loginUser(formData.username, formData.password);
      
      if (result.success && result.data) {
        // Save invitation token before clearing session
        const savedInvitationToken = sessionStorage.getItem('invitationToken');
        
        // Clear all cached data from previous session
        sessionStorage.clear();
        
        // Store tokens
        sessionStorage.setItem('accessToken', result.data.accessToken);
        sessionStorage.setItem('refreshToken', result.data.refreshToken);
        
        // Store user details
        const userData = {
          id: result.data.userId,
          username: result.data.userName,
          email: result.data.email
        };
        sessionStorage.setItem('user', JSON.stringify(userData));
        
        // Store email verification status
        sessionStorage.setItem('emailVerified', result.data.isEmailVerified.toString());
        
        // Restore invitation token if it existed
        if (savedInvitationToken) {
          sessionStorage.setItem('invitationToken', savedInvitationToken);
        }
        
        onSuccess(result.data.isEmailVerified);
      } else {
        setError(result.message || 'Login failed');
      }
    } catch (err) {
      setError('Failed to connect to server');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-900 mb-2">Welcome Back</h1>

      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Username
          </label>
          <input
            type="text"
            value={formData.username}
            onChange={(e) => setFormData({ ...formData, username: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
            disabled={loading}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Password
          </label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
            disabled={loading}
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-indigo-600 text-white py-3 rounded-lg hover:bg-indigo-700 transition-colors font-medium flex items-center justify-center gap-2"
        >
          <LogIn size={20} />
          {loading ? 'Signing in...' : 'Sign In'}
        </button>

        <div className="text-center">
          <button
            type="button"
            onClick={() => navigate('/forgot-password')}
            className="text-sm text-indigo-600 hover:text-indigo-700 font-medium"
          >
            Forgot password?
          </button>
        </div>
      </form>

      <p className="text-center text-sm text-gray-600 mt-4">
        Don't have an account?{' '}
        <button
          onClick={onSwitchToRegister}
          className="text-indigo-600 hover:text-indigo-700 font-medium"
        >
          Sign up
        </button>
      </p>
    </div>
  );
}

export default LoginForm;