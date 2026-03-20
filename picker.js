'use strict';
/* ══════════════════════════════════════
   ПОДДЕРЖКА — отправка обращений
   Сообщение идёт обоим администраторам
   через Telegram Bot API
══════════════════════════════════════ */

const BOT_TOKEN = '8297258284:AAFjq8Yn5en7c1XS0qKM4YMylnswzCSRgTM';
const ADMINS    = [8306863943, 6641882724];

function tgSend(chat_id, text) {
  return fetch(`https://api.telegram.org/bot${BOT_TOKEN}/sendMessage`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ chat_id, text, parse_mode: 'HTML' }),
  });
}

async function sendSup() {
  const subjEl = document.getElementById('sp-subj');
  const msgEl  = document.getElementById('sp-msg');
  const subj   = subjEl.value;
  const msg    = msgEl.value.trim();

  if (!subj || !msg) { toast(t('toast.fill')); return; }

  const btn = document.querySelector('#hlp-form-panel .btn-p');
  if (btn) { btn.style.opacity = '.5'; btn.style.pointerEvents = 'none'; }

  const user   = tgU.username ? `@${tgU.username}` : (tgU.first_name || 'Аноним');
  const uid    = tgU.id ? `[${tgU.id}]` : '';
  const labels = { buy: 'Покупка/Оплата', key: 'Проблема с ключом', hwid: 'Привязка HWID', tech: 'Техн. проблема', oth: 'Другое' };
  const text   = `<b>Обращение KENOS</b>\n\nОт: ${user} ${uid}\nТема: ${labels[subj] || subj}\n\n${msg}`;

  try {
    await Promise.all(ADMINS.map(id => tgSend(id, text)));
    toast(t('toast.sent'));
    subjEl.value = '';
    msgEl.value  = '';
    const sl = document.getElementById('subj-label');
    if (sl) { sl.textContent = t('frm.s0'); sl.style.color = 'rgba(255,255,255,.38)'; }
  } catch {
    tg?.sendData?.(JSON.stringify({ type: 'support', subject: subj, message: msg, user_id: tgU.id || 0 }));
    toast(t('toast.sent'));
  } finally {
    if (btn) { btn.style.opacity = ''; btn.style.pointerEvents = ''; }
  }
}
