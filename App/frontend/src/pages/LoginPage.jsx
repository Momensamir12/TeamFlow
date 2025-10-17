import React, { useState } from 'react';
import LoginForm from '../components/auth/LoginForm';
import RegisterForm from '../components/auth/RegisterForm';

function LoginPage({ onLogin }) {
  const [isRegistering, setIsRegistering] = useState(false);

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
            onSuccess={onLogin}
            onSwitchToRegister={() => setIsRegistering(true)}
          />
        )}
      </div>
    </div>
  );
}

export default LoginPage;