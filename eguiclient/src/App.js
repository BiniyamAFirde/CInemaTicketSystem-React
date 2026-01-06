import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import apiService from './services/api';

// Import components
import Login from './components/Login';
import Register from './components/Register';
import ScreeningList from './components/ScreeningList';
import SeatSelection from './components/SeatSelection';
import AdminPanel from './components/AdminPanel';
import UserProfile from './components/UserProfile';

function App() {
  const [currentUser, setCurrentUser] = useState(apiService.getCurrentUser());
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Check if user is still logged in
    const user = apiService.getCurrentUser();
    setCurrentUser(user);
  }, []);

  const handleLogin = (user) => {
    setCurrentUser(user);
  };

  const handleLogout = () => {
    apiService.logout();
    setCurrentUser(null);
  };

  const PrivateRoute = ({ children, adminOnly = false }) => {
    if (!currentUser) {
      return <Navigate to="/login" replace />;
    }

    if (adminOnly && !currentUser.isAdmin) {
      return <Navigate to="/screenings" replace />;
    }

    return children;
  };

  return (
    <BrowserRouter>
      <Routes>
        <Route 
          path="/login" 
          element={
            currentUser ? <Navigate to="/screenings" /> : <Login onLogin={handleLogin} />
          } 
        />
        <Route 
          path="/register" 
          element={
            currentUser ? <Navigate to="/screenings" /> : <Register />
          } 
        />
        <Route 
          path="/screenings" 
          element={
            <PrivateRoute>
              <ScreeningList currentUser={currentUser} onLogout={handleLogout} />
            </PrivateRoute>
          } 
        />
        <Route 
          path="/screening/:id/seats" 
          element={
            <PrivateRoute>
              <SeatSelection currentUser={currentUser} />
            </PrivateRoute>
          } 
        />
        <Route 
          path="/profile" 
          element={
            <PrivateRoute>
              <UserProfile currentUser={currentUser} onUpdate={setCurrentUser} />
            </PrivateRoute>
          } 
        />
        <Route 
          path="/admin" 
          element={
            <PrivateRoute adminOnly={true}>
              <AdminPanel currentUser={currentUser} onLogout={handleLogout} />
            </PrivateRoute>
          } 
        />
        <Route path="/" element={<Navigate to="/screenings" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
