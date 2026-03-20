
* { -webkit-tap-highlight-color: transparent; }
@supports (animation-timeline: scroll()) { * { -webkit-optimize-contrast: auto; } }
/* ═══════════════════════════════════════════
   TOKENS
═══════════════════════════════════════════ */
:root{
  --fd:'Cormorant Garamond',serif;
  --fb:'Rajdhani',sans-serif;
  --bg:#0d0d0d;
  --panel:rgba(14,14,14,.97);
  --card:rgba(14,14,14,.97);
  --g5:#555;--g4:#999;
  --ease:cubic-bezier(.25,.46,.45,.94);
  --fast:.28s;--mid:.38s;
  --smooth:cubic-bezier(.25,.46,.45,.94);
  /* iOS-style spring — для кнопок и карточек */
  --spring:cubic-bezier(.34,1.2,.64,1);
  --ac:rgba(255,255,255,1);
  --ac-dim:rgba(255,255,255,.08);
  --ac-glow:rgba(255,255,255,.5);
  --ac-soft:rgba(255,255,255,.22);
  --slider-bg:rgba(255,255,255,.07);
  --txt:#fff;
  --txt-dim:rgba(255,255,255,.6);
  --txt-muted:rgba(255,255,255,.22);
  --border:rgba(255,255,255,.08);
  --border-ac:rgba(255,255,255,.18);
}

/* ═══════════════════════════════════════════
   RESET + BASE
═══════════════════════════════════════════ */
*,*::before,*::after{
  margin:0;padding:0;box-sizing:border-box;
  -webkit-tap-highlight-color:transparent;
  /* убираем задержку 300ms на тап iOS/Android */
  touch-action:manipulation;
}
html{
  scroll-behavior:smooth;
  /* предотвращает резиновый скролл на iOS */
  height:100%;
}
body{
  background:var(--bg);color:#fff;
  font-family:var(--fb);
  min-height:100%;
  /* плавный скролл на iOS */
  -webkit-overflow-scrolling:touch;
  overflow-x:hidden;
  overscroll-behavior:none;
  -webkit-user-select:none;user-select:none;
  /* отключаем автоматическое масштабирование шрифта на iOS */
  -webkit-text-size-adjust:100%;
  text-size-adjust:100%;
}
input,textarea,select{
  -webkit-user-select:text;user-select:text;
  font-size:16px;
}
textarea{
  resize:none;
  -webkit-appearance:none;
  appearance:none;
}
a{color:inherit}

/* кнопки и кликабельные эл-ты — мгновенный отклик */
button,.ni,.svc-card,.btn,.lpill,.dlink{
  touch-action:manipulation;
  cursor:pointer;
}

/* ═══════════════════════════════════════════
   SAKURA CANVAS — GPU-слой z:0
═══════════════════════════════════════════ */
#cvs-sakura{
  position:fixed;top:0;left:0;
  width:100%;height:100%;
  z-index:0;pointer-events:none;
  opacity:.60;
}

/* ═══════════════════════════════════════════
   LOADER
═══════════════════════════════════════════ */
#loader{
  position:fixed;inset:0;z-index:9000;
  display:flex;flex-direction:column;
  align-items:center;justify-content:center;
  background:#0d0d0d;
  transition:opacity .45s ease-out, visibility .45s;
}
#loader.hidden{opacity:0;visibility:hidden;pointer-events:none}
#cvs-loader{position:absolute;inset:0;pointer-events:none;transform:translateZ(0);opacity:.60}

/* уголки */
.lc{position:absolute;width:36px;height:36px;opacity:0;animation:fadeIn .7s 2.2s ease forwards}
.lc-tl{top:28px;left:28px;border-top:1px solid rgba(255,255,255,.3);border-left:1px solid rgba(255,255,255,.3)}
.lc-tr{top:28px;right:28px;border-top:1px solid rgba(255,255,255,.3);border-right:1px solid rgba(255,255,255,.3)}
.lc-bl{bottom:28px;left:28px;border-bottom:1px solid rgba(255,255,255,.3);border-left:1px solid rgba(255,255,255,.3)}
.lc-br{bottom:28px;right:28px;border-bottom:1px solid rgba(255,255,255,.3);border-right:1px solid rgba(255,255,255,.3)}

.lc::before{
  content:'';position:absolute;width:3px;height:3px;
  border-radius:50%;background:rgba(255,255,255,.5);
}
.lc-tl::before{top:-1px;left:-1px}
.lc-tr::before{top:-1px;right:-1px}
.lc-bl::before{bottom:-1px;left:-1px}
.lc-br::before{bottom:-1px;right:-1px}

.ld{position:relative;z-index:1;display:flex;flex-direction:column;align-items:center;gap:18px}
.ld-eye{
  font-size:9px;letter-spacing:.55em;color:rgba(255,255,255,.25);
  text-transform:uppercase;opacity:0;
  animation:fadeIn .7s .6s ease forwards;
}
.ld-title{
  font-family:var(--fd);font-size:76px;font-weight:300;
  letter-spacing:.35em;color:#fff;line-height:1;
  padding-left:.35em;text-align:center;
  opacity:0;
  animation:riseIn 1.1s .3s var(--ease) forwards, glowPulse 4s 1.5s ease-in-out infinite;
}
.ld-prog{display:flex;flex-direction:column;align-items:center;gap:9px;opacity:0;animation:fadeIn .6s 1.0s ease forwards}
.ld-pct{
  font-size:10px;font-weight:500;letter-spacing:.28em;
  color:rgba(255,255,255,.45);font-variant-numeric:tabular-nums;
}
.ld-track{
  width:220px;height:1px;
  background:rgba(255,255,255,.08);
  border-radius:1px;overflow:hidden;position:relative;
}
.ld-fill{
  position:absolute;inset-block:0;left:0;width:100%;
  background:linear-gradient(90deg,transparent,rgba(255,255,255,.9) 40%,rgba(255,255,255,.6),transparent);
  box-shadow:0 0 16px rgba(255,255,255,.6);
  transform-origin:left;
  will-change:transform;
}
.ld-sub{
  font-size:10px;letter-spacing:.12em;color:rgba(255,255,255,.18);
  text-align:center;line-height:2.3;max-width:220px;
  opacity:0;animation:fadeIn .7s 1.8s ease forwards;
}
.ld-status{
  font-size:9px;letter-spacing:.2em;color:rgba(255,255,255,.2);
  text-transform:uppercase;opacity:0;
  animation:fadeIn .5s 1.2s ease forwards;
  min-height:13px;
  transition:opacity .3s ease;
}

/* ═══════════════════════════════════════════
   PERFORMANCE — GPU layers
═══════════════════════════════════════════ */
.panel{ contain:layout style; }
.ni{ contain:layout style; }
#cvs-sakura{ transform:translateZ(0); }
.nav{ transform:translateZ(0); }
.hdr{ transform:translateZ(0); }

@keyframes glowPulse{
  0%,100%{text-shadow:0 0 8px rgba(255,255,255,.06)}
  50%    {text-shadow:0 0 35px var(--ac-soft),0 0 80px rgba(255,255,255,.08),0 0 2px #fff}
}

/* ═══════════════════════════════════════════
   KEYFRAMES — минимальный набор
═══════════════════════════════════════════ */
@keyframes fadeIn{to{opacity:1}}
@keyframes riseIn{from{opacity:0;transform:translateY(14px)}to{opacity:1;transform:translateY(0)}}
@keyframes glow{
  0%,100%{text-shadow:0 0 6px rgba(255,255,255,.08)}
  50%    {text-shadow:0 0 26px rgba(255,255,255,.55),0 0 60px rgba(255,255,255,.14)}
}
@keyframes riseUp{
  from{opacity:0;transform:translateY(20px)}
  to  {opacity:1;transform:translateY(0)}
}
@keyframes navIn{
  0%  {opacity:.2}
  100%{opacity:1}
}
/* navGlow удалён — filter:drop-shadow в keyframes = 30fps на iPhone */
@keyframes pulse{
  0%,100%{opacity:1;transform:scale(1)}
  50%    {opacity:.28;transform:scale(.55)}
}
@keyframes btnGlow{
  0%,100%{box-shadow:0 4px 16px rgba(255,255,255,.07)}
  50%    {box-shadow:0 4px 26px rgba(255,255,255,.22),0 0 14px rgba(255,255,255,.06)}
}
@keyframes logoGlow{
  0%,100%{opacity:.7}
  50%    {opacity:1;text-shadow:0 0 20px rgba(255,255,255,.5),0 0 48px rgba(255,255,255,.14)}
}
@keyframes panelShine{
  0%,100%{box-shadow:0 4px 20px rgba(0,0,0,.45)}
  50%    {box-shadow:0 4px 20px rgba(0,0,0,.45),0 0 18px rgba(255,255,255,.035)}
}
@keyframes dotPulse{
  0%,100%{opacity:1;transform:scale(1)}
  50%    {opacity:.25;transform:scale(.5)}
}
@keyframes ambient{
  0%,100%{opacity:.45}
  50%    {opacity:.85}
}
/* glowPulse переведён на opacity — text-shadow анимация вызывает repaint, opacity — нет */
@keyframes heroPulse{
  0%,100%{ opacity:.88 }
  50%    { opacity:1   }
}
.hero-h{ animation:heroPulse 4s 1.5s ease-in-out infinite; }
.logo-t{ animation:logoGlow 6s 1s ease-in-out infinite; }

/* ═══════════════════════════════════════════
   APP SHELL
═══════════════════════════════════════════ */
#app{
  position:relative;z-index:1;
  max-width:480px;margin:0 auto;
  padding-bottom:70px;min-height:100vh;
  /* скрыт до конца лоадера — нет мигания контента */
  opacity:0;
  transition:opacity .3s ease;
}
#app.ready{ opacity:1; }

/* ambient убран — постоянная анимация opacity грела GPU */
#app::before{ display:none; }

/* ═══════════════════════════════════════════
   HEADER
═══════════════════════════════════════════ */
.hdr{
  position:sticky;top:0;z-index:100;
  padding:10px 16px;
  display:flex;align-items:center;justify-content:space-between;
  background:#0b0b0b;
  border-bottom:1px solid rgba(255,255,255,.08);
  overflow:visible;
}
.hdr-profile{display:flex;align-items:center;gap:8px}
.hdr-profile{display:flex;align-items:center;gap:8px;position:relative}

/* обёртка для колец */
.hdr-avatar-wrap{position:relative;flex-shrink:0}

/* Кольцо 1 — внутреннее, крутится по часовой */
.hdr-avatar-wrap::before{
  content:'';
  position:absolute;inset:-3px;
  border-radius:50%;
  border:1.5px solid transparent;
  border-top-color:var(--ac);
  border-right-color:var(--ac-dim);
  animation:spin1 6s linear infinite;
  pointer-events:none;
  z-index:2;
}
/* Кольцо 2 — внешнее, крутится против часовой */
.hdr-avatar-wrap::after{
  content:'';
  position:absolute;inset:-7px;
  border-radius:50%;
  border:1px solid transparent;
  border-bottom-color:var(--ac-soft);
  border-left-color:var(--ac-dim);
  animation:spin2 10s linear infinite;
  pointer-events:none;
  z-index:1;
}
@keyframes spin1{ to{ transform:rotate(360deg) } }
@keyframes spin2{ to{ transform:rotate(-360deg) } }

.hdr-avatar{
  width:30px;height:30px;border-radius:50%;
  border:1px solid var(--border);
  background:var(--ac-dim);
  display:flex;align-items:center;justify-content:center;
  overflow:hidden;color:var(--txt-muted);
}
.hdr-avatar img{width:100%;height:100%;object-fit:cover;border-radius:50%}
.hdr-name{font-size:11px;font-weight:500;letter-spacing:.06em;color:var(--txt-dim);max-width:86px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.hdr-logo{display:flex;align-items:center;gap:5px}
.logo-t{font-family:var(--fd);font-size:24px;font-weight:300;letter-spacing:.22em;color:var(--txt)}
.logo-d{width:5px;height:5px;border-radius:50%;background:var(--ac);box-shadow:0 0 6px var(--ac-glow)}
.lang-sw{
  display:flex;position:relative;
  background:rgba(255,255,255,.04);
  border:1px solid var(--border);
  border-radius:20px;
  padding:2px;
  gap:0;
}
/* ползунок — строго половина контейнера */
.lang-sw::before{
  content:'';
  position:absolute;
  top:2px;bottom:2px;left:2px;
  width:calc(50% - 2px);
  background:var(--ac-dim);
  border-radius:16px;
  transition:transform .28s cubic-bezier(.16,1,.3,1);
  pointer-events:none;
}
.lang-sw[data-lang="en"]::before{
  transform:translateX(100%);
}
.lang-sw[data-lang="ru"]::before{
  transform:translateX(0);
}
.lang-b{
  position:relative;z-index:1;
  padding:4px 0;
  width:36px;text-align:center;
  font-family:var(--fb);font-size:10px;font-weight:500;letter-spacing:.1em;
  color:var(--txt-muted);background:transparent;border:none;cursor:pointer;
  border-radius:16px;
  transition:color .22s ease;
}
.lang-b.on{ color:var(--txt); }
/* обёртка хедер-справа */
.hdr-right{display:flex;align-items:center;gap:7px}

/* ═══════════════════════════════════════════
   SECTIONS — плавные переходы
═══════════════════════════════════════════ */
#view{
  position:relative;
  min-height:calc(100vh - 60px);
}

/* ── Скрытая секция ── */
.sec{
  position:absolute;
  top:0;left:0;right:0;
  opacity:0;
  pointer-events:none;
  visibility:hidden;
  transition:
    opacity    .45s cubic-bezier(.25,.46,.45,.94),
    transform  .45s cubic-bezier(.25,.46,.45,.94),
    visibility .45s;
  transform:translateY(16px);
  /* will-change УБРАН с базового состояния — 9 GPU-слоёв одновременно = перегруз compositor */
}

/* ── Исходящая секция ── */
.sec.leaving{
  opacity:0;
  transform:translateY(-6px);
  pointer-events:none;
  visibility:hidden;
  will-change:opacity,transform; /* только пока анимируется */
}

.sec.on{
  position:relative;
  opacity:1;
  pointer-events:auto;
  visibility:visible;
  transform:translateY(0);
  will-change:auto; /* сбрасываем после появления — GPU слой больше не нужен */
}

.sec.on{
  position:relative;
  opacity:1;
  pointer-events:auto;
  visibility:visible;
  transform:translateY(0);
}

/* ══════════════════════════════════════
   FADE-IN элементов — единый transition
/* Акценты через тему */

/* ══════════════════════════════════════
   ВОЛНОВОЕ ПОЯВЛЕНИЕ КОНТЕНТА
   Каждый блок появляется снизу вверх
   с небольшим смещением — эффект волны
══════════════════════════════════════ */
/* ══ AI секция — плавные анимации ══ */
@keyframes bubbleIn{
  from{opacity:0;transform:translateY(12px)}
  to  {opacity:1;transform:translateY(0)}
}
.ai-bubble{
  animation:bubbleIn .65s cubic-bezier(.16,1,.3,1) both;
}
#s-ai.on > div:first-child p  { animation:waveIn .55s .03s cubic-bezier(.16,1,.3,1) both; }
#s-ai.on > div:first-child h1  { animation:waveIn .65s .08s cubic-bezier(.16,1,.3,1) both; }
#s-ai.on > div:first-child > div:nth-child(3){ animation:waveIn .55s .14s cubic-bezier(.16,1,.3,1) both; }
#s-ai.on #ai-empty            { animation:waveIn .75s .22s cubic-bezier(.16,1,.3,1) both; }

@keyframes waveIn{
  from{ opacity:0; transform:translateY(18px) scale(.98); }
  to  { opacity:1; transform:translateY(0) scale(1); }
}

.sec.on .hero,
.sec.on .sec-title,
.sec.on .svc-grid,
.sec.on .panel,
.sec.on .links,
.sec.on .btn{
  animation: waveIn .75s cubic-bezier(.16,1,.3,1) both;
}

.sec.on .hero                 { animation-delay:.03s }
.sec.on .sec-title            { animation-delay:.05s }
.sec.on .svc-grid             { animation-delay:.07s }
.sec.on .panel:nth-child(1)  { animation-delay:.08s }
.sec.on .panel:nth-child(2)  { animation-delay:.18s }
.sec.on .panel:nth-child(3)  { animation-delay:.28s }
.sec.on .panel:nth-child(4)  { animation-delay:.38s }
.sec.on .panel:nth-child(5)  { animation-delay:.48s }
.sec.on .panel:nth-child(6)  { animation-delay:.58s }
.sec.on .links                { animation-delay:.18s }

/* Кнопки внутри панелей — волна после панели */
.sec.on .btn:nth-of-type(1){ animation-delay:.22s }
.sec.on .btn:nth-of-type(2){ animation-delay:.30s }
.sec.on .btn.mt             { animation-delay:.30s }

.sec.on .hero-eye{ animation:waveIn .75s .06s cubic-bezier(.16,1,.3,1) both }
.sec.on .hero-h  { animation:waveIn .85s .14s cubic-bezier(.16,1,.3,1) both }
.sec.on .hero-sub{ animation:waveIn .75s .22s cubic-bezier(.16,1,.3,1) both }
.sec.on .divider { animation:waveIn .7s  .28s cubic-bezier(.16,1,.3,1) both }

.sec.on .svc-card:nth-child(1){ animation:waveIn .75s .10s cubic-bezier(.16,1,.3,1) both }
.sec.on .svc-card:nth-child(2){ animation:waveIn .75s .20s cubic-bezier(.16,1,.3,1) both }
.sec.on .svc-card:nth-child(3){ animation:waveIn .75s .30s cubic-bezier(.16,1,.3,1) both }
.sec.on .svc-card:nth-child(4){ animation:waveIn .75s .40s cubic-bezier(.16,1,.3,1) both }

.sec.on .info-r{ animation:none; opacity:1; }

@media(prefers-reduced-motion:reduce){
  .sec.on .hero,.sec.on .sec-title,.sec.on .svc-grid,.sec.on .panel,
  .sec.on .links,.sec.on .hero-eye,.sec.on .hero-h,.sec.on .hero-sub,
  .sec.on .divider,.sec.on .svc-card,.sec.on .info-r{
    animation:none;opacity:1;transform:none;
  }
}
.divider{
  background:linear-gradient(90deg,transparent,var(--ac-soft),transparent);
}
/* svc-card hover — розовый акцент */
.svc-card:hover{
  transform:translateY(-2px) scale(1.02);
  border-color:var(--ac-soft);
  box-shadow:0 8px 22px rgba(0,0,0,.5), 0 0 14px var(--ac-dim);
}
/* form focus — розовый */
.form-i:focus,.form-ta:focus{
  border-color:var(--ac-soft);
  background:rgba(255,255,255,.04);
  box-shadow:0 0 0 1px var(--ac-dim);
}
.hero{padding:40px 20px 14px;text-align:center}
.hero-eye{font-size:10px;letter-spacing:.38em;color:var(--g5);text-transform:uppercase;margin-bottom:11px}
/* KENOS hero */
.hero-h{
  font-family:var(--fd);font-size:clamp(58px,15vw,88px);
  font-weight:300;letter-spacing:.14em;line-height:1;
  color:var(--txt);display:block;text-align:center;
  cursor:default;
}
.hero-sub{font-size:11px;letter-spacing:.22em;color:var(--g5);text-transform:uppercase}
.divider{width:34px;height:1px;margin:16px auto}
.sec-title{font-size:10px;letter-spacing:.3em;text-transform:uppercase;color:var(--g5);padding:0 16px 9px}

/* ═══════════════════════════════════════════
   GLASS PANEL
   contain:layout style paint — изолируем
═══════════════════════════════════════════ */
.panel{
  margin:0 16px 13px;padding:20px;
  background:var(--panel);
  border:1px solid var(--border);
  border-radius:16px;
  position:relative;overflow:hidden;
  contain:layout style paint;
}

/* блик */
.panel::before{
  content:'';position:absolute;
  top:0;left:10%;right:10%;height:1px;
  background:linear-gradient(90deg,transparent,var(--border-ac),transparent);
  pointer-events:none;
}
.p-title{font-family:var(--fd);font-size:21px;font-weight:300;letter-spacing:.1em;color:var(--txt);margin-bottom:3px;display:flex;align-items:center;gap:8px}
.p-sub{font-size:10px;color:var(--g5);letter-spacing:.12em;text-transform:uppercase;margin-bottom:13px}
.p-body{font-size:13px;line-height:1.85;color:var(--txt-dim)}

/* ═══════════════════════════════════════════
   SERVICE GRID
═══════════════════════════════════════════ */
.svc-grid{padding:0 16px;display:grid;grid-template-columns:1fr 1fr;gap:10px;margin-bottom:16px}
.svc-card{
  -webkit-tap-highlight-color:transparent;
  background:rgba(14,14,14,.97);border:1px solid var(--border);
  border-radius:16px;padding:18px 14px 14px;
  cursor:pointer;position:relative;overflow:hidden;
  transition:transform .1s ease,border-color .18s ease,box-shadow .18s ease;
  box-shadow:0 4px 20px rgba(0,0,0,.45);
}
.svc-card::before{content:'';position:absolute;top:0;left:0;right:0;height:1px;background:linear-gradient(90deg,transparent,var(--border-ac),transparent);pointer-events:none}
.svc-card::after{content:'';position:absolute;inset:0;background:radial-gradient(circle at 50% 0%,var(--ac-dim) 0%,transparent 58%);pointer-events:none}
.svc-card:active{transform:scale(.94);transition:transform .06s ease}
.svc-ico{
  display:flex;justify-content:center;align-items:center;
  margin-bottom:12px;color:var(--txt-dim);
  position:relative;z-index:1;
}
.svc-card:nth-child(1) .svc-ico{animation-delay:0s}
.svc-card:nth-child(2) .svc-ico{animation-delay:-1s}
.svc-card:nth-child(3) .svc-ico{animation-delay:-2s}
.svc-card:nth-child(4) .svc-ico{animation-delay:-3s}
.svc-name{font-size:11px;font-weight:600;letter-spacing:.13em;text-transform:uppercase;color:var(--txt);margin-bottom:3px;position:relative;z-index:1}
.svc-desc{font-size:10px;color:var(--g5);position:relative;z-index:1}

/* ── INFO ENTRIES — тех работы и объявления ── */
.info-entry{
  padding:14px 0;border-bottom:1px solid var(--border);
  position:relative;
}
.info-entry:last-child{border-bottom:none}
.info-badge{
  display:inline-flex;align-items:center;gap:5px;
  padding:3px 9px;border-radius:6px;
  font-size:9px;font-weight:600;letter-spacing:.1em;text-transform:uppercase;
  margin-bottom:8px;
}
.info-badge.tech{background:rgba(255,255,255,.06);border:1px solid rgba(255,255,255,.12);color:rgba(200,200,200,.8)}
.info-badge.info{background:rgba(255,255,255,.06);border:1px solid rgba(255,255,255,.12);color:rgba(200,200,200,.8)}
.info-badge.ok  {background:rgba(255,255,255,.06);border:1px solid rgba(255,255,255,.12);color:rgba(200,200,200,.8)}
.info-badge.warn{background:rgba(255,255,255,.06);border:1px solid rgba(255,255,255,.12);color:rgba(200,200,200,.8)}
.info-badge svg{opacity:.8}
.info-e-title{font-size:14px;font-weight:500;color:var(--txt);letter-spacing:.04em;margin-bottom:5px}
.info-e-body{font-size:12px;color:var(--txt-dim);line-height:1.75}
.info-e-date{font-size:10px;color:var(--g5);letter-spacing:.1em;margin-top:6px}
.info-empty{text-align:center;padding:32px 0;color:var(--g5);font-size:12px;letter-spacing:.1em}
.info-dot{
  position:absolute;top:14px;right:0;
  width:7px;height:7px;border-radius:50%;
  background:rgba(200,200,200,.5);
  animation:dotPulse 2s ease-in-out infinite;
}
/* badge на nav-иконке */
.ni-badge{
  position:absolute;top:4px;right:3px;
  min-width:14px;height:14px;padding:0 3px;
  background:rgba(200,200,200,.3);border-radius:7px;
  font-size:8px;font-weight:700;color:#fff;
  display:flex;align-items:center;justify-content:center;
  animation:fadeIn .3s ease;
  pointer-events:none;
}
═══════════════════════════════════════════ */

/* ═══════════════════════════════════════════
   LINKS STRIP
═══════════════════════════════════════════ */
.links{
  padding:0 16px;
  display:flex;
  flex-direction:row;
  flex-wrap:nowrap;
  gap:8px;
  overflow-x:auto;
  overflow-y:hidden;
  scrollbar-width:none;
  margin-bottom:16px;
  -webkit-overflow-scrolling:touch;
}
.links::-webkit-scrollbar{display:none}
.lpill{
  flex:0 0 auto;
  display:flex;align-items:center;gap:6px;
  padding:8px 14px;
  background:var(--card);border:1px solid var(--border);
  border-radius:20px;text-decoration:none;
  white-space:nowrap;
  color:var(--txt-muted);font-size:11px;letter-spacing:.08em;font-weight:500;
  transition:border-color .2s ease,color .2s ease,opacity .2s ease;
}
.lpill:active{opacity:.6}

/* ═══════════════════════════════════════════
   PRICE ROW
═══════════════════════════════════════════ */
.price-row{display:flex;gap:8px;margin:5px 0 11px}
.p-opt{flex:1;padding:13px 5px;background:var(--ac-dim);border:1px solid var(--border);border-radius:11px;text-align:center;transition:border-color var(--fast) ease,background var(--fast) ease}
.p-opt:hover{border-color:var(--border-ac);background:var(--ac-dim)}
.p-opt-ico{display:flex;justify-content:center;align-items:center;margin-bottom:7px;color:var(--txt-dim);height:24px}
.big-g{font-family:'Cormorant Garamond',serif;font-size:32px;font-weight:300;color:var(--txt-dim);line-height:1;letter-spacing:.04em}
.p-val{font-family:'Cormorant Garamond',serif;font-size:32px;font-weight:300;color:var(--txt);line-height:1;letter-spacing:.04em}
.p-unit{font-size:9px;letter-spacing:.12em;color:var(--g5);text-transform:uppercase;margin-top:3px}

/* ═══════════════════════════════════════════
   TAGS
═══════════════════════════════════════════ */
.tags{display:flex;flex-wrap:wrap;gap:6px;margin-bottom:13px}
.tag{display:inline-flex;align-items:center;gap:5px;padding:4px 9px;border-radius:16px;font-size:10px;letter-spacing:.08em;text-transform:uppercase;border:1px solid var(--border);background:var(--ac-dim);color:var(--txt-muted)}
.tag svg{color:var(--ac-soft)}

/* ═══════════════════════════════════════════
   NOTE / KEY / INFO
═══════════════════════════════════════════ */
.note{padding:11px 13px;background:var(--ac-dim);border:1px solid var(--border);border-radius:10px;font-size:12px;color:var(--txt-dim);line-height:1.7;margin-bottom:12px}
.note b{color:var(--txt)}
.key-box{margin:11px 0;padding:17px;background:var(--ac-dim);border:1px solid var(--border);border-radius:12px;text-align:center}
.key-lbl{font-size:9px;letter-spacing:.3em;color:var(--g5);text-transform:uppercase;margin-bottom:9px}
.key-val{font-family:'Cormorant Garamond',serif;font-size:19px;font-weight:300;color:var(--txt);letter-spacing:.1em;text-shadow:0 0 16px var(--ac-soft);margin-bottom:9px;word-break:break-all}
.key-st{display:inline-flex;align-items:center;gap:6px;font-size:10px;letter-spacing:.15em;text-transform:uppercase}
.key-st::before{content:'';width:6px;height:6px;border-radius:50%;background:currentColor}
.key-st.act{color:rgba(200,200,200,.85)}.key-st.act::before{animation:dotPulse 2s ease-in-out infinite}
.key-st.off{color:var(--g5)}
.info-r{display:flex;justify-content:space-between;align-items:center;padding:9px 0;border-bottom:1px solid var(--border)}
.info-r:last-child{border-bottom:none}
.info-k{display:flex;align-items:center;gap:7px;font-size:11px;color:var(--g5);letter-spacing:.07em}
.info-k svg{color:var(--txt-muted)}
.info-v{font-size:12px;color:var(--txt);font-weight:500}

/* ═══════════════════════════════════════════
   FORM
═══════════════════════════════════════════ */
.form{display:flex;flex-direction:column;gap:10px}
.form-g{display:flex;flex-direction:column;gap:4px}
.form-l{font-size:10px;letter-spacing:.2em;text-transform:uppercase;color:var(--g5)}
.form-i,.form-ta{
  background:var(--ac-dim);border:1px solid var(--border);
  border-radius:10px;padding:10px 12px;
  font-family:var(--fb);font-size:13px;color:var(--txt);
  transition:border-color var(--fast) ease,background var(--fast) ease;
  outline:none;resize:none;appearance:none;
}
.form-i:focus,.form-ta:focus{border-color:rgba(255,255,255,.22);background:rgba(255,255,255,.06)}
.form-i::placeholder,.form-ta::placeholder{color:var(--g5)}

/* ═══════════════════════════════════════════
   BUTTONS
═══════════════════════════════════════════ */
.btn{
  -webkit-tap-highlight-color:transparent;
  display:flex;align-items:center;justify-content:center;gap:8px;
  width:100%;padding:13px 22px;border-radius:12px;
  font-family:var(--fb);font-size:12px;font-weight:600;letter-spacing:.18em;text-transform:uppercase;
  cursor:pointer;border:none;text-decoration:none;
  transition:transform .25s cubic-bezier(.16,1,.3,1),
             background .25s ease,
             box-shadow .25s ease,
             opacity .25s ease;
  backface-visibility:hidden;
  -webkit-backface-visibility:hidden;
}
.btn:hover{ transform:translateY(-2px); box-shadow:0 6px 20px rgba(0,0,0,.35); }
.btn:active{ transform:scale(.93)!important; transition:transform .08s ease!important; box-shadow:none; }
.btn-p{
  background:rgba(228,228,228,.92);
  color:rgba(0,0,0,.72);
  font-weight:600;letter-spacing:.2em;
  position:relative;overflow:hidden;
}
.btn-p::after{
  content:'';position:absolute;inset:0;
  background:rgba(255,255,255,0);
  border-radius:inherit;
  transition:background .3s ease;
  pointer-events:none;
}
.btn-p:hover{ background:rgba(255,255,255,.98); transform:translateY(-2px); box-shadow:0 8px 24px rgba(0,0,0,.4); }
.btn-p:active{ background:rgba(255,255,255,.97);transform:scale(.93)!important; box-shadow:none; }
.btn-p:active::after{ background:rgba(0,0,0,.06); }
.btn-s{background:rgba(255,255,255,.04);color:var(--txt);border:1px solid var(--border)}
.btn-s:hover{ background:rgba(255,255,255,.08); border-color:rgba(255,255,255,.2); }
.btn-s:active{ background:rgba(255,255,255,.08); }

/* AI инпут — анимация появления */
@keyframes inputSlideIn{
  from{ opacity:0; transform:translateY(12px); }
  to  { opacity:1; transform:translateY(0) scale(1); }
}
#ai-bar[style*="block"]{
  animation:inputSlideIn .45s cubic-bezier(.16,1,.3,1) both;
}
#ai-input{
  transition:border-color .25s ease, height .2s cubic-bezier(.16,1,.3,1)!important;
}
#ai-input:focus{
  box-shadow:0 0 0 1px rgba(255,255,255,.12);
}
.mt{margin-top:11px}

/* ═══════════════════════════════════════════
   DOC LINKS / CHANGELOG
═══════════════════════════════════════════ */
.dlink{display:flex;align-items:center;gap:10px;padding:10px 0;border-bottom:1px solid var(--border);text-decoration:none;color:var(--txt-muted);font-size:13px;transition:color .2s ease,opacity .2s ease}
.dlink:last-child{border-bottom:none}.dlink:active{opacity:.6}
.dlink svg:first-child{flex-shrink:0;color:var(--txt-muted)}
.cl-e{padding:10px 0;border-bottom:1px solid var(--border)}.cl-e:last-child{border-bottom:none}
.cl-v{display:inline-block;font-family:monospace;font-size:11px;font-weight:700;letter-spacing:.08em;color:var(--txt);background:var(--ac-dim);border:1px solid var(--border-ac);border-radius:6px;padding:2px 8px;margin-bottom:4px}
.cl-d{font-size:10px;color:var(--g5);letter-spacing:.1em;margin-bottom:4px}
.cl-t{font-size:13px;color:var(--txt-dim);line-height:1.65}

/* ═══════════════════════════════════════════
   BOTTOM NAV
═══════════════════════════════════════════ */
.nav{
  position:fixed;bottom:0;left:0;right:0;
  width:100%;max-width:480px;margin:0 auto;
  padding:5px 4px calc(5px + env(safe-area-inset-bottom));
  background:#0b0b0b;
  border-top:1px solid var(--border);
  display:flex;justify-content:space-around;
  z-index:200;
  overflow:clip;
  isolation:isolate;
  -webkit-transform:translateZ(0);transform:translateZ(0);
}

/* Скользящая подложка */
#nav-slider{
  position:absolute;
  left:0;top:4px;
  height:46px;width:44px;
  border-radius:10px;
  background:rgba(255,255,255,.1);
  border:1px solid rgba(255,255,255,.14);
  transform:translateX(0);
  transition:transform .65s cubic-bezier(.25,.46,.45,.94),
             width     .65s cubic-bezier(.25,.46,.45,.94);
  will-change:transform,width;
  pointer-events:none;z-index:0;
  box-shadow:0 1px 0 rgba(255,255,255,.1) inset;
}

.ni{
  -webkit-tap-highlight-color:transparent;
  display:flex;flex-direction:column;align-items:center;gap:3px;
  cursor:pointer;padding:6px 4px 4px;border-radius:11px;
  min-width:44px;position:relative;
  transition:none;
  z-index:1;
}
.ni svg{
  color:var(--txt-muted);
  display:block;
  /* только color transition — никаких filter/transform анимаций */
  transition:color .15s ease;
  /* will-change убран — 8 одновременных GPU слоёв убивали fps */
}

.ni span{
  font-size:8px;letter-spacing:.06em;text-transform:uppercase;
  color:var(--txt-muted);white-space:nowrap;
  transition:color .15s ease;
}
.ni:hover svg { color:var(--ac-soft); }
.ni:hover span{ color:var(--ac-soft); }
.ni:active{ opacity:.7; transition:opacity .1s ease; }
.ni.on:active{ opacity:1; } /* уже на этом разделе — никакого эффекта */
/* nav bounce убран — иконка остаётся статичной */

/* Активная иконка */
.ni.on span{ color:var(--ac); }
.ni.on svg { color:var(--ac); }

/* Свечение через ::after — дешевле чем filter:drop-shadow на SVG.
   box-shadow на div не вызывает rasterization SVG-слоя */
.ni::after{
  content:'';
  position:absolute;
  top:6px;left:50%;
  width:20px;height:20px;
  margin-left:-10px;
  border-radius:50%;
  background:var(--ac);
  opacity:0;
  filter:blur(8px);
  transition:opacity .32s ease;
  pointer-events:none;
  z-index:-1;
}
.ni.on::after{ opacity:.35; }

/* Линия-индикатор снизу */
.ni::before{
  content:'';position:absolute;
  bottom:3px;left:50%;
  transform:translateX(-50%) scaleX(0);
  width:16px;height:1.5px;border-radius:1px;
  background:linear-gradient(90deg,transparent,var(--ac),transparent);
  transition:transform .4s cubic-bezier(.34,1.56,.64,1),
             opacity .28s ease;
  opacity:0;
}
.ni.on::before{ transform:translateX(-50%) scaleX(1); opacity:.75; }

/* ═══════════════════════════════════════════
   STATUS — индикаторы состояния серверов
═══════════════════════════════════════════ */
.status-row{
  display:flex;align-items:center;justify-content:space-between;
  padding:11px 0;border-bottom:1px solid var(--border);
}
.status-row:last-child{border-bottom:none}
.status-label{display:flex;align-items:center;gap:8px;font-size:13px;color:var(--txt-dim)}
.status-label svg{color:var(--txt-muted)}
.status-badge{
  display:inline-flex;align-items:center;gap:5px;
  padding:3px 10px;border-radius:20px;font-size:10px;
  font-weight:600;letter-spacing:.08em;
  background:rgba(255,255,255,.06);border:1px solid var(--border);
  color:rgba(200,200,200,.7);
  transition:all .32s var(--smooth);
}
.status-badge .dot{
  width:6px;height:6px;border-radius:50%;
  background:rgba(200,200,200,.5);
  transition:background .32s ease;
}
.status-badge.online{ background:rgba(255,255,255,.07); border-color:rgba(255,255,255,.14); color:rgba(220,220,220,.9); }
.status-badge.online .dot{ background:rgba(220,220,220,.8); animation:dotPulse 2s ease-in-out infinite; }
.status-badge.offline{ background:rgba(255,255,255,.04); color:rgba(150,150,150,.6); }
.status-badge.offline .dot{ background:rgba(150,150,150,.4); }
.status-badge.loading .dot{ opacity:.4; }

/* ═══════════════════════════════════════════
   REVIEWS — карточки отзывов
═══════════════════════════════════════════ */
.rev-card{
  padding:14px 0;border-bottom:1px solid var(--border);
}
.rev-card:last-child{border-bottom:none}
.rev-top{display:flex;align-items:center;justify-content:space-between;margin-bottom:6px}
.rev-name{font-size:12px;font-weight:600;color:var(--txt);letter-spacing:.04em}
.rev-date{font-size:10px;color:var(--g5);letter-spacing:.06em}
.rev-stars{display:flex;gap:2px;margin-bottom:6px}
.rev-stars svg{color:rgba(200,200,200,.5)}
.rev-text{font-size:13px;color:var(--txt-dim);line-height:1.7}
.rev-empty{text-align:center;padding:28px 0;color:var(--g5);font-size:12px;letter-spacing:.1em}

/* ═══════════════════════════════════════════
   PROMO — поле промокода
═══════════════════════════════════════════ */
.promo-wrap{display:flex;gap:8px;margin-top:4px}
.promo-wrap .form-i{flex:1}
.promo-btn{
  padding:10px 16px;border-radius:10px;border:1px solid var(--border);
  background:rgba(255,255,255,.06);color:var(--txt);font-family:var(--fb);
  font-size:12px;font-weight:600;letter-spacing:.1em;cursor:pointer;
  transition:background .32s var(--smooth), border-color .32s var(--smooth);
  white-space:nowrap;
}
.promo-btn:active{background:rgba(255,255,255,.12);transition-duration:.1s}
.promo-result{
  margin-top:10px;padding:10px 13px;border-radius:10px;
  font-size:12px;letter-spacing:.06em;
  background:rgba(255,255,255,.05);border:1px solid var(--border);
  color:var(--txt-dim);text-align:center;
  display:none;
}
.promo-result.show{display:block}
.promo-result.ok{ border-color:rgba(200,200,200,.25); color:rgba(220,220,220,.9); }
.promo-result.err{ border-color:rgba(200,200,200,.1); color:rgba(150,150,150,.7); }
═══════════════════════════════════════════ */




/* PROFILE */



#toast{
  position:fixed;bottom:80px;left:50%;
  transform:translateX(-50%) translateY(8px);
  background:rgba(22,22,22,.97);
  border:1px solid rgba(255,255,255,.1);
  border-radius:20px;padding:8px 17px;
  font-size:12px;letter-spacing:.08em;color:var(--txt);
  z-index:999;opacity:0;pointer-events:none;
  transition:opacity .35s var(--ease),transform .35s var(--ease);
  will-change:transform,opacity;
  white-space:nowrap;
}
#toast.show{opacity:1;transform:translateX(-50%) translateY(0)}

/* ═══════════════════════════════════════════
   MOBILE PERFORMANCE
═══════════════════════════════════════════ */

/* Убираем тяжёлые backdrop-filter на слабых телефонах */
@media (max-width:480px){
  #toast{backdrop-filter:none;-webkit-backdrop-filter:none;background:rgba(30,30,30,.96)}
  /* меньше лепестков сакуры — JS читает этот флаг */
  body{ --mobile:1; }
  /* backdrop-filter blur — самый тяжёлый эффект, убираем на мобиле */
  #prf-overlay,#picker-ov{
    backdrop-filter:none!important;
    -webkit-backdrop-filter:none!important;
    background:rgba(0,0,0,.72)!important;
  }
  #prf-sheet,#picker-sheet{
    backdrop-filter:none!important;
    -webkit-backdrop-filter:none!important;
  }
  /* Убираем тяжёлые box-shadow на слабых устройствах */
  .panel{ box-shadow:none!important; }
}

/* Respect reduced motion — никаких анимаций для людей с эпилепсией */
@media (prefers-reduced-motion:reduce){
  *,*::before,*::after{ animation-duration:.01ms!important; transition-duration:.01ms!important; }
}

/* iOS Safari — фикс 100vh */
@supports (-webkit-touch-callout:none){
  #loader,#app{ min-height:-webkit-fill-available; }
}


/* ══ УНИВЕРСАЛЬНАЯ TAP АНИМАЦИЯ ══ */
*{-webkit-tap-highlight-color:transparent}

/* Всё кликабельное — мягкая пружина */
button, [onclick], .svc-card, .btn, .lpill, .ni,
.dlink, .lang-b, .key-copy, .info-r a, .panel,
.hdr-avatar-wrap, .logo-t{
  transition:transform .38s cubic-bezier(.25,.46,.45,.94),
             opacity   .38s cubic-bezier(.25,.46,.45,.94),
             background .32s cubic-bezier(.25,.46,.45,.94),
             border-color .32s cubic-bezier(.25,.46,.45,.94),
             box-shadow   .32s cubic-bezier(.25,.46,.45,.94);
  cursor:pointer;
}

/* Нажатие — мягкий scale */
button:active, [onclick]:active, .svc-card:active,
.btn:active, .lpill:active, .dlink:active,
.lang-b:active, .hdr-avatar-wrap:active{
  transform:scale(.96)!important;
  opacity:.75;
  transition:transform .12s cubic-bezier(.25,.46,.45,.94),
             opacity   .12s ease;
}

/* Нав-айтемы */
.ni:active{ transform:scale(.90)!important;
  transition:transform .12s cubic-bezier(.25,.46,.45,.94); }

/* Ссылки внутри панелей */
a:active{ opacity:.55; transition:opacity .18s ease; }

/* Карточки сервисов */
@media(hover:hover){
  .svc-card:hover{
    transform:translateY(-2px) scale(1.01);
    box-shadow:0 8px 24px rgba(0,0,0,.5);
  }
}
.svc-card:active{
  transform:scale(.95)!important;
  box-shadow:none;
  transition:transform .12s cubic-bezier(.25,.46,.45,.94)!important;
}

/* Кнопка ИИ */
@media(hover:hover){
  #ai-btn:hover{ background:rgba(255,255,255,.2)!important; transform:scale(1.06); }
}
#ai-btn:active{ transform:scale(.88)!important;
  transition:transform .12s cubic-bezier(.25,.46,.45,.94)!important; }

/* Lang switcher */
@media(hover:hover){ .lang-b:hover{ opacity:.8; } }

/* AI bar — анимация появления снизу */
@keyframes aiBarIn{
  from{ opacity:0; transform:translateZ(0) translateY(20px); }
  to  { opacity:1; transform:translateZ(0) translateY(0); }
}
@keyframes aiBarOut{
  from{ opacity:1; transform:translateZ(0) translateY(0); }
  to  { opacity:0; transform:translateZ(0) translateY(20px); }
}

/* GPU — только нужные слои */
#ai-bar{ backface-visibility:hidden; -webkit-backface-visibility:hidden; }

/* Числа — tabular монопространственный */
.num, #fps-num, .ld-pct, .p-val, .cl-v{
  font-variant-numeric: tabular-nums;
  font-feature-settings: "tnum";
  font-family: var(--fb) !important;
  letter-spacing: .02em;
}

#picker-list::-webkit-scrollbar{display:none}

/* Пункты пикера */
.picker-item{
  padding:13px 16px;
  font-size:14px;
  font-family:var(--fb);
  color:rgba(255,255,255,.7);
  cursor:pointer;
  border-bottom:1px solid rgba(255,255,255,.05);
  position:relative;
  overflow:hidden;
  transition:color .18s ease, text-shadow .18s ease, opacity .12s ease;
  -webkit-tap-highlight-color:transparent;
  touch-action:manipulation;
  user-select:none;
  -webkit-user-select:none;
}
.picker-item::before{
  content:'';
  position:absolute;top:0;left:0;right:0;height:1px;
  background:linear-gradient(90deg,transparent,rgba(255,255,255,.2),transparent);
  opacity:0;transition:opacity .18s ease;pointer-events:none;
}
.picker-item::after{
  content:'';
  position:absolute;inset:0;
  background:radial-gradient(circle at 50% 50%,rgba(255,255,255,.09) 0%,transparent 70%);
  opacity:0;transition:opacity .18s ease;pointer-events:none;
}

@keyframes pickerTap{
  0%  { color:rgba(255,255,255,.7); text-shadow:none; }
  30% { color:#fff; text-shadow:0 0 32px rgba(255,255,255,.9),0 0 12px rgba(255,255,255,.5); }
  100%{ color:rgba(255,255,255,.85); text-shadow:0 0 8px rgba(255,255,255,.2); }
}
@keyframes pickerSheetIn{
  from{ transform:translateY(60px); opacity:0; }
  to  { transform:translateY(0);    opacity:1; }
}
/* hover только на устройствах с настоящим курсором */
@media(hover:hover){
  .picker-item:hover{
    color:#fff;
    text-shadow:0 0 18px rgba(255,255,255,.55),0 0 6px rgba(255,255,255,.25);
  }
  .picker-item:hover::before,
  .picker-item:hover::after{ opacity:1; }
}

/* Нажатие — плавная вспышка */
.picker-item.tapped{
  animation: pickerTap .55s cubic-bezier(.16,1,.3,1) forwards;
}
.picker-item.tapped::before,
.picker-item.tapped::after{ opacity:1; transition:opacity .55s ease; }


/* ═══════════════════════════════════════════
   DOWNLOAD LINK
═══════════════════════════════════════════ */
.dlink{
  display:flex;align-items:center;gap:8px;
  padding:11px 0;border-bottom:1px solid var(--border);
  text-decoration:none;color:var(--txt-dim);font-size:13px;
}
