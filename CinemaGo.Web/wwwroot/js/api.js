import { CONFIG } from './config.js';

export class ApiError extends Error {
  constructor(message, status, payload) { super(message); this.name = 'ApiError'; this.status = status; this.payload = payload; }
}

export function setApiBaseUrl(url) {
  const clean = String(url || '').trim().replace(/\/$/, '');
  if (!/^https?:\/\//i.test(clean)) throw new Error('API URL must begin with http:// or https://');
  localStorage.setItem('cinemago_api_url', clean);
}

export async function api(path, { method = 'GET', body, query, auth = true, headers = {}, signal } = {}) {
    const url = new URL(
        `${CONFIG.API_BASE_URL}${path}`,
        window.location.origin
    );

  if (query) Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') url.searchParams.set(key, String(value));
  });
  const token = localStorage.getItem(CONFIG.TOKEN_KEY);
  const requestHeaders = { Accept: 'application/json', ...headers };
  if (body !== undefined) requestHeaders['Content-Type'] = 'application/json';
  if (auth && token) requestHeaders.Authorization = `Bearer ${token}`;
  let response;
  try {
    response = await fetch(url, { method, headers: requestHeaders, body: body === undefined ? undefined : JSON.stringify(body), signal });
  } catch (error) {
    if (error.name === 'AbortError') throw error;
    throw new ApiError('Could not connect to CinemaGo API. Check the API URL, server status, and CORS configuration.', 0);
  }
  const raw = response.status === 204 ? '' : await response.text();
  let payload = null;
  if (raw) { try { payload = JSON.parse(raw); } catch { payload = raw; } }
  if (!response.ok) {
    if (response.status === 401 && auth) window.dispatchEvent(new CustomEvent('cinemago:unauthorized'));
    const message = payload?.message || payload?.title || (typeof payload === 'string' ? payload : '') || `Request failed (${response.status})`;
    throw new ApiError(message, response.status, payload);
  }
  return payload;
}

export const endpoints = {
  register: body => api('/api/Auth/register', { method: 'POST', body, auth: false }),
  registerAdmin: body => api('/api/Auth/register-admin', { method: 'POST', body, auth: false }),
  confirmEmail: (userId, token) => api('/api/Auth/confirm-email', { auth: false, query: { UserId: userId, Token: token } }),
  resendConfirmation: body => api('/api/Auth/resend-confirmation', { method: 'POST', body, auth: false }),
  login: body => api('/api/Auth/login', { method: 'POST', body, auth: false }),
  movies: query => api('/api/Movies', { query }), movie: id => api(`/api/Movies/${encodeURIComponent(id)}`),
  createMovie: body => api('/api/Movies', { method: 'POST', body }), updateMovie: (id, body) => api(`/api/Movies/${id}`, { method: 'PUT', body }), deleteMovie: id => api(`/api/Movies/${id}`, { method: 'DELETE' }),
  genres: () => api('/api/Genres'), halls: () => api('/api/Halls'),
  sessions: query => api('/api/Sessions', { query }), session: id => api(`/api/Sessions/${id}`), createSession: body => api('/api/Sessions', { method: 'POST', body }), updateSession: (id, body) => api(`/api/Sessions/${id}`, { method: 'PUT', body }), cancelSession: id => api(`/api/Sessions/${id}`, { method: 'DELETE' }), seats: id => api(`/api/Sessions/${id}/seats`),
  createBooking: body => api('/api/Bookings', { method: 'POST', body }), myBookings: query => api('/api/Bookings/my', { query }), booking: id => api(`/api/Bookings/${id}`), cancelBooking: id => api(`/api/Bookings/${id}`, { method: 'DELETE' }), ticket: id => api(`/api/Tickets/${id}`), revenue: query => api('/api/Reports/revenue', { query })
};
