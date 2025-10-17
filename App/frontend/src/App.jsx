import React, { useState, useEffect } from 'react';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    const token = sessionStorage.getItem('accessToken');
    if (token) setIsLoggedIn(true);
  }, []);

  return (
    <div className="App">
      {isLoggedIn ? (
        <DashboardPage onLogout={() => setIsLoggedIn(false)} />
      ) : (
        <LoginPage onLogin={() => setIsLoggedIn(true)} />
      )}
    </div>
  );
}

export default App;