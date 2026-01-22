import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function AdminPanel({ currentUser, onLogout }) {
  const [activeTab, setActiveTab] = useState('screenings');
  const [users, setUsers] = useState([]);
  const [screenings, setScreenings] = useState([]);
  const [cinemas, setCinemas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Screening form state
  const [newScreening, setNewScreening] = useState({
    cinemaId: '',
    movieTitle: '',
    startDateTime: ''
  });

  const navigate = useNavigate();

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const loadData = async () => {
    setLoading(true);
    setError('');
    
    try {
      if (activeTab === 'users') {
        const usersData = await apiService.getUsers();
        setUsers(usersData);
      } else if (activeTab === 'screenings') {
        const [screeningsData, cinemasData] = await Promise.all([
          apiService.getScreenings(),
          apiService.getCinemas()
        ]);
        setScreenings(screeningsData);
        setCinemas(cinemasData);
      }
    } catch (err) {
      setError('Failed to load data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Screening Management
  const handleCreateScreening = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      await apiService.createScreening({
        cinemaId: parseInt(newScreening.cinemaId),
        movieTitle: newScreening.movieTitle,
        startDateTime: new Date(newScreening.startDateTime).toISOString()
      });

      setSuccess('Screening created successfully!');
      setNewScreening({ cinemaId: '', movieTitle: '', startDateTime: '' });
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to create screening');
    }
  };

  const handleDeleteScreening = async (id) => {
    if (!window.confirm('Are you sure you want to delete this screening? All reservations will be deleted.')) {
      return;
    }

    try {
      await apiService.deleteScreening(id);
      setSuccess('Screening deleted successfully!');
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to delete screening');
    }
  };

  // User Management
  const handleDeleteUser = async (userId) => {
    if (!window.confirm('Are you sure you want to delete this user?')) {
      return;
    }

    try {
      await apiService.deleteUser(userId);
      setSuccess('User deleted successfully!');
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      if (err.message.includes('modified by another process')) {
        setError('User was modified by another session. Please refresh and try again.');
      } else {
        setError(err.message || 'Failed to delete user');
      }
    }
  };

  const formatDateTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex justify-between items-center">
            <h1 className="text-2xl font-bold text-gray-800">🎬 Admin Panel</h1>
            <div className="flex items-center gap-4">
              <span className="text-gray-600">Admin: {currentUser.firstName}</span>
              <button
                onClick={() => navigate('/screenings')}
                className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition"
              >
                User View
              </button>
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

      {/* Tabs */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex space-x-8">
            <button
              onClick={() => setActiveTab('screenings')}
              className={`py-4 px-2 border-b-2 font-medium transition ${
                activeTab === 'screenings'
                  ? 'border-purple-600 text-purple-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Screenings Management
            </button>
            <button
              onClick={() => setActiveTab('users')}
              className={`py-4 px-2 border-b-2 font-medium transition ${
                activeTab === 'users'
                  ? 'border-purple-600 text-purple-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              Users Management
            </button>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            {error}
          </div>
        )}

        {success && (
          <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
            {success}
          </div>
        )}

        {loading ? (
          <div className="text-center py-12">
            <p className="text-xl text-gray-600">Loading...</p>
          </div>
        ) : (
          <>
            {activeTab === 'screenings' && (
              <div className="space-y-8">
                {/* Create Screening Form */}
                <div className="bg-white rounded-xl shadow-lg p-6">
                  <h2 className="text-2xl font-bold text-gray-800 mb-6">Create New Screening</h2>
                  <form onSubmit={handleCreateScreening} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Cinema *
                        </label>
                        <select
                          value={newScreening.cinemaId}
                          onChange={(e) => setNewScreening({ ...newScreening, cinemaId: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        >
                          <option value="">Select Cinema</option>
                          {cinemas.map((cinema) => (
                            <option key={cinema.id} value={cinema.id}>
                              {cinema.name} ({cinema.rows}×{cinema.seatsPerRow})
                            </option>
                          ))}
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Movie Title *
                        </label>
                        <input
                          type="text"
                          value={newScreening.movieTitle}
                          onChange={(e) => setNewScreening({ ...newScreening, movieTitle: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Date & Time *
                        </label>
                        <input
                          type="datetime-local"
                          value={newScreening.startDateTime}
                          onChange={(e) => setNewScreening({ ...newScreening, startDateTime: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        />
                      </div>
                    </div>

                    <button
                      type="submit"
                      className="w-full md:w-auto px-6 py-3 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700 transition"
                    >
                      Create Screening
                    </button>
                  </form>
                </div>

                {/* Screenings List */}
                <div className="bg-white rounded-xl shadow-lg overflow-hidden">
                  <h2 className="text-2xl font-bold text-gray-800 p-6 border-b">All Screenings</h2>
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Movie</th>
                          <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Cinema</th>
                          <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date & Time</th>
                          <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200">
                        {screenings.map((screening) => (
                          <tr key={screening.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 font-medium text-gray-900">{screening.movieTitle}</td>
                            <td className="px-6 py-4 text-gray-600">{screening.cinemaName}</td>
                            <td className="px-6 py-4 text-gray-600">{formatDateTime(screening.startDateTime)}</td>
                            <td className="px-6 py-4">
                              <button
                                onClick={() => handleDeleteScreening(screening.id)}
                                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition text-sm"
                              >
                                Delete
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}

            {activeTab === 'users' && (
              <div className="bg-white rounded-xl shadow-lg overflow-hidden">
                <h2 className="text-2xl font-bold text-gray-800 p-6 border-b">All Users</h2>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Phone</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {users.map((user) => (
                        <tr key={user.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 font-medium text-gray-900">
                            {user.firstName} {user.lastName}
                          </td>
                          <td className="px-6 py-4 text-gray-600">{user.email}</td>
                          <td className="px-6 py-4 text-gray-600">{user.phoneNumber}</td>
                          <td className="px-6 py-4">
                            <span className={`px-2 py-1 rounded-full text-xs font-semibold ${
                              user.isAdmin 
                                ? 'bg-purple-100 text-purple-800' 
                                : 'bg-blue-100 text-blue-800'
                            }`}>
                              {user.isAdmin ? 'Admin' : 'User'}
                            </span>
                          </td>
                          <td className="px-6 py-4">
                            {!user.isAdmin && (
                              <button
                                onClick={() => handleDeleteUser(user.id)}
                                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition text-sm"
                              >
                                Delete
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default AdminPanel;