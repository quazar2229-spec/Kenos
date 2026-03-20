'use strict';
/* ══════════════════════════════════════
   APP — инициализация приложения
   Запускается последним после всех JS
   Содержит: Telegram init, loader,
   toast, clipboard, loader прогресс
══════════════════════════════════════ */

/* ── Telegram WebApp ── */
const tg  = window.Telegram?.WebApp;
if (tg) { tg.ready(); tg.expand(); tg.setHeaderColor('#0d0d0d'); tg.setBackgroundColor('#0d0d0d'); }
const tgU = tg?.initDataUnsafe?.user || {};

/* ── Toast — всплывающее уведомление ── */
const toastEl = document.getElementById('toast');
let _toastTimer = 0;
function toast(msg) {
  clearTimeout(_toastTimer);
  toastEl.textContent = msg;
  toastEl.classList.add('show');
  _toastTimer = setTimeout(() => toastEl.classList.remove('show'), 2100);
}

/* ── Clipboard — копирование в буфер ── */
function copy(str) {
  (navigator.clipboard?.writeText(str) || Promise.reject())
    .then(() => toast(t('toast.cp')))
    .catch(() => {
      const a = document.createElement('textarea');
      a.value = str;
      Object.assign(a.style, { position: 'fixed', opacity: '0' });
      document.body.appendChild(a);
      a.select();
      document.execCommand('copy');
      document.body.removeChild(a);
      toast(t('toast.cp'));
    });
}

/* Кнопка копирования HWID */
document.getElementById('cp-hwid').onclick = () => {
  const v = document.getElementById('hwid-v').textContent;
  v && v !== '—' ? copy(v) : toast('HWID не привязан');
};

/* Запрет копирования контента страницы */
document.addEventListener('copy',        e => e.preventDefault(), { passive: false });
document.addEventListener('contextmenu', e => e.preventDefault(), { passive: false });

/* ── Loader — анимированный прогресс-бар ── */
(function initLoader() {
  const fill   = document.getElementById('ldfill');
  const pct    = document.getElementById('ldpct');
  const status = document.getElementById('ldstatus');
  const ld     = document.getElementById('loader');
  if (!fill || !pct) return;

  fill.style.width           = '100%';
  fill.style.transform       = 'scaleX(0)';
  fill.style.transformOrigin = 'left';
  fill.style.transition      = 'none';

  /* Статусы на определённых процентах */
  const STAGES = [
    { at:  5, text: 'Инициализация...'    },
    { at: 22, text: 'Загрузка модулей...' },
    { at: 48, text: 'Проверка лицензии...' },
    { at: 72, text: 'Синхронизация...'    },
    { at: 91, text: 'Почти готово...'     },
    { at: 100, text: 'Добро пожаловать'   },
  ];
  let stageIdx = 0;
  let cur = 0;
  const DUR = 4200;
  let t0 = 0;

  function ease(x) { return x < .5 ? 8 * x * x * x * x : 1 - Math.pow(-2 * x + 2, 4) / 2; }

  function tick(now) {
    if (!t0) t0 = now;
    const p = Math.min((now - t0) / DUR, 1);
    const v = Math.round(ease(p) * 100);

    if (v !== cur) {
      cur = v;
      fill.style.transform = `scaleX(${v / 100})`;
      pct.textContent = v + '%';

      if (v >= 100) fill.style.boxShadow = '0 0 28px 6px rgba(255,255,255,.8)';

      if (status && stageIdx < STAGES.length && v >= STAGES[stageIdx].at) {
        status.style.opacity = '0';
        const txt = STAGES[stageIdx].text;
        setTimeout(() => { status.textContent = txt; status.style.opacity = '1'; }, 120);
        stageIdx++;
      }
    }

    if (p < 1) {
      requestAnimationFrame(tick);
    } else {
      setTimeout(() => {
        ld.classList.add('hidden');
        document.getElementById('app')?.classList.add('ready');
        moveSlider(document.getElementById('n-home'));
      }, 250);
    }
  }

  /* Старт через 1с — дать CSS-анимациям отыграть */
  setTimeout(() => requestAnimationFrame(tick), 1000);
})();

/* ── Старт: переводы и данные ── */
setLang('ru');
loadKey();
loadCL();
loadInfo();
checkStatus();
loadReviews();
