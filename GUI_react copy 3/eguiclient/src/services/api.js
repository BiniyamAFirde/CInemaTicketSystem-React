const API_BASE_URL = 'http://localhost:5155/api';

class ApiService {
  constructor() {
    this.currentUser = this.loadCurrentUser();
  }

  loadCurrentUser() {
    const userStr = localStorage.getItem('currentUser');
    return userStr ? JSON.parse(userStr) : null;
  }

  saveCurrentUser(user) {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUser = user;
  }

  getCurrentUser() {
    return this.currentUser;
  }

  logout() {
    localStorage.removeItem('currentUser');
    this.currentUser = null;
  }

  // ✅ FIXED: Enhanced response handler with better conflict detection
  async handleResponse(response) {
    const contentType = response.headers.get('content-type');
    const isJson = contentType && contentType.includes('application/json');
    
    // Parse response body
    let data;
    try {
      data = isJson ? await response.json() : await response.text();
    } catch (parseError) {
      data = null;
    }

    if (!response.ok) {
      // Extract error message
      let errorMessage = 'An error occurred';
      
      if (data) {
        if (typeof data === 'object') {
          errorMessage = data.message || data.error || JSON.stringify(data);
        } else {
          errorMessage = data;
        }
      } else {
        errorMessage = response.statusText || `HTTP ${response.status}`;
      }

      const error = new Error(errorMessage);
      error.status = response.status;
      
      // ✅ CRITICAL: Attach conflict flag for 409 Conflict responses
      if (response.status === 409) {
        error.conflict = true;
        console.log('🔴 CONFLICT DETECTED:', errorMessage);
      }
      
      // Also check if response explicitly says it's a conflict
      if (data && data.conflict === true) {
        error.conflict = true;
        console.log('🔴 CONFLICT FLAG DETECTED:', errorMessage);
      }

      throw error;
    }

    return data;
  }

  // Auth endpoints
  async register(userData) {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData)
    });
    return this.handleResponse(response);
  }

  async login(credentials) {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
    const user = await this.handleResponse(response);
    this.saveCurrentUser(user);
    return user;
  }

  // User endpoints
  async getUsers() {
    const response = await fetch(`${API_BASE_URL}/users`);
    return this.handleResponse(response);
  }

  async getUser(id) {
    const response = await fetch(`${API_BASE_URL}/users/${id}`);
    return this.handleResponse(response);
  }

  async createUser(userData) {
    const response = await fetch(`${API_BASE_URL}/users`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData)
    });
    return this.handleResponse(response);
  }

  async updateUser(id, userData) {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData)
    });
    return this.handleResponse(response);
  }

  async toggleUserAdmin(id, rowVersion) {
    const response = await fetch(`${API_BASE_URL}/users/${id}/toggle-admin`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ rowVersion })
    });
    return this.handleResponse(response);
  }

  async deleteUser(id, rowVersion) {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ rowVersion })
    });
    return this.handleResponse(response);
  }

  // Cinema endpoints
  async getCinemas() {
    const response = await fetch(`${API_BASE_URL}/cinemas`);
    return this.handleResponse(response);
  }

  async getCinema(id) {
    const response = await fetch(`${API_BASE_URL}/cinemas/${id}`);
    return this.handleResponse(response);
  }

  // Screening endpoints
  async getScreenings() {
    const response = await fetch(`${API_BASE_URL}/screenings`);
    return this.handleResponse(response);
  }

  async getScreening(id) {
    const response = await fetch(`${API_BASE_URL}/screenings/${id}`);
    return this.handleResponse(response);
  }

  async createScreening(screeningData) {
    const response = await fetch(`${API_BASE_URL}/screenings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(screeningData)
    });
    return this.handleResponse(response);
  }

  async deleteScreening(id) {
    const response = await fetch(`${API_BASE_URL}/screenings/${id}`, {
      method: 'DELETE'
    });
    return this.handleResponse(response);
  }

  // Reservation endpoints
  async getSeatMap(screeningId) {
    const response = await fetch(`${API_BASE_URL}/reservations/screening/${screeningId}/seatmap`);
    return this.handleResponse(response);
  }

  async checkSeatAvailability(screeningId, row, seat) {
    const response = await fetch(`${API_BASE_URL}/reservations/check-availability`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ screeningId, row, seat })
    });
    return this.handleResponse(response);
  }

  // ✅ CRITICAL: Create reservation with proper error handling
  async createReservation(reservationData) {
    const userId = this.currentUser?.id;
    if (!userId) {
      throw new Error('You must be logged in to make a reservation');
    }

    const response = await fetch(`${API_BASE_URL}/reservations?userId=${userId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(reservationData)
    });
    
    return this.handleResponse(response);
  }

  async cancelReservationBySeat(screeningId, row, seat) {
    const userId = this.currentUser?.id;
    if (!userId) {
      throw new Error('You must be logged in to cancel a reservation');
    }

    const response = await fetch(
      `${API_BASE_URL}/reservations/seat?screeningId=${screeningId}&row=${row}&seat=${seat}&userId=${userId}`,
      { method: 'DELETE' }
    );
    return this.handleResponse(response);
  }
}

const apiService = new ApiService();
export default apiService;