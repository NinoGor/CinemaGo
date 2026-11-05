import { CONFIG } from './config.js';
import { endpoints } from './api.js';

export function currentUser() {
  try {
    const user = JSON.parse(localStorage.getItem(CONFIG.USER_KEY) || 'null');
    const token = localStorage.getItem(CONFIG.TOKEN_KEY);
    if (!user || !token || (user.expiresAt && Date.parse(user.expiresAt) <= Date.now())) { logout(false); return null; }
    return user;
  } catch { return null; }
}
export function isAdmin() { return (currentUser()?.roles || []).some(role => String(role).toLowerCase() === 'admin' || String(role).toLowerCase() === 'administrator'); }
export async function login(email, password) {
  const result = await endpoints.login({ email, password });
  if (!result?.token) throw new Error('The API response did not contain an authentication token.');
  localStorage.setItem(CONFIG.TOKEN_KEY, result.token);
  localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(result));
  return result;
}
export function logout(redirect = true) { localStorage.removeItem(CONFIG.TOKEN_KEY); localStorage.removeItem(CONFIG.USER_KEY); if (redirect) location.hash = '#/'; }
export function requireUser() { if (!currentUser()) { location.hash = '#/login'; return false; } return true; }
export function requireAdmin() { if (!requireUser()) return false; if (!isAdmin()) { location.hash = '#/forbidden'; return false; } return true; }
