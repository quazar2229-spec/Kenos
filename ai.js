'use strict';
/* ══════════════════════════════════════
   САКУРА v4 — оптимизированная анимация
   • Sin/Cos LUT (1024) — ноль Math.trig в hot loop
   • ImageBitmap — GPU текстуры без CPU→GPU копии
   • Адаптивный FPS — 30fps на слабых, 60fps на нормальных
   • Адаптивное количество лепестков по RAM/CPU
══════════════════════════════════════ */

const _UA  = navigator.userAgent;
const _MOB = /iPhone|iPad|iPod|Android/i.test(_UA);

/* ── Sin/Cos таблица (LUT) ── */
const LUT  = 1024;
const LM   = LUT - 1;
const SINT = new Float32Array(LUT);
const COST = new Float32Array(LUT);
for (let i = 0; i < LUT; i++) {
  const a = i / LUT * 6.2831853;
  SINT[i] = Math.sin(a);
  COST[i] = Math.cos(a);
}

/* ── Цвета лепестков ── */
const _PC = [
  ['rgba(255,210,225,.82)', 'rgba(240,130,165,.60)', 'rgba(210,70,110,0)'],
  ['rgba(255,200,218,.78)', 'rgba(230,115,155,.55)', 'rgba(195,60,100,0)'],
  ['rgba(255,220,232,.82)', 'rgba(245,145,175,.60)', 'rgba(215,80,120,0)'],
  ['rgba(250,190,210,.78)', 'rgba(225,105,145,.55)', 'rgba(190,50,90,0)'],
];
const _SZ = [3, 5, 7, 9, 12, 15];
const _CN = 4;

/* ── Рисуем один лепесток на offscreen canvas ── */
function _rPetal(cx, r, ci) {
  const c = _PC[ci], w = r, h = r * 1.6;
  cx.beginPath(); cx.moveTo(0, -h);
  cx.bezierCurveTo(w * .8, -h * .5, w, h * .2, 0, h);
  cx.bezierCurveTo(-w, h * .2, -w * .8, -h * .5, 0, -h);
  cx.closePath();
  const g = cx.createRadialGradient(0, -h * .15, r * .1, 0, 0, h);
  g.addColorStop(0, c[0]); g.addColorStop(.4, c[1]); g.addColorStop(1, c[2]);
  cx.fillStyle = g; cx.fill();
  cx.beginPath(); cx.moveTo(0, -h * .9); cx.quadraticCurveTo(w * .15, 0, 0, h * .85);
  cx.strokeStyle = 'rgba(200,80,120,.25)'; cx.lineWidth = r * .055; cx.stroke();
}

/* ── Кэш canvas + GPU текстуры (ImageBitmap) ── */
const _cvs = new Map(), _bmp = new Map();
let _bmpOk = false;

(async () => {
  const ps = [];
  _SZ.forEach(r => {
    for (let c = 0; c < _CN; c++) {
      const p = r * 2.2, s = Math.ceil(p * 2), k = r + '_' + c;
      const cc = document.createElement('canvas');
      cc.width = cc.height = s;
      const cx = cc.getContext('2d');
      cx.translate(p, p);
      _rPetal(cx, r, c);
      _cvs.set(k, cc);
      if (typeof createImageBitmap === 'function')
        ps.push(createImageBitmap(cc).then(b => _bmp.set(k, b)));
    }
  });
  if (ps.length) { await Promise.all(ps); _bmpOk = true; }
})();

/* ── Основная функция инициализации ── */
function initSakura(id, count, alpha) {
  const cvs = document.getElementById(id);
  if (!cvs) return;
  const ctx = cvs.getContext('2d', { alpha: true, desynchronized: true });
  let W = 0, H = 0;

  /* STRIDE 9: x, y, sizeIdx, colorIdx, vx, vy, angleIdx, angleVel, opacity */
  const S   = 9;
  const buf = new Float32Array(count * S);
  const sw  = new Uint16Array(count); /* swing speed */
  const sp  = new Uint16Array(count); /* swing phase */

  function spawn(i, top) {
    const b = i * S;
    buf[b]     = Math.random() * W;
    buf[b + 1] = top ? Math.random() * H : -50;
    buf[b + 2] = (Math.random() * _SZ.length) | 0;
    buf[b + 3] = (Math.random() * _CN) | 0;
    buf[b + 4] = (Math.random() - .5) * .55;
    buf[b + 5] = .16 + Math.random() * .32;
    buf[b + 6] = (Math.random() * LUT) | 0;
    buf[b + 7] = (Math.random() < .5 ? 1 : -1) * (1 + (Math.random() * 2) | 0);
    buf[b + 8] = .58 + Math.random() * .42;
    sp[i] = (Math.random() * LUT) | 0;
    sw[i] = 1 + (Math.random() * 2) | 0;
  }

  function resize() { W = cvs.width = window.innerWidth; H = cvs.height = window.innerHeight; }
  new ResizeObserver(resize).observe(document.documentElement);
  resize();
  for (let i = 0; i < count; i++) spawn(i, true);

  let alive = true, lastT = 0;
  const isMobWeak = _MOB && (navigator.deviceMemory || 4) <= 2;
  const CAP = isMobWeak ? 1000 / 30 : 1000 / 63; /* слабые = 30fps, остальные = 60fps */

  document.addEventListener('visibilitychange', () => {
    alive = !document.hidden;
    if (alive) requestAnimationFrame(fr);
  }, { passive: true });

  function fr(now) {
    if (!alive) return;
    if (now - lastT < CAP) { requestAnimationFrame(fr); return; }
    lastT = now;
    ctx.clearRect(0, 0, W, H);
    const useB = _bmpOk;

    for (let i = 0; i < count; i++) {
      const b = i * S;
      sp[i] = (sp[i] + sw[i]) & LM;
      buf[b]     += buf[b + 4] + SINT[sp[i]] * (.35 + buf[b + 8] * .25) * .22;
      buf[b + 1] += buf[b + 5];
      buf[b + 6]  = (buf[b + 6] + buf[b + 7] + LUT) & LM;

      if (buf[b + 1] > H + 60 || buf[b] < -80 || buf[b] > W + 80) { spawn(i, false); continue; }

      const k   = _SZ[buf[b + 2]] + '_' + buf[b + 3];
      const img = (useB ? _bmp : _cvs).get(k);
      if (!img) continue;

      const ai = buf[b + 6] | 0;
      ctx.globalAlpha = buf[b + 8] * alpha;
      ctx.setTransform(COST[ai], SINT[ai], -SINT[ai], COST[ai], buf[b], buf[b + 1]);
      ctx.drawImage(img, -(img.width >> 1), -(img.width >> 1));
    }
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.globalAlpha = 1;
    requestAnimationFrame(fr);
  }
  requestAnimationFrame(fr);
}

/* ── Запуск с адаптивным количеством лепестков ── */
(function startSakura() {
  const mem    = navigator.deviceMemory       || 4;
  const cpu    = navigator.hardwareConcurrency || 4;
  const isMob  = _MOB;
  let n;
  if (isMob && (mem <= 2 || cpu <= 4)) n = 6;   /* слабый телефон */
  else if (isMob)                       n = 12;  /* нормальный телефон */
  else                                  n = 20;  /* ПК */

  initSakura('cvs-sakura', n, .72);
  initSakura('cvs-loader', Math.min(n, 10), .72);
})();
