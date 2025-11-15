import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import LoginForm from '../components/auth/LoginForm';
import RegisterForm from '../components/auth/RegisterForm';

function LoginPage({ onLogin }) {
  const [isRegistering, setIsRegistering] = useState(false);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  const handleLoginSuccess = (emailVerified) => {
    // Check if there's a redirect parameter (from invitation)
    const redirect = searchParams.get('redirect');
    const token = searchParams.get('token');
    
    // Always call onLogin first to update auth state
    onLogin(emailVerified);
    
    if (redirect && token) {
      // Navigate to invitation acceptance
      // Use setTimeout to ensure state update completes first
      setTimeout(() => {
        navigate(`${redirect}?token=${token}`, { replace: true });
      }, 0);
    }
    // If no redirect, the Route's Navigate component will handle navigation to dashboard
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md">
        {isRegistering ? (
          <RegisterForm 
            onSuccess={() => setIsRegistering(false)} 
            onSwitchToLogin={() => setIsRegistering(false)}
          />
        ) : (
          <LoginForm 
            onSuccess={handleLoginSuccess}
            onSwitchToRegister={() => setIsRegistering(true)}
          />
        )}
      </div>
    </div>
  );
}

export default LoginPage;