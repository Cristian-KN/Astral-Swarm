/* ===========================================================
   ASTRAL SWARM — HUD runtime
   timer · xp/level · hearts · gold · loadout · minimap
   pause overlay · level-up overlay (3 cards) · reroll
   =========================================================== */
(function () {
  'use strict';
  var I = window.ICONS;

  /* ---------------- game state ---------------- */
  var S = {
    level: 7,
    xp: 0.42,            // 0..1 toward next level
    timeLeft: 178,       // seconds (02:58)
    hpMax: 6,            // half-hearts? we use whole hearts; hpMax = number of hearts
    hp: 4.5,             // can be .5 for half heart
    gold: 1240,
    kills: 312,
    paused: false,
    weapons: [
      { id: 'sword', name: 'Espada Rúnica', lvl: 4 },
      { id: 'orb',   name: 'Orbe Ardiente', lvl: 2 },
      { id: 'aura',  name: 'Aura Sagrada',  lvl: 1 }
    ],
    weaponMax: 3,
    passives: [
      { id: 'boot',   name: 'Botas Veloces' },
      { id: 'clover', name: 'Trébol' },
      { id: 'magnet', name: 'Imán Arcano' },
      { id: 'tome',   name: 'Tomo Prohibido' }
    ]
  };

  // restore persisted bits
  try {
    var saved = JSON.parse(localStorage.getItem('astral.hud') || 'null');
    if (saved) Object.assign(S, saved);
  } catch (e) {}
  function persist() {
    try {
      localStorage.setItem('astral.hud', JSON.stringify({
        level: S.level, xp: S.xp, timeLeft: S.timeLeft, hp: S.hp, hpMax: S.hpMax,
        gold: S.gold, kills: S.kills, weapons: S.weapons, passives: S.passives
      }));
    } catch (e) {}
  }

  var $ = function (s, r) { return (r || document).querySelector(s); };

  /* ---------------- render: XP + level ---------------- */
  function renderXP() {
    $('.xp-fill').style.width = (S.xp * 100).toFixed(1) + '%';
    $('.lvl-num').textContent = S.level;
  }

  /* ---------------- render: timer ---------------- */
  function fmt(t) {
    var m = Math.floor(t / 60), s = Math.floor(t % 60);
    return (m < 10 ? '0' : '') + m + ':' + (s < 10 ? '0' : '') + s;
  }
  function renderTimer() { $('.t-clock').textContent = fmt(S.timeLeft); }

  /* ---------------- render: hearts ---------------- */
  function renderHearts() {
    var host = $('.hearts');
    host.innerHTML = '';
    for (var i = 0; i < S.hpMax; i++) {
      var d = document.createElement('span');
      d.className = 'heart';
      var filled = S.hp - i;
      d.innerHTML = filled >= 1 ? I.heartFull() : (filled >= 0.5 ? I.heartHalf() : I.heartEmpty());
      host.appendChild(d);
    }
  }

  /* ---------------- render: gold ---------------- */
  function renderGold() {
    $('.gold-chip .coin').innerHTML = I.coin();
    $('.gold-chip .amt').textContent = S.gold.toLocaleString('es-ES');
  }

  /* ---------------- render: loadout ---------------- */
  function pips(lvl) {
    var out = '<span class="lvl-pips">';
    for (var i = 0; i < lvl; i++) out += '<i></i>';
    return out + '</span>';
  }
  function renderLoadout() {
    var wRow = $('.row-weapons');
    wRow.innerHTML = '';
    for (var i = 0; i < S.weaponMax; i++) {
      var w = S.weapons[i];
      var slot = document.createElement('div');
      if (w) {
        slot.className = 'slot bevel' + (w.lvl >= 8 ? ' maxed' : '');
        slot.innerHTML = '<span class="ico">' + (I[w.id] ? I[w.id]() : '') + '</span>' + pips(w.lvl);
        slot.title = w.name + ' · Nv ' + w.lvl;
      } else {
        slot.className = 'slot bevel is-empty';
      }
      wRow.appendChild(slot);
    }
    var pRow = $('.row-passives');
    pRow.innerHTML = '';
    S.passives.forEach(function (p) {
      var slot = document.createElement('div');
      slot.className = 'slot passive bevel';
      slot.innerHTML = '<span class="ico">' + (I[p.id] ? I[p.id]() : '') + '</span>';
      slot.title = p.name;
      pRow.appendChild(slot);
    });
  }

  /* ---------------- minimap blips ---------------- */
  function buildMinimap() {
    var inner = $('.map-inner');
    inner.querySelectorAll('.blip').forEach(function (b) { b.remove(); });
    // player center
    var p = document.createElement('span'); p.className = 'blip player'; p.style.left = '50%'; p.style.top = '50%';
    inner.appendChild(p);
    // enemies (the "swarm")
    for (var i = 0; i < 34; i++) {
      var e = document.createElement('span'); e.className = 'blip enemy';
      e.dataset.ang = (Math.random() * Math.PI * 2).toFixed(3);
      e.dataset.rad = (18 + Math.random() * 60).toFixed(1);
      e.dataset.spd = (0.4 + Math.random() * 0.9).toFixed(3);
      inner.appendChild(e);
    }
    for (var j = 0; j < 5; j++) {
      var l = document.createElement('span'); l.className = 'blip loot';
      l.style.left = (15 + Math.random() * 70) + '%';
      l.style.top = (15 + Math.random() * 70) + '%';
      inner.appendChild(l);
    }
  }
  function animateMinimap(t) {
    var enemies = document.querySelectorAll('.blip.enemy');
    enemies.forEach(function (e) {
      var ang = parseFloat(e.dataset.ang) + (parseFloat(e.dataset.spd) * t * 0.0003);
      var rad = parseFloat(e.dataset.rad);
      var x = 50 + Math.cos(ang) * rad * 0.5;
      var y = 50 + Math.sin(ang) * rad * 0.5;
      e.style.left = x + '%'; e.style.top = y + '%';
    });
  }

  /* ---------------- LEVEL-UP overlay ---------------- */
  var UPGRADE_POOL = [
    { id: 'sword',  name: 'Espada Rúnica',  kind: 'Arma',   rarity: 'Común',     desc: 'Aumenta el daño en +12% y el alcance del tajo.' },
    { id: 'orb',    name: 'Orbe Ardiente',  kind: 'Arma',   rarity: 'Raro',      desc: 'Lanza un orbe adicional que persigue enemigos.' },
    { id: 'aura',   name: 'Aura Sagrada',   kind: 'Arma',   rarity: 'Épico',     desc: 'El aura crece un 20% y quema con luz divina.' },
    { id: 'bolt',   name: 'Centella',       kind: 'Arma',   rarity: 'Raro',      desc: 'Un rayo golpea al enemigo más cercano cada 3s.' },
    { id: 'axe',    name: 'Hacha Giratoria',kind: 'Arma',   rarity: 'Común',     desc: 'Arroja un hacha que rebota entre enemigos.' },
    { id: 'boot',   name: 'Botas Veloces',  kind: 'Pasivo', rarity: 'Común',     desc: '+10% de velocidad de movimiento.' },
    { id: 'magnet', name: 'Imán Arcano',    kind: 'Pasivo', rarity: 'Común',     desc: '+30% de rango de recogida de orbes.' },
    { id: 'tome',   name: 'Tomo Prohibido', kind: 'Pasivo', rarity: 'Épico',     desc: '-8% de enfriamiento en todas las armas.' },
    { id: 'wing',   name: 'Alas de Cuervo', kind: 'Pasivo', rarity: 'Raro',      desc: 'Concede un destello de evasión cada 12s.' },
    { id: 'heartUp',name: 'Corazón de Hierro',kind:'Pasivo',rarity: 'Raro',      desc: '+1 corazón de vida máxima.' },
    { id: 'skull',  name: 'Maldición',      kind: 'Pasivo', rarity: 'Épico',     desc: '+15% daño, pero recibes +5% de daño.' },
    { id: 'potion', name: 'Vial de Vida',   kind: 'Pasivo', rarity: 'Común',     desc: 'Regenera 0.5 corazones cada 10s.' },
    { id: 'clover', name: 'Trébol',         kind: 'Pasivo', rarity: 'Raro',      desc: '+10% de suerte: mejores cofres y rarezas.' },
    { id: 'ring',   name: 'Anillo de Égida',kind: 'Pasivo', rarity: 'Épico',     desc: '+1 de armadura: reduce el daño recibido.' }
  ];
  var rerolls = 2;

  function pick3() {
    var pool = UPGRADE_POOL.slice();
    var out = [];
    while (out.length < 3 && pool.length) {
      out.push(pool.splice(Math.floor(Math.random() * pool.length), 1)[0]);
    }
    return out;
  }
  function renderCards() {
    var host = $('.cards');
    host.innerHTML = '';
    pick3().forEach(function (u) {
      var c = document.createElement('div');
      c.className = 'card';
      c.dataset.id = u.id;
      c.dataset.kind = u.kind;
      c.innerHTML =
        '<div class="card-rarity">' + u.rarity + '</div>' +
        '<div class="card-icon"><span class="ico">' + (I[u.id] ? I[u.id]() : '') + '</span></div>' +
        '<h3>' + u.name + '</h3>' +
        '<div class="tier">' + u.kind + '</div>' +
        '<p>' + u.desc + '</p>' +
        '<div class="pick-hint">▸ Elegir ◂</div>';
      host.appendChild(c);
    });
    $('.reroll-badge').textContent = rerolls;
  }
  function openLevelUp() {
    S.paused = true;
    renderCards();
    $('.overlay--levelup').hidden = false;
  }
  function closeLevelUp() {
    $('.overlay--levelup').hidden = true;
    S.paused = false;
  }
  function applyUpgrade(id, kind) {
    var w = S.weapons.find(function (x) { return x.id === id; });
    if (w) { w.lvl = Math.min(8, w.lvl + 1); }
    else if (kind === 'Arma' && S.weapons.length < S.weaponMax) {
      var def = UPGRADE_POOL.find(function (x) { return x.id === id; });
      S.weapons.push({ id: id, name: def.name, lvl: 1 });
    } else {
      var pdef = UPGRADE_POOL.find(function (x) { return x.id === id; });
      if (id === 'heartUp') { S.hpMax += 1; S.hp = Math.min(S.hpMax, S.hp + 1); }
      if (!S.passives.find(function (x) { return x.id === id; }) && kind === 'Pasivo' && id !== 'heartUp') {
        S.passives.push({ id: id, name: pdef.name });
      }
    }
    renderLoadout(); renderHearts(); persist();
  }

  /* ---------------- PAUSE overlay ---------------- */
  function setPaused(v) {
    S.paused = v;
    $('.pause-overlay').hidden = !v;
    var pb = $('.pause-btn .ico');
    pb.innerHTML = v ? I.play() : I.pause();
    if (v) {
      $('.ps-time').textContent = fmt(S.timeLeft);
      $('.ps-level').textContent = S.level;
      $('.ps-kills').textContent = S.kills.toLocaleString('es-ES');
      $('.ps-gold').textContent = S.gold.toLocaleString('es-ES');
    }
  }

  /* ---------------- main loop ---------------- */
  var last = performance.now();
  var goldTick = 0, killTick = 0;
  function loop(now) {
    var dt = Math.min(100, now - last); last = now;
    var lvlOpen = !$('.overlay--levelup').hidden;
    var pzOpen = !$('.pause-overlay').hidden;

    if (!lvlOpen && !pzOpen) {
      // timer counts down
      S.timeLeft -= dt / 1000;
      if (S.timeLeft <= 0) S.timeLeft = 0;
      renderTimer();

      // xp trickles up; level up at 1.0
      S.xp += dt / 1000 * 0.05;
      if (S.xp >= 1) { S.xp -= 1; S.level += 1; renderXP(); openLevelUp(); }
      else renderXP();

      // gold + kills tick
      goldTick += dt; killTick += dt;
      if (goldTick > 700) { goldTick = 0; S.gold += Math.floor(Math.random() * 6); renderGold(); }
      if (killTick > 900) { killTick = 0; S.kills += Math.floor(1 + Math.random() * 3); }

      animateMinimap(now);
    }
    requestAnimationFrame(loop);
  }

  /* ---------------- wire up ---------------- */
  function boot() {
    // pause button + icon
    $('.pause-btn .ico').innerHTML = I.pause();
    $('.pause-btn').addEventListener('click', function () { setPaused(!S.paused); });

    // level-up clicks
    $('.overlay--levelup').addEventListener('click', function (ev) {
      var card = ev.target.closest('.card');
      if (card) { applyUpgrade(card.dataset.id, card.dataset.kind); closeLevelUp(); return; }
      var rr = ev.target.closest('.btn-reroll');
      if (rr && rerolls > 0) { rerolls--; renderCards(); }
      var skip = ev.target.closest('.btn-skip');
      if (skip) { S.gold += 50; renderGold(); closeLevelUp(); }
    });

    // pause menu
    $('.pause-overlay').addEventListener('click', function (ev) {
      var a = ev.target.closest('[data-pact]');
      if (!a) { if (ev.target === $('.pause-overlay')) setPaused(false); return; }
      var act = a.getAttribute('data-pact');
      if (act === 'resume') setPaused(false);
      else if (act === 'settings') { /* hook to settings screen */ a.textContent = 'Ajustes ✓'; setTimeout(function(){ a.textContent='Ajustes'; }, 700); }
      else if (act === 'exit') {
        document.querySelector('.stage').style.transition = 'filter .5s steps(6)';
        document.querySelector('.stage').style.filter = 'brightness(.1) saturate(.3)';
        setTimeout(function(){ document.querySelector('.stage').style.filter=''; setPaused(false); }, 1200);
      }
    });

    // keyboard: Esc/P pause, Space dev-trigger level up
    document.addEventListener('keydown', function (ev) {
      if (ev.key === 'Escape' || ev.key === 'p' || ev.key === 'P') {
        if (!$('.overlay--levelup').hidden) return;
        setPaused(!S.paused);
      }
      if (ev.key === 'l' || ev.key === 'L') { if ($('.overlay--levelup').hidden) openLevelUp(); }
    });

    renderXP(); renderTimer(); renderHearts(); renderGold(); renderLoadout(); buildMinimap();
    fitStage();
    requestAnimationFrame(loop);

    // dev/preview hook — lets you jump to any HUD state from the console
    window.ASTRAL_HUD = {
      levelUp: openLevelUp,
      closeLevelUp: closeLevelUp,
      pause: function () { setPaused(true); },
      resume: function () { setPaused(false); },
      state: S
    };

    // light damage flash demo every ~14s
    setInterval(function () {
      if (S.paused || !$('.overlay--levelup').hidden) return;
      var v = $('.dmg-vignette'); v.classList.add('show');
      S.hp = Math.max(0.5, S.hp - 0.5); renderHearts();
      setTimeout(function () { v.classList.remove('show'); }, 160);
      // heal back so it loops nicely
      setTimeout(function () { S.hp = Math.min(S.hpMax, S.hp + 0.5); renderHearts(); }, 4000);
    }, 14000);
  }

  /* ---------------- fit stage to viewport (letterbox) ---------------- */
  function fitStage() {
    var stage = document.querySelector('.stage');
    if (!stage || stage.parentElement !== document.body) return;
    document.body.style.cssText = 'margin:0;height:100%;display:flex;align-items:center;justify-content:center;overflow:hidden;background:#000;';
    stage.style.flex = '0 0 auto';
    function apply() {
      var s = Math.min(window.innerWidth / 1600, window.innerHeight / 900);
      stage.style.transformOrigin = 'center center';
      stage.style.transform = 'scale(' + s + ')';
      stage.style.margin = (-(900 * (1 - s)) / 2) + 'px ' + (-(1600 * (1 - s)) / 2) + 'px';
    }
    apply();
    window.addEventListener('resize', apply);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
