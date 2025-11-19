import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useSearchParams } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import VerifyEmailPage from './pages/VerifyEmailPage';
import EmailVerificationRequiredPage from './pages/EmailVerificationRequiredPage';
import AcceptInvitationPage from './pages/AcceptInvitationPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';

function LoginRoute({ isLoggedIn, emailVerified, onLogin }) {
  const [searchParams] = useSearchParams();
  const hasRedirect = searchParams.has('redirect');
  
  if (isLoggedIn && !hasRedirect) {
    // Only auto-navigate if there's no pending redirect
    return emailVerified ? <Navigate to="/dashboard" /> : <Navigate to="/verify-email-required" />;
  }
  
  return <LoginPage onLogin={onLogin} />;
}

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [emailVerified, setEmailVerified] = useState(true);

  useEffect(() => {
    const token = sessionStorage.getItem('accessToken');
    if (token) {
      setIsLoggedIn(true);
      
      // Check if email verification is required
      const user = JSON.parse(sessionStorage.getItem('user') || '{}');
      const isVerified = sessionStorage.getItem('emailVerified') === 'true';
      setEmailVerified(isVerified);
    }
  }, []);

  const handleLogin = (verified) => {
    setIsLoggedIn(true);
    setEmailVerified(verified);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setEmailVerified(true);
  };

  return (
    <Router>
      <div className="App">
        <Routes>
          <Route path="/verify-email" element={<VerifyEmailPage />} />
          <Route path="/accept-invitation" element={<AcceptInvitationPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          
          <Route 
            path="/login" 
            element={<LoginRoute isLoggedIn={isLoggedIn} emailVerified={emailVerified} onLogin={handleLogin} />}
          />
          
          <Route 
            path="/verify-email-required" 
            element={
              isLoggedIn && !emailVerified ? (
                <EmailVerificationRequiredPage onLogout={handleLogout} />
              ) : (
                <Navigate to={isLoggedIn ? "/dashboard" : "/login"} />
              )
            } 
          />
          
          <Route 
            path="/dashboard" 
            element={
              isLoggedIn ? (
                emailVerified ? (
                  <DashboardPage onLogout={() => { setIsLoggedIn(false); setEmailVerified(true); }} />
                ) : (
                  <Navigate to="/verify-email-required" />
                )
              ) : (
                <Navigate to="/login" />
              )
            } 
          />
          
          <Route path="/" element={<Navigate to={isLoggedIn ? "/dashboard" : "/login"} />} />
          <Route path="*" element={<Navigate to={isLoggedIn ? "/dashboard" : "/login"} />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;