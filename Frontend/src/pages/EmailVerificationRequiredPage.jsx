import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, CheckCircle, AlertCircle, Workflow } from 'lucide-react';
import axios from 'axios';
import { API_BASE_URL } from '../constants/config';

function EmailVerificationRequiredPage({ onLogout }) {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  
  const user = JSON.parse(sessionStorage.getItem('user') || '{}');
  const email = user.email;

  const handleResendEmail = async () => {
    if (!email) {
      setError('Email not found. Please login again.');
      return;
    }

    setLoading(true);
    setError('');
    setMessage('');

    try {
      const response = await axios.post(`${API_BASE_URL}/auth/resend-verification`, {
        email: email
      });

      if (response.data.success) {
        setMessage('Verification email sent! Please check your inbox.');
      } else {
        setError(response.data.message || 'Failed to send verification email');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to send verification email');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    // Save invitation token if it exists
    const invitationToken = sessionStorage.getItem('invitationToken');
    sessionStorage.clear();
    if (invitationToken) {
      sessionStorage.setItem('invitationToken', invitationToken);
    }
    // Call the parent logout handler to update App state
    if (onLogout) {
      onLogout();
    }
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-500 via-purple-500 to-pink-500 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-md w-full">
        <div className="flex flex-col items-center">
          {/* TeamFlow Logo */}
          <div className="w-16 h-16 bg-gradient-to-br from-indigo-600 to-purple-600 rounded-2xl flex items-center justify-center mb-4 shadow-lg">
            <Workflow size={32} className="text-white" strokeWidth={2.5} />
          </div>
          <h1 className="text-xl font-bold text-gray-900 mb-4 text-center">TeamFlow</h1>
          
          {/* Email Icon */}
          <div className="w-20 h-20 bg-yellow-100 rounded-full flex items-center justify-center mb-6">
            <Mail className="text-yellow-600" size={40} />
          </div>
          
          <h2 className="text-2xl font-bold text-gray-900 mb-2 text-center">
            Verify Your Email
          </h2>
          
          <p className="text-gray-600 text-center mb-6">
            We sent a verification email to <strong>{email}</strong>. 
            Please check your inbox and click the verification link to access your account.
          </p>

          {message && (
            <div className="w-full mb-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3">
              <CheckCircle className="text-green-600 flex-shrink-0 mt-0.5" size={20} />
              <p className="text-green-800 text-sm">{message}</p>
            </div>
          )}

          {error && (
            <div className="w-full mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
              <AlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={20} />
              <p className="text-red-800 text-sm">{error}</p>
            </div>
          )}

          <div className="w-full space-y-3">
            <button
              onClick={handleResendEmail}
              disabled={loading}
              className="w-full px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Sending...' : 'Resend Verification Email'}
            </button>

            <button
              onClick={handleLogout}
              className="w-full px-6 py-3 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
            >
              Back to Login
            </button>
          </div>

          <p className="text-sm text-gray-500 mt-6 text-center">
            Didn't receive the email? Check your spam folder or click the resend button above.
          </p>
        </div>
      </div>
    </div>
  );
}

export default EmailVerificationRequiredPage;
