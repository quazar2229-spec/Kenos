'use strict';
/* ══════════════════════════════════════
   I18N — переводы интерфейса
   Использование: t('ключ') → строка
══════════════════════════════════════ */

const TR = {
  ru: {
    'hero.eye':'Emulator Solutions','hero.sub':'Premium · BlueStacks · Custom','hero.tip':'совершенству нет предела',
    'sec.svc':'Услуги','sec.lnk':'Ссылки',
    'svc.bs':'Custom BlueStacks','svc.bs.d':'Настройка под вас',
    'svc.key':'Ключ доступа','svc.key.d':'HWID лицензия',
    'svc.pay':'Оплата','svc.pay.d':'Реквизиты',
    'svc.hlp':'Поддержка','svc.hlp.d':'Связаться с нами',
    'lnk.ch':'Канал','lnk.ct':'Чат','lnk.ow':'Владелец','lnk.dev':'Кодеры',
    'buy.eye':'Лучший выбор','buy.get':'Что вы получаете','buy.get.d':'Лучший эмулятор — персонально для вас',
    'tag.hw':'HWID-защита','tag.fp':'Оптимизация FPS','tag.up':'Авто-обновление','tag.sp':'24/7 поддержка','tag.be':'Лучший на рынке',
    'pr.rub':'Рублей','pr.st':'Stars','pr.per':'/ месяц · выберите любой способ',
    'btn.pay':'Перейти к оплате','btn.ask':'Задать вопрос','btn.buy':'Купить','btn.st':'Купить через Stars',
    'btn.getbs':'Купить Custom BlueStacks','btn.ref':'Обновить данные','btn.send':'Отправить',
    'pay.eye':'Безопасно','pay.h':'Оплата',
    'pay.card':'Банковская карта','pay.card.d':'Перевод по СБП · 800 ₽',
    'pay.card.n':'Реквизиты и детали оплаты вы получите напрямую у кодера после нажатия кнопки ниже.',
    'pay.g':'G-голда','pay.g.n':'Оплата голдой <b>Standoff 2</b> — только через нашего кодера.',
    'pay.st.d':'Прямо внутри Telegram · 400 Stars','pay.st.n':'Оплата через <b>Telegram Stars</b> — только у нашего кодера.',
    'key.eye':'Лицензия','key.h':'Ключ','key.emu':'Ключ эмулятора','key.emu.d':'Ручная привязка HWID оператором',
    'key.lbl':'ВАШ КЛЮЧ','key.off':'Не активен','key.on':'Активен',
    'key.hw':'HWID','key.exp':'Истекает','key.none':'Ключ не куплен',
    'hlp.eye':'Всегда на связи','hlp.h':'Поддержка','hlp.op':'Связаться с оператором','hlp.op.d':'Ответим в течение 15 минут',
    'frm.subj':'Тема','frm.s0':'Выберите тему...','frm.s1':'Покупка / Оплата','frm.s2':'Проблема с ключом','frm.s3':'Привязка HWID','frm.s4':'Техническая проблема','frm.s5':'Другое',
    'frm.msg':'Сообщение','frm.ph':'Опишите проблему...',
    'hlp.ct':'Прямые контакты','hlp.ct.d':'Для срочных вопросов',
    'ct.own':'Владелец','ct.ch':'Канал','ct.chat':'Чат','ct.dev':'Кодер','ct.loh':'Кодер-лох',
    'nav.hm':'Главная','nav.by':'Купить','nav.py':'Оплата','nav.ky':'Ключ','nav.hp':'Помощь','nav.ab':'О нас',
    'inf.eye':'Статус · Новости','inf.h':'Инфо','inf.panel':'Информация','inf.panel.d':'Тех. работы · Статус сервиса · Новости','inf.empty':'Нет активных объявлений',
    'abt.eye':'О проекте','abt.h':'О нас','abt.p.d':'Standoff 2 · Premium Emulator',
    'abt.p.t':'KENOS — ваш идеал для игры в <b style="color:var(--txt)">Standoff 2</b>.<br>Лучшая производительность и сенса на рынке.',
    'abt.cl':'Обновления','abt.cl.d':'История версий бота',
    'abt.docs':'Документы','abt.docs.d':'Юридическая информация',
    'abt.pp':'Политика конфиденциальности','abt.tos':'Пользовательское соглашение',
    'abt.team':'Контакты','abt.team.d':'Наша команда',
    'toast.cp':'Скопировано','toast.sent':'Отправлено','toast.fill':'Заполните все поля','toast.ref':'Обновлено',
    'ai.eye':'Искусственный интеллект','ai.h':'Ассистент','ai.ph':'Сообщение...','ai.empty':'Спроси что угодно о KENOS',
  },
  en: {
    'hero.eye':'Emulator Solutions','hero.sub':'Premium · BlueStacks · Custom','hero.tip':'perfection has no limits',
    'sec.svc':'Services','sec.lnk':'Links',
    'svc.bs':'Custom BlueStacks','svc.bs.d':'Tailored for you',
    'svc.key':'Access Key','svc.key.d':'HWID License',
    'svc.pay':'Payment','svc.pay.d':'Requisites',
    'svc.hlp':'Support','svc.hlp.d':'Contact us',
    'lnk.ch':'Channel','lnk.ct':'Chat','lnk.ow':'Owner','lnk.dev':'Devs',
    'buy.eye':'Best Choice','buy.get':'What you get','buy.get.d':'Best emulator — personally for you',
    'tag.hw':'HWID Protection','tag.fp':'FPS Optimization','tag.up':'Auto-update','tag.sp':'24/7 Support','tag.be':'Best on market',
    'pr.rub':'Rubles','pr.st':'Stars','pr.per':'/ month · choose any method',
    'btn.pay':'Go to Payment','btn.ask':'Ask a Question','btn.buy':'Buy','btn.st':'Buy with Stars',
    'btn.getbs':'Buy Custom BlueStacks','btn.ref':'Refresh','btn.send':'Send',
    'pay.eye':'Secure','pay.h':'Payment',
    'pay.card':'Bank Card','pay.card.d':'SBP Transfer · 800 ₽',
    'pay.card.n':'Requisites provided directly by our developer.',
    'pay.g':'G-Gold','pay.g.n':'Payment in <b>Standoff 2 Gold</b> — only via our coder.',
    'pay.st.d':'Right inside Telegram · 400 Stars','pay.st.n':'Payment via <b>Telegram Stars</b> — only via our coder.',
    'key.eye':'License','key.h':'Key','key.emu':'Emulator Key','key.emu.d':'Manual HWID binding by operator',
    'key.lbl':'YOUR KEY','key.off':'Not Active','key.on':'Active',
    'key.hw':'HWID','key.exp':'Expires','key.none':'No key purchased',
    'hlp.eye':'Always in touch','hlp.h':'Support','hlp.op':'Contact Operator','hlp.op.d':'Response in 15 minutes',
    'frm.subj':'Subject','frm.s0':'Select topic...','frm.s1':'Purchase / Payment','frm.s2':'Key Issue','frm.s3':'HWID Binding','frm.s4':'Technical Issue','frm.s5':'Other',
    'frm.msg':'Message','frm.ph':'Describe your issue...',
    'hlp.ct':'Direct Contacts','hlp.ct.d':'For urgent matters',
    'ct.own':'Owner','ct.ch':'Channel','ct.chat':'Chat','ct.dev':'Developer','ct.loh':'Coder-loh',
    'nav.hm':'Home','nav.by':'Buy','nav.py':'Pay','nav.ky':'Key','nav.hp':'Help','nav.ab':'About',
    'inf.eye':'Status · News','inf.h':'Info','inf.panel':'Information','inf.panel.d':'Maintenance · Service Status · News','inf.empty':'No active announcements',
    'abt.eye':'About project','abt.h':'About us','abt.p.d':'Standoff 2 · Premium Emulator',
    'abt.p.t':'KENOS — your ideal for <b style="color:var(--txt)">Standoff 2</b>.<br>Best performance and sensitivity on market.',
    'abt.cl':'Updates','abt.cl.d':'Bot version history',
    'abt.docs':'Documents','abt.docs.d':'Legal information',
    'abt.pp':'Privacy Policy','abt.tos':'Terms of Service',
    'abt.team':'Contacts','abt.team.d':'Our team',
    'toast.cp':'Copied','toast.sent':'Sent','toast.fill':'Fill all fields','toast.ref':'Refreshed',
    'ai.eye':'Artificial Intelligence','ai.h':'Assistant','ai.ph':'Message...','ai.empty':'Ask anything about KENOS',
  }
};

let lang = 'ru';
const t = k => TR[lang][k] || TR.ru[k] || k;

function setLang(l) {
  lang = l;
  document.getElementById('lang-sw').dataset.lang = l;
  document.querySelectorAll('.lang-b').forEach(b => b.classList.toggle('on', b.textContent === l.toUpperCase()));
  document.querySelectorAll('[data-i18n]').forEach(el => el.innerHTML = t(el.dataset.i18n));
  document.querySelectorAll('option[data-i18n]').forEach(el => el.textContent = t(el.dataset.i18n));
  document.querySelectorAll('[data-i18n-ph]').forEach(el => el.placeholder = t(el.dataset.i18nPh));
  document.querySelectorAll('[data-tip-key]').forEach(el => el.dataset.tip = t(el.dataset.tipKey));
  const tip = document.getElementById('kenos-tip');
  if (tip) tip.textContent = t('hero.tip');
}
