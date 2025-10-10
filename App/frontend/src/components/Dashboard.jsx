import React from 'react';
import { useAuth } from '../hooks/useAuth';

const Dashboard = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Welcome to Your Dashboard</h1>
        <button onClick={handleLogout} className="logout-btn">
          Logout
        </button>
      </header>
      
      <main className="dashboard-content">
        <div className="welcome-card">
          <h2>Hello!</h2>
          <p>You have successfully logged in to your account.</p>
          <div className="user-info">
            <h3>Your Information:</h3>
            <p><strong>Tokens are stored securely</strong></p>
            <p>Access Token: {user?.tokens?.accessToken ? '✓ Present' : '✗ Missing'}</p>
            <p>Refresh Token: {user?.tokens?.refreshToken ? '✓ Present' : '✗ Missing'}</p>
          </div>
        </div>
        
        <div className="features">
          <h3>What you can do next:</h3>
          <ul>
            <li>Add your application features here</li>
            <li>Create protected API calls</li>
            <li>Build your main application components</li>
            <li>Add user profile management</li>
          </ul>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;