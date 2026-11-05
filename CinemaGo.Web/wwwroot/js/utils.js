export const $ = (selector, root = document) => root.querySelector(selector);
export const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];
export function esc(value = '') { return String(value ?? '').replace(/[&<>"']/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c])); }
export function money(value) { return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'GEL', maximumFractionDigits: 2 }).format(Number(value || 0)); }
export function dateTime(value) { if (!value) return '—'; const d = new Date(value); return Number.isNaN(+d) ? '—' : d.toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' }); }
export function dateOnly(value) { if (!value) return '—'; const d = new Date(value); return Number.isNaN(+d) ? '—' : d.toLocaleDateString([], { dateStyle: 'medium' }); }
export function duration(minutes) { return `${Math.floor((minutes || 0) / 60)}h ${(minutes || 0) % 60}m`; }
export function toast(message, type = 'info') { const host = $('#toasts'); if (!host) return; const el = document.createElement('div'); el.className = `toast ${type}`; el.textContent = message; host.append(el); setTimeout(() => el.remove(), 4200); }
export function errorMessage(error) { return error?.message || 'Something went wrong. Please try again.'; }
export function paged(data) { if (Array.isArray(data)) return { items: data, totalCount: data.length, page: 1, pageSize: data.length, totalPages: 1 }; return { items: Array.isArray(data?.items) ? data.items : [], totalCount: Number(data?.totalCount || 0), page: Number(data?.page || 1), pageSize: Number(data?.pageSize || 10), totalPages: Number(data?.totalPages || 1) }; }
export function poster(title = 'CinemaGo') { const initials = String(title).split(/\s+/).slice(0, 2).map(x => x[0] || '').join('').toUpperCase(); return `<div class="poster-fallback"><span>${esc(initials || 'CG')}</span><small>${esc(title)}</small></div>`; }
export function formData(form) { return Object.fromEntries(new FormData(form).entries()); }
export function qs(params) { const s = new URLSearchParams(); Object.entries(params).forEach(([k,v]) => { if (v !== '' && v != null) s.set(k, v); }); return s.toString(); }
