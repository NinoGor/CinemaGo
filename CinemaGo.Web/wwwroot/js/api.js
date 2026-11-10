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
  const url = new URL(`${CONFIG.API_BASE_URL}${path}`);
  if (query) Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return;

    // The API caps every paginated request at 200 records. Enforce that limit
    // centrally so no page can accidentally send an oversized PageSize.
    if (key.toLowerCase() === 'pagesize') {
      const requested = Number(value);
      const pageSize = Number.isFinite(requested)
        ? Math.min(200, Math.max(1, Math.trunc(requested)))
        : 10;
      url.searchParams.set(key, String(pageSize));
      return;
    }

    url.searchParams.set(key, String(value));
  });
  const token = localStorage.getItem(CONFIG.TOKEN_KEY);
  const requestHeaders = { Accept: 'application/json', ...headers };
  const isFormData = typeof FormData !== 'undefined' && body instanceof FormData;
  if (body !== undefined && !isFormData) requestHeaders['Content-Type'] = 'application/json';
  if (auth && token) requestHeaders.Authorization = `Bearer ${token}`;
  let response;
  try {
    response = await fetch(url, { method, headers: requestHeaders, body: body === undefined ? undefined : (isFormData ? body : JSON.stringify(body)), signal });
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
  uploadMoviePoster: (id, file) => { const body = new FormData(); body.append('file', file); return api(`/api/Movies/${id}/poster`, { method: 'POST', body }); },
  setMoviePosterUrl: (id, posterUrl) => api(`/api/Movies/${id}/poster`, { method: 'PUT', body: { posterUrl } }),
  deleteMoviePoster: id => api(`/api/Movies/${id}/poster`, { method: 'DELETE' }),
  genres: () => api('/api/Genres'), genre: id => api(`/api/Genres/${id}`),
  createGenre: body => api('/api/Genres', { method: 'POST', body }), updateGenre: (id, body) => api(`/api/Genres/${id}`, { method: 'PUT', body }), deleteGenre: id => api(`/api/Genres/${id}`, { method: 'DELETE' }),
  halls: () => api('/api/Halls'), hall: id => api(`/api/Halls/${id}`),
  createHall: body => api('/api/Halls', { method: 'POST', body }), updateHall: (id, body) => api(`/api/Halls/${id}`, { method: 'PUT', body }), deleteHall: id => api(`/api/Halls/${id}`, { method: 'DELETE' }),
  hallSeats: id => api(`/api/Halls/${id}/seats`), updateHallSeat: (id, seatId, body) => api(`/api/Halls/${id}/seats/${seatId}`, { method: 'PUT', body }),
  sessions: query => api('/api/Sessions', { query }), session: id => api(`/api/Sessions/${id}`), createSession: body => api('/api/Sessions', { method: 'POST', body }), updateSession: (id, body) => api(`/api/Sessions/${id}`, { method: 'PUT', body }), cancelSession: id => api(`/api/Sessions/${id}`, { method: 'DELETE' }), seats: id => api(`/api/Sessions/${id}/seats`),
  createBooking: body => api('/api/Bookings', { method: 'POST', body }), myBookings: query => api('/api/Bookings/my', { query }), booking: id => api(`/api/Bookings/${id}`), cancelBooking: id => api(`/api/Bookings/${id}`, { method: 'DELETE' }), ticket: id => api(`/api/Tickets/${id}`), revenue: query => api('/api/Reports/revenue', { query })
};
