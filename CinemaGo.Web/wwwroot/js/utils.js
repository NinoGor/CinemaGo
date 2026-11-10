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
export function poster(url) {
    if (url && url.trim() !== '') {
        const safeUrl = esc(url);
        return `
      <div class="poster-wrapper" style="position:relative; width:100%; height:100%; overflow:hidden; background:#111; display:flex; align-items:center; justify-content:center;">
        <!-- blurred ფონი -->
        <div style="position:absolute; inset:0; background-image:url('${safeUrl}'); background-size:cover; background-position:center; filter:blur(12px) brightness(0.6); transform:scale(1.2);"></div>
        <!-- მთავარი სურათი, რომელიც სრულად ჩანს და არ იჭრება -->
        <img src="${safeUrl}" alt="Movie poster" style="position:relative; max-width:100%; max-height:100%; object-fit:contain; box-shadow: 0 4px 20px rgba(0,0,0,0.5);">
      </div>
    `;
    }
    return `<div class="poster-fallback" style="display:flex;align-items:center;justify-content:center;height:100%;background:var(--card-bg,#222);color:var(--muted);font-weight:600;">No Poster</div>`;
}
export function formData(form) { return Object.fromEntries(new FormData(form).entries()); }
export function qs(params) { const s = new URLSearchParams(); Object.entries(params).forEach(([k,v]) => { if (v !== '' && v != null) s.set(k, v); }); return s.toString(); }
