import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import apiService from '../services/api';

function SeatSelection({ currentUser }) {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [screening, setScreening] = useState(null);
  const [seatMap, setSeatMap] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [selectedSeat, setSelectedSeat] = useState(null);
  const [confirmLoading, setConfirmLoading] = useState(false);

  useEffect(() => {
    loadScreeningData();
  }, [id]);

  const loadScreeningData = async () => {
    setLoading(true);
    setError('');
    
    try {
      const [screeningData, seats] = await Promise.all([
        apiService.getScreening(id),
        apiService.getSeatMap(id)
      ]);
      
      setScreening(screeningData);
      setSeatMap(seats);
    } catch (err) {
      setError('Failed to load screening data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSeatClick = async (seat) => {
    if (seat.isReserved && seat.userId !== currentUser.id) {
      setError('⚠️ This seat is already reserved by another user');
      setTimeout(() => setError(''), 3000);
      return;
    }

    setError('');
    setSuccessMessage('');

    if (seat.isReserved && seat.userId === currentUser.id) {
      await handleCancelReservation(seat);
      return;
    }

    setSelectedSeat(seat);
    setShowConfirmModal(true);
  };

  const handleConfirmReservation = async () => {
    if (!selectedSeat) return;
    
    setConfirmLoading(true);
    setError('');
    setSuccessMessage('');

    try {
      // request to back
      await apiService.createReservation({
        screeningId: parseInt(id),
        row: selectedSeat.row,
        seat: selectedSeat.seat
      });

      setSuccessMessage(`✅ Seat Row ${selectedSeat.row + 1}, Seat ${selectedSeat.seat + 1} reserved successfully!`);
      setShowConfirmModal(false);
      setSelectedSeat(null);

      await loadScreeningData();
      
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
  //
      if (err.conflict) {
        setError('⚠️ CONFLICT: This seat was just reserved by another user. Please select a different seat.');
        setShowConfirmModal(false);
        setSelectedSeat(null);
        await loadScreeningData(); 
      } else {
        setError(err.message || 'Failed to reserve seat');
      }
      setTimeout(() => setError(''), 5000);
    } finally {
      setConfirmLoading(false);
    }
  };


  const handleCancelConfirmation = () => {
    setShowConfirmModal(false);
    setSelectedSeat(null);
  };

  
  const handleCancelReservation = async (seat) => {
    setError('');
    setSuccessMessage('');

    try {
      await apiService.cancelReservationBySeat(
        parseInt(id),
        seat.row,
        seat.seat
      );
      
      setSuccessMessage('✅ Reservation cancelled successfully');
      await loadScreeningData();
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      setError(err.message || 'Failed to cancel reservation');
      setTimeout(() => setError(''), 5000);
    }
  };

  const getSeatClass = (seat) => {
    if (seat.isReserved) {
      if (seat.userId === currentUser.id) {
        return 'bg-blue-500 hover:bg-blue-600 cursor-pointer shadow-lg';
      }
      return 'bg-red-500 cursor-not-allowed opacity-75';
    }
    return 'bg-green-500 hover:bg-green-600 cursor-pointer transform hover:scale-110 transition-all shadow-md';
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-purple-600 mx-auto mb-4"></div>
          <p className="text-xl text-gray-700">Loading screening...</p>
        </div>
      </div>
    );
  }

  if (!screening) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-xl text-red-600">Screening not found</div>
      </div>
    );
  }

  const myReservations = seatMap.filter(s => s.isReserved && s.userId === currentUser.id);

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
          <button
            onClick={() => navigate('/screenings')}
            className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition"
          >
            ← Back to Screenings
          </button>
          <h1 className="text-2xl font-bold text-gray-800">{screening.movieTitle}</h1>
          <span className="text-gray-600">
            {new Date(screening.startDateTime).toLocaleString()}
          </span>
        </div>
      </nav>

      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Error Alert */}
        {error && (
          <div className="bg-red-100 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded mb-4 flex items-start animate-pulse">
            <span className="text-2xl mr-3">⚠️</span>
            <span className="font-medium">{error}</span>
          </div>
        )}

        {/* Success Alert */}
        {successMessage && (
          <div className="bg-green-100 border-l-4 border-green-500 text-green-700 px-4 py-3 rounded mb-4 flex items-start">
            <span className="text-2xl mr-3">✅</span>
            <span className="font-medium">{successMessage}</span>
          </div>
        )}

        <div className="bg-white rounded-xl shadow-lg p-8">
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Select Your Seats</h2>
            <div className="flex flex-wrap gap-6 text-sm">
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-green-500 rounded shadow-md"></div>
                <span className="font-medium">Available - Click to Reserve</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-blue-500 rounded shadow-lg"></div>
                <span className="font-medium">Your Seats - Click to Cancel</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-red-500 rounded opacity-75"></div>
                <span className="font-medium">Occupied by Others</span>
              </div>
            </div>
          </div>

          <div className="mb-8">
            {/* Screen */}
            <div className="bg-gradient-to-r from-gray-700 via-gray-800 to-gray-700 text-white text-center py-4 rounded-lg mb-8 font-bold text-lg shadow-xl">
              🎬 SCREEN
            </div>

            {/* Seat Grid */}
            <div className="flex flex-col items-center gap-2">
              {Array.from({ length: screening.rows }).map((_, row) => (
                <div key={row} className="flex items-center gap-2">
                  <span className="w-10 text-center font-bold text-gray-600 text-lg">
                    {row + 1}
                  </span>
                  {Array.from({ length: screening.seatsPerRow }).map((_, seatNum) => {
                    const seat = seatMap.find(s => s.row === row && s.seat === seatNum);
                    if (!seat) return null;

                    return (
                      <button
                        key={seatNum}
                        onClick={() => handleSeatClick(seat)}
                        className={`w-10 h-10 rounded transition-all ${getSeatClass(seat)}`}
                        disabled={seat.isReserved && seat.userId !== currentUser.id}
                        title={
                          seat.isReserved && seat.userId !== currentUser.id 
                            ? 'Reserved by another user' 
                            : seat.isReserved 
                            ? 'Your seat - Click to cancel'
                            : 'Available - Click to reserve'
                        }
                      >
                        <span className="text-white text-xs font-bold">{seatNum + 1}</span>
                      </button>
                    );
                  })}
                </div>
              ))}
            </div>
          </div>

          {/* My Reservations Summary */}
          {myReservations.length > 0 && (
            <div className="bg-blue-50 border-l-4 border-blue-500 p-6 rounded-lg shadow-md">
              <h3 className="font-bold text-lg mb-3 text-blue-900">Your Current Reservations:</h3>
              <div className="flex flex-wrap gap-2">
                {myReservations.map((r, idx) => (
                  <span key={idx} className="bg-blue-500 text-white px-4 py-2 rounded-lg font-semibold shadow-md">
                    Row {r.row + 1}, Seat {r.seat + 1}
                  </span>
                ))}
              </div>
              <p className="text-sm text-blue-700 mt-3 font-medium">
                💡 Click on your reserved seats to cancel them
              </p>
            </div>
          )}
        </div>
      </div>

      {/* ✅ CONFIRMATION MODAL */}
      {showConfirmModal && selectedSeat && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-2xl p-8 max-w-md w-full animate-scale-in">
            <div className="text-center mb-6">
              <div className="w-16 h-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-4xl">🎫</span>
              </div>
              <h2 className="text-2xl font-bold text-gray-800 mb-2">Confirm Reservation</h2>
              <p className="text-gray-600">Please confirm your seat selection</p>
            </div>

            <div className="bg-gradient-to-r from-purple-50 to-blue-50 p-6 rounded-lg mb-6 border-2 border-purple-200">
              <div className="text-center">
                <p className="text-sm text-gray-600 mb-2">Selected Seat</p>
                <p className="text-3xl font-bold text-purple-600">
                  Row {selectedSeat.row + 1}, Seat {selectedSeat.seat + 1}
                </p>
              </div>
            </div>

            <div className="bg-yellow-50 border-l-4 border-yellow-500 p-4 mb-6 rounded">
              <p className="text-sm text-yellow-800">
                <strong>⚠️ Important:</strong> Once confirmed, the seat will be immediately reserved. 
                If another user reserves it first, you'll be notified.
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={handleConfirmReservation}
                disabled={confirmLoading}
                className="flex-1 px-6 py-3 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition font-semibold disabled:bg-purple-300 disabled:cursor-not-allowed shadow-lg"
              >
                {confirmLoading ? (
                  <span className="flex items-center justify-center">
                    <svg className="animate-spin h-5 w-5 mr-2" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                    </svg>
                    Reserving...
                  </span>
                ) : (
                  '✅ Confirm Reservation'
                )}
              </button>
              <button
                onClick={handleCancelConfirmation}
                disabled={confirmLoading}
                className="flex-1 px-6 py-3 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition font-semibold disabled:opacity-50 shadow-md"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* CSS for animations */}
      <style jsx>{`
        @keyframes scale-in {
          from {
            transform: scale(0.9);
            opacity: 0;
          }
          to {
            transform: scale(1);
            opacity: 1;
          }
        }
        .animate-scale-in {
          animation: scale-in 0.2s ease-out;
        }
      `}</style>
    </div>
  );
}

export default SeatSelection;