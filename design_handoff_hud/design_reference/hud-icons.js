/* ===========================================================
   ASTRAL SWARM — HUD icons (pixel-art SVG sprites)
   Each returns an <svg> string, 16-unit grid, crisp edges.
   =========================================================== */
window.ICONS = (function () {
  function svg(inner, vb) {
    return '<svg viewBox="0 0 ' + (vb || 16) + ' ' + (vb || 16) + '" shape-rendering="crispEdges" xmlns="http://www.w3.org/2000/svg">' + inner + '</svg>';
  }
  function px(x, y, w, h, c) { return '<rect x="' + x + '" y="' + y + '" width="' + w + '" height="' + h + '" fill="' + c + '"/>'; }

  var I = {};

  /* ---------- HEARTS (Zelda-style) ---------- */
  var heartShape = function (fill, dark) {
    // 16x16 pixel heart
    return svg(
      px(3,2,4,2,dark)+px(9,2,4,2,dark)+
      px(2,4,12,2,dark)+
      px(2,6,12,3,fill)+px(2,4,12,2,fill).replace(fill,fill)+ // body
      px(3,4,4,2,fill)+px(9,4,4,2,fill)+
      px(3,9,10,2,fill)+px(4,11,8,2,fill)+px(5,13,6,1,fill)+px(7,14,2,1,fill)+
      // shine
      px(4,5,2,2,'rgba(255,255,255,.6)')
    );
  };
  I.heartFull  = function(){ return heartShape('#e0473e','#7a1f1c'); };
  I.heartHalf  = function(){
    return svg(
      px(3,2,4,2,'#7a1f1c')+px(9,2,4,2,'#3a3340')+
      px(2,4,6,2,'#7a1f1c')+px(8,4,6,2,'#3a3340')+
      px(2,6,6,3,'#e0473e')+px(8,6,6,3,'#2a2731')+
      px(3,9,5,2,'#e0473e')+px(8,9,5,2,'#2a2731')+
      px(4,11,4,2,'#e0473e')+px(8,11,4,2,'#2a2731')+
      px(5,13,3,1,'#e0473e')+px(8,13,3,1,'#2a2731')+px(7,14,2,1,'#e0473e')+
      px(4,5,2,2,'rgba(255,255,255,.6)')
    );
  };
  I.heartEmpty = function(){
    return svg(
      px(3,2,4,2,'#3a3340')+px(9,2,4,2,'#3a3340')+
      px(2,4,12,2,'#3a3340')+
      px(2,6,12,3,'#201d27')+px(3,9,10,2,'#201d27')+px(4,11,8,2,'#201d27')+px(5,13,6,1,'#201d27')+px(7,14,2,1,'#201d27')
    );
  };

  /* ---------- COIN ---------- */
  I.coin = function(){
    return svg(
      px(5,2,6,1,'#8a5a16')+px(4,3,8,1,'#c9933a')+px(3,4,10,1,'#c9933a')+
      px(3,5,10,6,'#f6cf6b')+px(4,11,8,1,'#c9933a')+px(5,12,6,1,'#c9933a')+px(5,13,6,1,'#8a5a16')+
      px(3,5,2,6,'#ffe9a8')+ // left shine
      px(7,5,2,6,'#c9933a')+px(7,5,2,1,'#8a5a16')+px(7,10,2,1,'#8a5a16') // engraved bar
    );
  };

  /* ---------- PAUSE ---------- */
  I.pause = function(){ return svg(px(3,2,4,12,'currentColor')+px(9,2,4,12,'currentColor')); };
  I.play  = function(){ return svg(px(4,2,2,12,'currentColor')+px(6,3,2,10,'currentColor')+px(8,4,2,8,'currentColor')+px(10,5,2,6,'currentColor')+px(12,7,2,2,'currentColor')); };

  /* ---------- WEAPONS ---------- */
  // Sword
  I.sword = function(){
    return svg(
      px(7,1,2,9,'#dfe6ee')+px(6,2,1,7,'#aeb8c4')+px(9,2,1,7,'#fff')+px(7,1,2,1,'#fff')+
      px(4,10,8,2,'#8a5a16')+ // guard
      px(7,12,2,3,'#5a3a1f')+px(6,15,4,1,'#c9933a') // grip + pommel
    );
  };
  // Magic orb / staff
  I.orb = function(){
    return svg(
      px(6,1,4,1,'#ffd06a')+px(5,2,6,1,'#ffb24a')+px(4,3,8,4,'#ff9a3c')+px(5,7,6,1,'#ff7a1e')+px(6,8,4,1,'#d4621a')+
      px(6,3,2,2,'rgba(255,255,255,.7)')+
      px(7,9,2,6,'#5a3a1f')+px(6,14,4,1,'#3a2414')
    );
  };
  // Aura / holy circle
  I.aura = function(){
    return svg(
      px(6,2,4,1,'#ffe9a8')+px(4,3,2,2,'#ffd06a')+px(10,3,2,2,'#ffd06a')+
      px(2,5,2,6,'#ff9a3c')+px(12,5,2,6,'#ff9a3c')+
      px(4,11,2,2,'#ffd06a')+px(10,11,2,2,'#ffd06a')+px(6,13,4,1,'#ffe9a8')+
      px(6,6,4,4,'rgba(255,200,90,.5)')+px(7,7,2,2,'#fff3d6')
    );
  };
  // Throwing axe / projectile
  I.axe = function(){
    return svg(
      px(8,2,2,9,'#5a3a1f')+ // handle
      px(3,2,6,2,'#cdd6df')+px(2,4,7,3,'#aeb8c4')+px(3,7,5,2,'#dfe6ee')+
      px(3,3,3,1,'#fff') // shine
    );
  };
  // Lightning
  I.bolt = function(){
    return svg(
      px(8,1,3,4,'#bfe6ff')+px(6,5,4,3,'#6fd0ff')+px(7,8,4,3,'#bfe6ff')+px(5,11,4,4,'#2f8fd6')+
      px(8,2,1,3,'#fff')
    );
  };

  /* ---------- PASSIVES ---------- */
  I.boot = function(){ // speed
    return svg(px(5,2,5,7,'#6e4a2a')+px(5,9,8,3,'#8a5a16')+px(4,12,10,2,'#3a2414')+px(6,3,2,4,'#a06a3a')+px(11,10,2,2,'#c9933a'));
  };
  I.clover = function(){ // luck
    return svg(px(4,4,3,3,'#3fae4a')+px(9,4,3,3,'#3fae4a')+px(4,9,3,3,'#3fae4a')+px(9,9,3,3,'#3fae4a')+px(6,6,4,4,'#2f8a38')+px(7,11,2,4,'#5a3a1f')+px(5,5,1,1,'#7fe08a')+px(10,5,1,1,'#7fe08a'));
  };
  I.magnet = function(){ // pickup range
    return svg(px(3,2,3,8,'#c93a2a')+px(10,2,3,8,'#c93a2a')+px(3,2,10,3,'#c93a2a')+px(3,10,3,4,'#cfd6dd')+px(10,10,3,4,'#cfd6dd')+px(4,3,2,1,'#ff6a52'));
  };
  I.tome = function(){ // cooldown / book
    return svg(px(3,3,10,11,'#5a3a1f')+px(4,4,9,9,'#caa15a')+px(7,4,2,9,'#3a2414')+px(5,6,2,1,'#3a2414')+px(9,6,2,1,'#3a2414')+px(5,9,2,1,'#3a2414')+px(9,9,2,1,'#3a2414'));
  };
  I.wing = function(){ // movement / dash
    return svg(px(3,4,2,2,'#dfe6ee')+px(5,5,2,2,'#cdd6df')+px(2,6,3,2,'#fff')+px(4,7,4,2,'#dfe6ee')+px(3,9,6,2,'#aeb8c4')+px(5,11,5,2,'#cdd6df')+px(8,5,5,7,'#eef3f8'));
  };
  I.ring = function(){ // armor
    return svg(px(5,3,6,2,'#b18bff')+px(4,5,2,6,'#9a6ad6')+px(10,5,2,6,'#9a6ad6')+px(5,11,6,2,'#b18bff')+px(7,1,2,3,'#ffd06a')+px(6,6,4,4,'rgba(177,139,255,.3)'));
  };
  I.potion = function(){ // regen
    return svg(px(6,1,4,2,'#aeb8c4')+px(7,3,2,2,'#cdd6df')+px(5,5,6,9,'#3fae4a')+px(5,5,6,2,'#7fe08a')+px(6,9,4,4,'#2f8a38')+px(6,6,1,3,'rgba(255,255,255,.5)'));
  };
  I.skull = function(){ // damage up / curse
    return svg(px(4,3,8,6,'#e6e0d2')+px(3,5,2,3,'#c9c2b0')+px(11,5,2,3,'#c9c2b0')+px(5,5,2,2,'#1a1410')+px(9,5,2,2,'#1a1410')+px(4,9,8,2,'#caa15a')+px(5,11,2,3,'#e6e0d2')+px(9,11,2,3,'#e6e0d2')+px(7,11,2,2,'#e6e0d2'));
  };
  I.heartUp = function(){ // max hp
    return svg(I_inner_heart('#e0473e','#7a1f1c'));
  };
  function I_inner_heart(fill, dark){
    return px(3,3,4,2,fill)+px(9,3,4,2,fill)+px(2,5,12,3,fill)+px(3,8,10,2,fill)+px(4,10,8,2,fill)+px(5,12,6,1,fill)+px(7,13,2,1,fill)+px(4,5,2,2,'rgba(255,255,255,.6)');
  }

  return I;
})();
