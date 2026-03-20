'use strict';
/* ══════════════════════════════════════
   НАВИГАЦИЯ — переключение разделов
   go(id)        — перейти в раздел
   moveSlider()  — двигает подсветку в навбаре
══════════════════════════════════════ */

const SEC_NAV = {
  's-home': 'n-home',
  's-buy':  'n-buy',
  's-pay':  'n-pay',
  's-key':  'n-key',
  's-hlp':  'n-hlp',
  's-inf':  'n-inf',
  's-adm':  'n-adm',
  's-abt':  'n-abt',
  's-ai':   'n-ai',
};

let _cur = 's-home';

function moveSlider(navItemEl) {
  const slider = document.getElementById('nav-slider');
  if (!slider || !navItemEl) return;
  const nav  = slider.parentElement;
  const navW = nav.offsetWidth;
  const w = Math.min(navItemEl.offsetWidth, navW - 8);
  const x = Math.max(4, Math.min(navItemEl.offsetLeft, navW - w - 4));
  slider.style.transform = 'translateX(' + x + 'px)';
  slider.style.width = w + 'px';
}

function go(id) {
  if (id === _cur) return;

  const prev = document.getElementById(_cur);
  const next = document.getElementById(id);
  if (!next) return;

  _cur = id;

  /* 1. Старая секция — плавно уходит вверх */
  if (prev) {
    prev.style.transition = 'opacity .28s cubic-bezier(.25,.46,.45,.94), transform .28s cubic-bezier(.25,.46,.45,.94)';
    prev.style.opacity = '0';
    prev.style.transform = 'translateY(-6px)';
    prev.style.pointerEvents = 'none';
    setTimeout(() => {
      prev.classList.remove('on');
      prev.style.transition = '';
      prev.style.opacity = '';
      prev.style.transform = '';
      prev.style.pointerEvents = '';
    }, 300);
  }

  /* 2. Скролл вверх мгновенно */
  window.scrollTo({ top: 0, behavior: 'instant' });

  /* 3. Новая секция появляется */
  setTimeout(() => { next.classList.add('on'); }, 60);

  /* 4. Обновляем навбар */
  document.querySelectorAll('.ni').forEach(n => n.classList.remove('on'));
  const activeNav = document.getElementById(SEC_NAV[id]);
  activeNav?.classList.add('on');
  moveSlider(activeNav);

  /* 5. AI input bar — показываем/скрываем */
  const aiBar = document.getElementById('ai-bar');
  const aiSec = document.getElementById('s-ai');
  const nav2  = document.getElementById('nav');

  if (aiBar) {
    if (id === 's-ai') {
      aiBar.style.animation = 'none';
      aiBar.style.display = 'block';
      requestAnimationFrame(() => {
        aiBar.style.animation = 'aiBarIn .45s cubic-bezier(.16,1,.3,1) both';
      });
    } else {
      if (nav2) { nav2.style.transform = ''; nav2.style.opacity = ''; nav2.style.transition = ''; }
      if (aiSec) { aiSec.style.bottom = ''; aiSec.style.transition = ''; }
      aiBar.style.animation = 'aiBarOut .22s cubic-bezier(.16,1,.3,1) both';
      setTimeout(() => { aiBar.style.display = 'none'; aiBar.style.animation = 'none'; }, 230);
    }
  }
}

/* ── Touch на навбаре — только реальный тап, не скролл ── */
(function initNavTouch() {
  if (!('ontouchstart' in window)) return;

  document.querySelectorAll('.ni').forEach(el => {
    let _sx = 0, _sy = 0, _moved = false;

    el.addEventListener('touchstart', e => {
      _sx = e.touches[0].clientX;
      _sy = e.touches[0].clientY;
      _moved = false;
    }, { passive: true });

    el.addEventListener('touchmove', e => {
      if (Math.abs(e.touches[0].clientX - _sx) > 8 ||
          Math.abs(e.touches[0].clientY - _sy) > 8) {
        _moved = true;
      }
    }, { passive: true });

    el.addEventListener('touchend', e => {
      if (_moved) return;
      e.preventDefault();
      const onclick = el.getAttribute('onclick');
      if (onclick) eval(onclick); // eslint-disable-line no-eval
    }, { passive: false });
  });

  document.addEventListener('touchmove', () => {}, { passive: true });
})();
