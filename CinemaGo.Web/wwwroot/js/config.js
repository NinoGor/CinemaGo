export const CONFIG = Object.freeze({
  API_BASE_URL: localStorage.getItem('cinemago_api_url') || window.location.origin,
  TOKEN_KEY: 'cinemago_token',
  USER_KEY: 'cinemago_user'
});
