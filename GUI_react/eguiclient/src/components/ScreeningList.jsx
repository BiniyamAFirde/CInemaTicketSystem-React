import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function ScreeningList({ currentUser, onLogout }) {
  const [screenings, setScreenings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    loadScreenings();
  }, []);

  const loadScreenings = async () => {
    setLoading(true);
    setError('');
    
    try {
      const data = await apiService.getScreenings();
      setScreenings(data);
    } catch (err) {
      setError('Failed to load screenings');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const formatDateTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleSelectSeats = (screeningId) => {
    navigate(`/screening/${screeningId}/seats`);
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-xl text-gray-700">Loading screenings...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex justify-between items-center">
            <h1 className="text-2xl font-bold text-gray-800">🎬 Cinema Tickets</h1>
            <div className="flex items-center gap-4">
              <span className="text-gray-600">
                Welcome, {currentUser.firstName} {currentUser.lastName}
              </span>
              <button
                onClick={() => navigate('/profile')}
                className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition"
              >
                Profile
              </button>
              {currentUser.isAdmin && (
                <button
                  onClick={() => navigate('/admin')}
                  className="px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600 transition"
                >
                  Admin Panel
                </button>
              )}
              <button
                onClick={onLogout}
                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="mb-8">
          <h2 className="text-3xl font-bold text-gray-800 mb-2">Available Screenings</h2>
          <p className="text-gray-600">Select a movie to reserve your seats</p>
        </div>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            {error}
          </div>
        )}

        {screenings.length === 0 ? (
          <div className="bg-white rounded-xl shadow-lg p-12 text-center">
            <p className="text-xl text-gray-600">No screenings available at the moment</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {screenings.map((screening) => (
              <div
                key={screening.id}
                className="bg-white rounded-xl shadow-lg overflow-hidden hover:shadow-2xl transition-shadow duration-300"
              >
                <div className="bg-gradient-to-r from-purple-600 to-blue-600 p-6">
                  <h3 className="text-2xl font-bold text-white mb-2">
                    {screening.movieTitle}
                  </h3>
                  <p className="text-purple-100">
                    📍 {screening.cinemaName}
                  </p>
                </div>

                <div className="p-6">
                  <div className="space-y-3 mb-6">
                    <div className="flex items-center text-gray-700">
                      <span className="font-semibold w-24">Date:</span>
                      <span>{formatDateTime(screening.startDateTime)}</span>
                    </div>
                    <div className="flex items-center text-gray-700">
                      <span className="font-semibold w-24">Size:</span>
                      <span>{screening.rows} rows × {screening.seatsPerRow} seats</span>
                    </div>
                    <div className="flex items-center text-gray-700">
                      <span className="font-semibold w-24">Capacity:</span>
                      <span>{screening.rows * screening.seatsPerRow} seats</span>
                    </div>
                  </div>

                  <button
                    onClick={() => handleSelectSeats(screening.id)}
                    className="w-full bg-purple-600 text-white py-3 rounded-lg font-semibold hover:bg-purple-700 transition"
                  >
                    Select Seats
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default ScreeningList;