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

  async handleResponse(response) {
    const contentType = response.headers.get('content-type');
    const isJson = contentType && contentType.includes('application/json');
    const data = isJson ? await response.json() : await response.text();

    if (!response.ok) {
      const error = (data && data.message) || data || response.statusText;
      throw new Error(error);
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

  // NEW: Create user (Admin only)
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

  // NEW: Toggle admin status
  async toggleUserAdmin(id) {
    const response = await fetch(`${API_BASE_URL}/users/${id}/toggle-admin`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' }
    });
    return this.handleResponse(response);
  }

  async deleteUser(id) {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      method: 'DELETE'
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

  async createReservation(reservationData) {
    const userId = this.currentUser?.id;
    const response = await fetch(`${API_BASE_URL}/reservations?userId=${userId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(reservationData)
    });
    return this.handleResponse(response);
  }

  async cancelReservationBySeat(screeningId, row, seat) {
    const userId = this.currentUser?.id;
    const response = await fetch(
      `${API_BASE_URL}/reservations/seat?screeningId=${screeningId}&row=${row}&seat=${seat}&userId=${userId}`,
      { method: 'DELETE' }
    );
    return this.handleResponse(response);
  }
}

const apiService = new ApiService();
export default apiService;