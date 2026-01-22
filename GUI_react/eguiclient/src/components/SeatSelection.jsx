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
      setError('This seat is already reserved by another user');
      setTimeout(() => setError(''), 3000);
      return;
    }

    setError('');
    setSuccessMessage('');

    try {
      if (seat.isReserved && seat.userId === currentUser.id) {
        // Cancel reservation
        await apiService.cancelReservationBySeat(
          parseInt(id),
          seat.row,
          seat.seat
        );
        setSuccessMessage('Reservation cancelled successfully');
      } else {
        // Create reservation
        await apiService.createReservation({
          screeningId: parseInt(id),
          row: seat.row,
          seat: seat.seat
        });
        setSuccessMessage('Seat reserved successfully!');
      }

      // Reload seat map
      await loadScreeningData();
      
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      // Handle concurrency conflicts
      if (err.message.includes('already reserved')) {
        setError('This seat was just reserved by another user. Please select a different seat.');
        await loadScreeningData(); // Refresh seat map
      } else {
        setError(err.message || 'Operation failed');
      }
      setTimeout(() => setError(''), 5000);
    }
  };

  const getSeatClass = (seat) => {
    if (seat.isReserved) {
      if (seat.userId === currentUser.id) {
        return 'bg-blue-500 hover:bg-blue-600 cursor-pointer';
      }
      return 'bg-red-500 cursor-not-allowed';
    }
    return 'bg-green-500 hover:bg-green-600 cursor-pointer';
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-xl">Loading...</div>
      </div>
    );
  }

  if (!screening) {
    return (
      <div className="min-h-screen flex items-center justify-center">
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
            className="px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400"
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
        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            {error}
          </div>
        )}

        {successMessage && (
          <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
            {successMessage}
          </div>
        )}

        <div className="bg-white rounded-xl shadow-lg p-8">
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Select Your Seats</h2>
            <div className="flex gap-6 text-sm">
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-green-500 rounded"></div>
                <span>Available</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-blue-500 rounded"></div>
                <span>Your Seats</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-8 h-8 bg-red-500 rounded"></div>
                <span>Occupied</span>
              </div>
            </div>
          </div>

          <div className="mb-8">
            <div className="bg-gray-800 text-white text-center py-4 rounded-lg mb-8">
              SCREEN
            </div>

            <div className="flex flex-col items-center gap-2">
              {Array.from({ length: screening.rows }).map((_, row) => (
                <div key={row} className="flex items-center gap-2">
                  <span className="w-8 text-center font-semibold text-gray-600">
                    {row + 1}
                  </span>
                  {Array.from({ length: screening.seatsPerRow }).map((_, seatNum) => {
                    const seat = seatMap.find(s => s.row === row && s.seat === seatNum);
                    if (!seat) return null;

                    return (
                      <button
                        key={seatNum}
                        onClick={() => handleSeatClick(seat)}
                        className={`w-10 h-10 rounded transition ${getSeatClass(seat)}`}
                        disabled={seat.isReserved && seat.userId !== currentUser.id}
                      >
                        <span className="text-white text-xs">{seatNum + 1}</span>
                      </button>
                    );
                  })}
                </div>
              ))}
            </div>
          </div>

          {myReservations.length > 0 && (
            <div className="bg-blue-50 p-6 rounded-lg">
              <h3 className="font-bold text-lg mb-2">Your Reservations:</h3>
              <div className="flex flex-wrap gap-2">
                {myReservations.map((r, idx) => (
                  <span key={idx} className="bg-blue-500 text-white px-3 py-1 rounded">
                    Row {r.row + 1}, Seat {r.seat + 1}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default SeatSelection;