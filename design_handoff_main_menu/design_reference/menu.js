/* ===========================================================
   ASTRAL SWARM — menu interactions (vanilla, no deps)
   Wires every .stage on the page: screen flow, sliders,
   toggles, exit modal, play transition, ambient embers.
   =========================================================== */
(function () {
  'use strict';

  function initStage(stage) {
    var variant = stage.getAttribute('data-variant') || 'forged';
    var menu = stage.querySelector('.screen--menu');
    var settings = stage.querySelector('.screen--settings');
    var modal = stage.querySelector('.modal-wrap');
    var playOverlay = stage.querySelector('.play-overlay');

    /* ---------- ambient embers (drifting motes) ---------- */
    var emberHost = stage.querySelector('.embers');
    if (emberHost && !emberHost.dataset.built) {
      emberHost.dataset.built = '1';
      var N = 26;
      for (var i = 0; i < N; i++) {
        var e = document.createElement('span');
        e.className = 'ember';
        e.style.left = (Math.random() * 100) + '%';
        e.style.setProperty('--dur', (7 + Math.random() * 7).toFixed(2) + 's');
        e.style.setProperty('--delay', (-Math.random() * 12).toFixed(2) + 's');
        e.style.setProperty('--drift', (Math.random() * 120 - 60).toFixed(0) + 'px');
        var s = 2 + Math.round(Math.random() * 3);
        e.style.width = s + 'px';
        e.style.height = s + 'px';
        emberHost.appendChild(e);
      }
    }

    /* ---------- screen navigation ---------- */
    function openSettings() {
      if (!settings) return;
      settings.hidden = false;
    }
    function closeSettings() {
      if (!settings) return;
      settings.hidden = true;
    }
    function showModal() { if (modal) modal.hidden = false; }
    function hideModal() { if (modal) modal.hidden = true; }

    function doPlay() {
      if (!playOverlay) return;
      playOverlay.classList.add('show');
      setTimeout(function () { playOverlay.classList.remove('show'); }, 2200);
    }

    /* ---------- button actions ---------- */
    stage.addEventListener('click', function (ev) {
      var t = ev.target.closest('[data-action]');
      if (!t || !stage.contains(t)) return;
      var action = t.getAttribute('data-action');
      switch (action) {
        case 'play':     doPlay(); break;
        case 'settings': openSettings(); break;
        case 'back':     closeSettings(); break;
        case 'exit':     showModal(); break;
        case 'exit-yes': hideModal(); doExit(); break;
        case 'exit-no':  hideModal(); break;
      }
    });

    function doExit() {
      // mock: dim the world briefly to acknowledge the action
      stage.style.transition = 'filter .5s steps(6), opacity .5s';
      stage.style.filter = 'brightness(.15) saturate(.4)';
      setTimeout(function () {
        stage.style.filter = '';
      }, 1400);
    }

    /* close settings / modal with Escape */
    stage.setAttribute('tabindex', '-1');
    document.addEventListener('keydown', function (ev) {
      if (ev.key !== 'Escape') return;
      if (modal && !modal.hidden) { hideModal(); return; }
      if (settings && !settings.hidden) { closeSettings(); }
    });
    if (modal) {
      modal.addEventListener('click', function (ev) {
        if (ev.target === modal) hideModal();
      });
    }

    /* ---------- segmented (display mode) toggles ---------- */
    stage.querySelectorAll('.seg').forEach(function (seg) {
      seg.addEventListener('click', function (ev) {
        var opt = ev.target.closest('.seg-opt');
        if (!opt) return;
        seg.querySelectorAll('.seg-opt').forEach(function (o) { o.classList.remove('is-active'); });
        opt.classList.add('is-active');
        try {
          localStorage.setItem('astral.' + variant + '.displaymode', opt.textContent.trim());
        } catch (e) {}
      });
      // restore
      try {
        var saved = localStorage.getItem('astral.' + variant + '.displaymode');
        if (saved) {
          seg.querySelectorAll('.seg-opt').forEach(function (o) {
            o.classList.toggle('is-active', o.textContent.trim() === saved);
          });
        }
      } catch (e) {}
    });

    /* ---------- pixel sliders ---------- */
    stage.querySelectorAll('.pslider').forEach(function (sl) {
      var fill = sl.querySelector('.pslider__fill');
      var handle = sl.querySelector('.pslider__handle');
      var track = sl.querySelector('.pslider__track');
      var row = sl.closest('.slider-row');
      var valEl = row ? row.querySelector('.val') : null;
      var key = 'astral.' + variant + '.' + (sl.getAttribute('data-key') || 'slider');

      function render(v) {
        v = Math.max(0, Math.min(100, Math.round(v)));
        sl.setAttribute('data-value', v);
        sl.setAttribute('aria-valuenow', v);
        fill.style.width = v + '%';
        handle.style.left = v + '%';
        if (valEl) valEl.textContent = v + '%';
        try { localStorage.setItem(key, v); } catch (e) {}
      }
      function fromEvent(clientX) {
        var r = (track || sl).getBoundingClientRect();
        return ((clientX - r.left) / r.width) * 100;
      }

      var dragging = false;
      sl.addEventListener('pointerdown', function (ev) {
        dragging = true;
        sl.setPointerCapture(ev.pointerId);
        render(fromEvent(ev.clientX));
        ev.preventDefault();
      });
      sl.addEventListener('pointermove', function (ev) {
        if (dragging) render(fromEvent(ev.clientX));
      });
      sl.addEventListener('pointerup', function () { dragging = false; });
      sl.addEventListener('pointercancel', function () { dragging = false; });
      sl.setAttribute('role', 'slider');
      sl.setAttribute('tabindex', '0');
      sl.setAttribute('aria-valuemin', '0');
      sl.setAttribute('aria-valuemax', '100');
      sl.addEventListener('keydown', function (ev) {
        var v = parseInt(sl.getAttribute('data-value'), 10) || 0;
        if (ev.key === 'ArrowRight' || ev.key === 'ArrowUp') { render(v + 5); ev.preventDefault(); }
        else if (ev.key === 'ArrowLeft' || ev.key === 'ArrowDown') { render(v - 5); ev.preventDefault(); }
        else if (ev.key === 'Home') { render(0); }
        else if (ev.key === 'End') { render(100); }
      });

      // initial value: localStorage > data-value attr > 70
      var init = 70;
      try {
        var s = localStorage.getItem(key);
        if (s !== null) init = parseInt(s, 10);
        else if (sl.hasAttribute('data-value')) init = parseInt(sl.getAttribute('data-value'), 10);
      } catch (e) {
        if (sl.hasAttribute('data-value')) init = parseInt(sl.getAttribute('data-value'), 10);
      }
      render(init);
    });
  }

  function boot() {
    document.querySelectorAll('.stage').forEach(initStage);
    fitStages();
  }

  /* ---- auto-fit each top-level stage to its viewport (letterbox) ---- */
  function fitStages() {
    var stages = document.querySelectorAll('.stage');
    if (!stages.length) return;
    // only auto-fit when a stage is a direct child of <body> (standalone view).
    // inside the comparison canvas the iframes are sized exactly 1408x768 (scale 1).
    stages.forEach(function (stage) {
      if (stage.parentElement !== document.body) return;
      document.documentElement.style.height = '100%';
      document.body.style.height = '100%';
      document.body.style.display = 'flex';
      document.body.style.alignItems = 'center';
      document.body.style.justifyContent = 'center';
      document.body.style.overflow = 'hidden';
      stage.style.flex = '0 0 auto';
      function apply() {
        var s = Math.min(window.innerWidth / 1408, window.innerHeight / 768);
        stage.style.transformOrigin = 'center center';
        stage.style.transform = 'scale(' + s + ')';
        // collapse the layout box to the scaled size so flex centering is exact
        stage.style.margin = (-(768 * (1 - s)) / 2) + 'px ' + (-(1408 * (1 - s)) / 2) + 'px';
      }
      apply();
      window.addEventListener('resize', apply);
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', boot);
  } else {
    boot();
  }
})();
