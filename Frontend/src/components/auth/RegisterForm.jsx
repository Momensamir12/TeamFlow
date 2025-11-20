import React, { useState } from 'react';
import { registerUser } from '../../api/authenticationApi';
import { UserPlus, Mail, Check, X } from 'lucide-react';

function RegisterForm({ onSuccess, onSwitchToLogin }) {
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    email: '',
    firstName: '',
    lastName: ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showSuccessMessage, setShowSuccessMessage] = useState(false);

  // Password validation rules
  const passwordValidation = {
    minLength: formData.password.length >= 8,
    hasUpperCase: /[A-Z]/.test(formData.password),
    hasLowerCase: /[a-z]/.test(formData.password),
    hasNumber: /[0-9]/.test(formData.password),
    hasSpecialChar: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(formData.password)
  };

  const isPasswordValid = Object.values(passwordValidation).every(v => v);

  const ValidationRule = ({ met, text }) => (
    <div className="flex items-center gap-2 text-sm">
      {met ? (
        <Check size={16} className="text-green-600" />
      ) : (
        <X size={16} className="text-gray-300" />
      )}
      <span className={met ? 'text-green-600' : 'text-gray-500'}>
        {text}
      </span>
    </div>
  );

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    // Validate password
    if (!isPasswordValid) {
      setError('Password does not meet all requirements');
      setLoading(false);
      return;
    }

    try {
      const result = await registerUser(formData);
      
      if (result.success) {
        setShowSuccessMessage(true);
      } else {
        setError(result.message || result.errors?.join(', ') || 'Registration failed');
      }
    } catch (err) {
      setError('Failed to connect to server. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field, value) => {
    setFormData({ ...formData, [field]: value });
  };

  return (
    <div>
      {showSuccessMessage ? (
        <div className="text-center">
          <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Mail className="text-green-600" size={40} />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-2">
            Check Your Email!
          </h2>
          <p className="text-gray-600 mb-6">
            We've sent a verification link to <strong>{formData.email}</strong>. 
            Please check your inbox and verify your email to complete registration.
          </p>
          <button
            onClick={onSwitchToLogin}
            className="w-full px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium"
          >
            Go to Login
          </button>
        </div>
      ) : (
        <div>
          {error && (
            <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              First Name *
            </label>
            <input
              type="text"
              value={formData.firstName}
              onChange={(e) => handleChange('firstName', e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              placeholder="John"
              required
              disabled={loading}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Last Name *
            </label>
            <input
              type="text"
              value={formData.lastName}
              onChange={(e) => handleChange('lastName', e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              placeholder="Doe"
              required
              disabled={loading}
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Username *
          </label>
          <input
            type="text"
            value={formData.username}
            onChange={(e) => handleChange('username', e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            placeholder="johndoe"
            required
            disabled={loading}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Email *
          </label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => handleChange('email', e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            placeholder="john@example.com"
            required
            disabled={loading}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Password *
          </label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => handleChange('password', e.target.value)}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${
              formData.password && isPasswordValid ? 'border-green-300' : 'border-gray-300'
            }`}
            placeholder="••••••••"
            required
            disabled={loading}
          />
          
          {formData.password && (
            <div className="mt-3 p-3 bg-gray-50 rounded-lg space-y-2">
              <p className="text-xs font-semibold text-gray-700 mb-2">Password Requirements:</p>
              <ValidationRule 
                met={passwordValidation.minLength} 
                text="At least 8 characters" 
              />
              <ValidationRule 
                met={passwordValidation.hasUpperCase} 
                text="Contains an uppercase letter (A-Z)" 
              />
              <ValidationRule 
                met={passwordValidation.hasLowerCase} 
                text="Contains a lowercase letter (a-z)" 
              />
              <ValidationRule 
                met={passwordValidation.hasNumber} 
                text="Contains a number (0-9)" 
              />
              <ValidationRule 
                met={passwordValidation.hasSpecialChar} 
                text="Contains a special character (!@#$%^&* etc.)" 
              />
            </div>
          )}
        </div>

        <button
          type="submit"
          disabled={loading || !isPasswordValid}
          className="w-full bg-indigo-600 text-white py-3 rounded-lg hover:bg-indigo-700 transition-colors font-medium flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <UserPlus size={20} />
          {loading ? 'Creating Account...' : 'Sign Up'}
        </button>
      </form>

      <p className="text-center text-sm text-gray-600 mt-4">
        Already have an account?{' '}
        <button
          onClick={onSwitchToLogin}
          className="text-indigo-600 hover:text-indigo-700 font-medium"
        >
          Sign in
        </button>
      </p>
        </div>
      )}
    </div>
  );
}

export default RegisterForm;