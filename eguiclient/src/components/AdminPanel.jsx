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

  const [newScreening, setNewScreening] = useState({
    cinemaId: '',
    movieTitle: '',
    startDateTime: ''
  });

  const [showCreateUserModal, setShowCreateUserModal] = useState(false);
  const [showEditUserModal, setShowEditUserModal] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  
  const [newUser, setNewUser] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    isAdmin: false
  });


  const [editUserForm, setEditUserForm] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    rowVersion: '' 
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

      setSuccess('✅ Screening created successfully!');
      setNewScreening({ cinemaId: '', movieTitle: '', startDateTime: '' });
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to create screening');
    }
  };

  const handleDeleteScreening = async (id) => {
    if (!window.confirm('⚠️ Are you sure you want to delete this screening? All reservations will be deleted.')) {
      return;
    }

    try {
      await apiService.deleteScreening(id);
      setSuccess('✅ Screening deleted successfully!');
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to delete screening');
    }
  };

  const handleOpenCreateUserModal = () => {
    setNewUser({
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      password: '',
      isAdmin: false
    });
    setShowCreateUserModal(true);
  };

  const handleCreateUser = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      await apiService.createUser(newUser);
      setSuccess('✅ User created successfully!');
      setShowCreateUserModal(false);
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to create user');
    }
  };

  
  const handleOpenEditModal = (user) => {
    setEditingUser(user);
    setEditUserForm({
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      rowVersion: user.rowVersion 
    });
    setShowEditUserModal(true);
  };

 
  const handleUpdateUser = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      await apiService.updateUser(editingUser.id, {
        firstName: editUserForm.firstName,
        lastName: editUserForm.lastName,
        phoneNumber: editUserForm.phoneNumber,
        rowVersion: editUserForm.rowVersion 
      });
      
      setSuccess('✅ User updated successfully!');
      setShowEditUserModal(false);
      setEditingUser(null);
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      
      if (err.conflict) {
        setError('⚠️ CONFLICT: This user was modified by another admin. Refreshing with latest data...');
        setTimeout(async () => {
          setShowEditUserModal(false);
          setEditingUser(null);
          await loadData();
          setError('');
        }, 3000);
      } else {
        setError(err.message || 'Failed to update user');
      }
    }
  };

 
  const handleToggleAdmin = async (userId) => {
    const user = users.find(u => u.id === userId);
    const action = user.isAdmin ? 'remove admin rights from' : 'make admin';
    
    if (!window.confirm(`⚠️ Are you sure you want to ${action} this user?`)) {
      return;
    }

    try {
      await apiService.toggleUserAdmin(userId, user.rowVersion); 
      setSuccess('✅ User role updated successfully!');
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      if (err.conflict) {
        setError('⚠️ CONFLICT: User was modified by another admin. Refreshing data...');
        setTimeout(async () => {
          await loadData();
          setError('');
        }, 2000);
      } else {
        setError(err.message || 'Failed to update user role');
      }
    }
  };

  
  const handleDeleteUser = async (userId) => {
    const user = users.find(u => u.id === userId);
    
    if (!window.confirm('⚠️ Are you sure you want to delete this user?')) {
      return;
    }

    try {
      await apiService.deleteUser(userId, user.rowVersion); 
      setSuccess('✅ User deleted successfully!');
      await loadData();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      if (err.conflict) {
        setError('⚠️ CONFLICT: User was modified by another admin. Refreshing data...');
        setTimeout(async () => {
          await loadData();
          setError('');
        }, 2000);
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
              📽️ Screenings Management
            </button>
            <button
              onClick={() => setActiveTab('users')}
              className={`py-4 px-2 border-b-2 font-medium transition ${
                activeTab === 'users'
                  ? 'border-purple-600 text-purple-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              👥 Users Management
            </button>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Error Alert */}
        {error && (
          <div className="bg-red-100 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded mb-4 flex items-start shadow-md animate-pulse">
            <span className="text-2xl mr-3">⚠️</span>
            <div className="flex-1">
              <p className="font-bold">Conflict Detected</p>
              <p className="font-medium">{error}</p>
            </div>
          </div>
        )}

        {/* Success Alert */}
        {success && (
          <div className="bg-green-100 border-l-4 border-green-500 text-green-700 px-4 py-3 rounded mb-4 flex items-start shadow-md">
            <span className="text-2xl mr-3">✅</span>
            <span className="font-medium">{success}</span>
          </div>
        )}

        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-purple-600 mx-auto mb-4"></div>
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
                        <label className="block text-sm font-medium text-gray-700 mb-2">Cinema *</label>
                        <select
                          value={newScreening.cinemaId}
                          onChange={(e) => setNewScreening({ ...newScreening, cinemaId: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
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
                        <label className="block text-sm font-medium text-gray-700 mb-2">Movie Title *</label>
                        <input
                          type="text"
                          value={newScreening.movieTitle}
                          onChange={(e) => setNewScreening({ ...newScreening, movieTitle: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Date & Time *</label>
                        <input
                          type="datetime-local"
                          value={newScreening.startDateTime}
                          onChange={(e) => setNewScreening({ ...newScreening, startDateTime: e.target.value })}
                          required
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
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
                          <tr key={screening.id} className="hover:bg-gray-50 transition">
                            <td className="px-6 py-4 font-medium text-gray-900">{screening.movieTitle}</td>
                            <td className="px-6 py-4 text-gray-600">{screening.cinemaName}</td>
                            <td className="px-6 py-4 text-gray-600">{formatDateTime(screening.startDateTime)}</td>
                            <td className="px-6 py-4">
                              <button
                                onClick={() => handleDeleteScreening(screening.id)}
                                className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition text-sm"
                              >
                                🗑️ Delete
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
              <div className="space-y-6">
                <div className="flex justify-between items-center">
                  <h2 className="text-2xl font-bold text-gray-800">All Users</h2>
                  <button
                    onClick={handleOpenCreateUserModal}
                    className="px-6 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition"
                  >
                    + Create New User
                  </button>
                </div>

                <div className="bg-white rounded-xl shadow-lg overflow-hidden">
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
                          <tr key={user.id} className="hover:bg-gray-50 transition">
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
                                {user.isAdmin ? '👨‍💼 Admin' : '👤 User'}
                              </span>
                            </td>
                            <td className="px-6 py-4">
                              <div className="flex gap-2">
                                <button
                                  onClick={() => handleOpenEditModal(user)}
                                  className="px-3 py-1.5 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition text-sm"
                                >
                                  ✏️ Edit
                                </button>
                                <button
                                  onClick={() => handleToggleAdmin(user.id)}
                                  className={`px-3 py-1.5 rounded-lg transition text-sm ${
                                    user.isAdmin
                                      ? 'bg-orange-500 text-white hover:bg-orange-600'
                                      : 'bg-purple-500 text-white hover:bg-purple-600'
                                  }`}
                                >
                                  {user.isAdmin ? '⬇️ Remove Admin' : '⬆️ Make Admin'}
                                </button>
                                {!user.isAdmin && (
                                  <button
                                    onClick={() => handleDeleteUser(user.id)}
                                    className="px-3 py-1.5 bg-red-500 text-white rounded-lg hover:bg-red-600 transition text-sm"
                                  >
                                    🗑️ Delete
                                  </button>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {/* Create User Modal */}
      {showCreateUserModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-2xl p-8 max-w-md w-full">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Create New User</h2>
            <form onSubmit={handleCreateUser} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">First Name *</label>
                <input
                  type="text"
                  value={newUser.firstName}
                  onChange={(e) => setNewUser({ ...newUser, firstName: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Last Name *</label>
                <input
                  type="text"
                  value={newUser.lastName}
                  onChange={(e) => setNewUser({ ...newUser, lastName: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Email *</label>
                <input
                  type="email"
                  value={newUser.email}
                  onChange={(e) => setNewUser({ ...newUser, email: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Phone Number *</label>
                <input
                  type="tel"
                  value={newUser.phoneNumber}
                  onChange={(e) => setNewUser({ ...newUser, phoneNumber: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Password *</label>
                <input
                  type="password"
                  value={newUser.password}
                  onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
                  required
                  minLength="6"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isAdmin"
                  checked={newUser.isAdmin}
                  onChange={(e) => setNewUser({ ...newUser, isAdmin: e.target.checked })}
                  className="w-4 h-4 text-purple-600 border-gray-300 rounded focus:ring-purple-500"
                />
                <label htmlFor="isAdmin" className="ml-2 block text-sm text-gray-700">
                  Make this user an admin
                </label>
              </div>

              <div className="flex gap-3 mt-6">
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition font-semibold"
                >
                  Create User
                </button>
                <button
                  type="button"
                  onClick={() => setShowCreateUserModal(false)}
                  className="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition font-semibold"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Edit User Modal */}
      {showEditUserModal && editingUser && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-2xl p-8 max-w-md w-full">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Edit User</h2>
            <form onSubmit={handleUpdateUser} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">First Name *</label>
                <input
                  type="text"
                  value={editUserForm.firstName}
                  onChange={(e) => setEditUserForm({ ...editUserForm, firstName: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Last Name *</label>
                <input
                  type="text"
                  value={editUserForm.lastName}
                  onChange={(e) => setEditUserForm({ ...editUserForm, lastName: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Phone Number *</label>
                <input
                  type="tel"
                  value={editUserForm.phoneNumber}
                  onChange={(e) => setEditUserForm({ ...editUserForm, phoneNumber: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500"
                />
              </div>

              <div className="bg-gray-100 p-3 rounded-lg">
                <p className="text-sm text-gray-600">
                  <strong>Email:</strong> {editingUser.email} (cannot be changed)
                </p>
              </div>

              <div className="flex gap-3 mt-6">
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-semibold"
                >
                  Update User
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowEditUserModal(false);
                    setEditingUser(null);
                  }}
                  className="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition font-semibold"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default AdminPanel;