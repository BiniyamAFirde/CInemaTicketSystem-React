import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function UserProfile({ currentUser, onUpdate }) {
  const [formData, setFormData] = useState({
    firstName: currentUser.firstName,
    lastName: currentUser.lastName,
    phoneNumber: currentUser.phoneNumber,
    rowVersion: currentUser.rowVersion // ✅ Include RowVersion
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await apiService.updateUser(currentUser.id, formData);
     
      const updatedUser = { 
        ...currentUser, 
        ...formData,
        rowVersion: response.rowVersion || formData.rowVersion
      };
      
      apiService.saveCurrentUser(updatedUser);
      onUpdate(updatedUser);

      setSuccess('✅ Profile updated successfully!');
    
      setFormData(prev => ({
        ...prev,
        rowVersion: response.rowVersion || prev.rowVersion
      }));
      
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      if (err.conflict) {
        setError('⚠️ CONFLICT: Your profile was modified in another session. The page will refresh with the latest data.');
        setTimeout(async () => {
         
          const freshUser = await apiService.getUser(currentUser.id);
          apiService.saveCurrentUser(freshUser);
          onUpdate(freshUser);
          setFormData({
            firstName: freshUser.firstName,
            lastName: freshUser.lastName,
            phoneNumber: freshUser.phoneNumber,
            rowVersion: freshUser.rowVersion
          });
          setError('');
        }, 3000);
      } else {
        setError(err.message || 'Failed to update profile');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex justify-between items-center">
            <h1 className="text-2xl font-bold text-gray-800">My Profile</h1>
            <button
              onClick={() => navigate('/screenings')}
              className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition"
            >
              ← Back to Screenings
            </button>
          </div>
        </div>
      </nav>

      <div className="max-w-2xl mx-auto px-4 py-12">
        <div className="bg-white rounded-xl shadow-lg p-8">
          <h2 className="text-2xl font-bold text-gray-800 mb-6">Edit Profile</h2>

          {error && (
            <div className="bg-red-100 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded mb-4 flex items-start">
              <span className="text-2xl mr-3">⚠️</span>
              <span className="font-medium">{error}</span>
            </div>
          )}

          {success && (
            <div className="bg-green-100 border-l-4 border-green-500 text-green-700 px-4 py-3 rounded mb-4 flex items-start">
              <span className="text-2xl mr-3">✅</span>
              <span className="font-medium">{success}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                First Name
              </label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                required
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Last Name
              </label>
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                required
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Email (cannot be changed)
              </label>
              <input
                type="email"
                value={currentUser.email}
                disabled
                className="w-full px-4 py-3 border border-gray-300 rounded-lg bg-gray-100 cursor-not-allowed"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Phone Number
              </label>
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                required
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              />
            </div>

            <div className="flex gap-4">
              <button
                type="submit"
                disabled={loading}
                className="flex-1 bg-purple-600 text-white py-3 rounded-lg font-semibold hover:bg-purple-700 transition disabled:bg-purple-300 disabled:cursor-not-allowed"
              >
                {loading ? 'Saving...' : 'Save Changes'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/screenings')}
                className="flex-1 bg-gray-300 text-gray-700 py-3 rounded-lg font-semibold hover:bg-gray-400 transition"
              >
                Cancel
              </button>
            </div>
          </form>

          <div className="mt-8 pt-8 border-t border-gray-200">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Account Information</h3>
            <div className="space-y-2 text-gray-600">
              <p><span className="font-medium">Account Type:</span> {currentUser.isAdmin ? 'Administrator' : 'Regular User'}</p>
              <p><span className="font-medium">User ID:</span> {currentUser.id}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default UserProfile;