# Handoff — ASTRAL SWARM · HUD In-Game (+ Level-Up + Pausa)

Paquete de integración para **Unity 6 (6000.x)**. Interfaz de juego (HUD) para el *VampireSurvivors-like* **ASTRAL SWARM**, en estilo **pixel-art dark fantasy** (referencia: Berserk). Usa el tema **"Forja de Hierro"** (oro de brasa), el mismo del menú principal — ver el paquete `design_handoff_main_menu` para los tokens compartidos.

Cubre **tres vistas**: el **HUD** que se superpone al gameplay, la pantalla de **Subida de Nivel** (3 cartas) y el menú de **Pausa**.

---

## 1. Sobre los archivos de este paquete

Los `.html / .css / .js` de `design_reference/` son **referencias de diseño en HTML** (prototipo funcional). **No son código para copiar a Unity** — recrea el diseño con el sistema de UI del proyecto.

- **Recomendado: Unity UI Toolkit (UXML + USS)** — mapea casi 1:1 con este HTML/CSS (flexbox, `var(--*)`, clases, `:hover`, transiciones). Un HUD se hace genial con un `UIDocument` superpuesto a la cámara del juego.
- **uGUI (Canvas + prefabs)** también vale: `Canvas` en `Screen Space - Overlay`, cada chip/slot un prefab con `Image` 9-slice + `TextMeshProUGUI`.

Si el repo ya usa un sistema, respétalo. Lee `hud.css` para medidas/colores exactos y `hud.js` para el comportamiento.

> **Fidelidad: ALTA.** Posiciones, tamaños y colores son finales. Los `.png` de `screenshots/` son la fuente de verdad visual.

---

## 2. Resolución y escalado

- **Lienzo de diseño: 1600 × 900 px** (16:9; coincide con la captura de gameplay aportada).
- El HUD vive en un *stage* 1600×900 que **se escala uniformemente para encajar la ventana** y se centra con *letterbox*. Escala = `min(vw/1600, vh/900)`.
- **El HUD es resolución-independiente:** está anclado a los bordes (no posiciones absolutas dependientes del centro). Reproduce los **anclajes**, no las coordenadas en píxeles literales, para que funcione en cualquier aspecto:
  - **UI Toolkit:** usa `position:absolute` + `top/left/right/bottom` igual que el CSS (ya son anclajes). `PanelSettings` → `Scale With Screen Size`, ref `1600×900`.
  - **uGUI:** usa los *anchors* del `RectTransform` (esquinas) descritos en §3. `Canvas Scaler` → `Scale With Screen Size`, ref `1600×900`, `Match` 0.5.

---

## 3. Layout del HUD — anclajes y medidas

Todo se ancla a un borde de la pantalla. **Z-order:** gameplay (0) → scrim (1) → HUD (5) → viñeta de daño (8) → overlays (20-22).

### 3.1 Barra de XP + Nivel — *arriba, ancho completo*
| Elemento | Valor |
|---|---|
| Barra XP | pegada al **borde superior**, ancho 100%, alto `22px` |
| Track | fondo `#0c0b12`, borde inferior `3px` `#08070c` |
| Relleno | degradado **azul** `#6fd0ff → #2f8fd6` (XP es azul, contrasta con el oro del tema), glow `rgba(90,190,255,.6)`. Transición al subir, en *steps* |
| Marcas de segmento | líneas verticales oscuras cada ~80px (separadores de nivel) |
| **Badge LVL** | esquina **sup-izq**, `top 8px / left 18px`, alto `44px`. Fondo degradado oro `#f6cf6b → #c9933a`, texto oscuro `#1c1206`. "LVL" en `Silkscreen` 12px + número en `Pixelify Sans` 28px |

### 3.2 Timer — *arriba centro*
| Propiedad | Valor |
|---|---|
| Posición | `top 34px`, centrado horizontal |
| Plaqueta | bevel metal oscuro (ver §5), padding `8px 26px 10px` |
| Etiqueta | "RONDA" — `Silkscreen` 10px, letter-spacing 4px, color `#b6a988` |
| Reloj | `mm:ss` — `Pixelify Sans` 700, **44px**, color `#fff3d6`, glow ámbar, `tabular-nums` |

### 3.3 Vida (corazones) + Oro — *sup-izq, bajo la barra*
- **Corazones** (`top 64px / left 18px`): fila con `gap 4px`. Cada corazón **34×30px**, pixel-art estilo Zelda. Tres estados: **lleno** (rojo `#e0473e` + sombra `#7a1f1c` + brillo blanco), **medio** (mitad izq roja / mitad der vacía), **vacío** (gris `#201d27`). La vida puede ser fraccionaria (medios corazones). `hpMax` = nº de corazones.
- **Oro** (debajo, `gap 10px`): chip bevel, alto `40px`. Moneda pixel **24×24px** + cantidad en `Pixelify Sans` 700 22px color oro `#f6cf6b`, `tabular-nums`. Formato con separador de miles.

### 3.4 Pausa — *sup-der*
Botón cuadrado **52×52px** (`top 30px / right 22px`), bevel. Icono ❚❚ (pausa) en oro; cambia a ▶ (play) cuando el juego está pausado. Hover: brillo + glow ámbar. Active: baja 2px.

### 3.5 Loadout (armas + pasivos) — *inf-izq*
`bottom 20px / left 20px`, dos filas con etiqueta a la izquierda (`Silkscreen` 9px, color `#b6a988`).
- **Fila ARMAS:** **3 slots** de **56×56px** (bevel). Cada uno: icono pixel **38×38px** + **pips de nivel** (barritas `3×6px` doradas, abajo-der, una por nivel, máx 8). Slot maxeado (Nv 8) lleva glow ámbar permanente. Slots vacíos muestran `+` gris.
- **Fila PASIVOS:** slots **46×46px** (icono 30×30px), **número ilimitado**, hacen *wrap* (máx ancho ~560px). Sin pips.

> **Diseño del juego:** 3 armas máximo (slots fijos), pasivos infinitos. Confirmado por el usuario.

### 3.6 Minimapa — *inf-der*
`bottom 20px / right 20px`, **184×184px**, marco bevel con padding 6px. Interior: gradiente verde con rejilla sutil (`repeating-linear-gradient` cada 11-12px) + borde oscuro. Blips:
- **Jugador:** cuadrado blanco `9×9px` con glow, centro fijo (50%,50%).
- **Enemigos ("enjambre"):** `5×5px` rojos, orbitan/derivan hacia el jugador (~34 blips).
- **Loot:** `5×5px` dorados con glow.
- Etiqueta "MAPA" arriba-izq (`Silkscreen` 9px).

### 3.7 Viñeta de daño
Capa a pantalla completa (z-index 8), `box-shadow: inset 0 0 120px 30px rgba(200,30,20,.6)`, normalmente `opacity:0`. Pulso a `1` ~160ms al recibir daño. Decorativa.

---

## 4. Pantalla de SUBIDA DE NIVEL (overlay, z-index 20)

Se abre al llenar la barra de XP. **Pausa el juego.** Fondo: gradiente radial oscuro (`rgba(10,8,16,.72)`→`rgba(6,5,11,.9)`) + blur opcional.

- **Encabezado:** "¡Nivel Superior!" en blackletter **UnifrakturCook 64px** oro con contorno + glow (una sola línea, `nowrap`). Subtítulo "ELIGE TU RECOMPENSA" en `Silkscreen` 13px, letter-spacing 6px. Entra con animación *drop-in* en *steps*.
- **3 cartas** (`gap 24px`), cada una **300px ancho / min 360px alto**:
  - Panel bevel oscuro con **studs** de acento en las 2 esquinas superiores.
  - **Rareza** arriba (`Silkscreen` 10px): `Común` / `Raro` / `Épico` (color ámbar `#ffd06a`).
  - **Icono** en marco **96×96px** (icono pixel 64×64px) con glow radial.
  - **Nombre** (`Pixelify Sans` 700, 25px) + **tipo** (`Arma`/`Pasivo`, oro, `Silkscreen` 11px).
  - **Descripción** (`Pixelify Sans` 16px, `text-wrap:pretty`, color `#b6a988`).
  - **Hint** "▸ Elegir ◂" abajo, aparece en hover.
  - **Hover:** la carta sube `-8px`, brillo +8%, glow ámbar fuerte. Entrada escalonada (animación `cardIn` con delays 0.04/0.1/0.16s).
- **Acciones** (`reroll-row`, `gap 14px`):
  - **Rerollear** (badge con nº de rerolls restantes) — vuelve a tirar las 3 cartas.
  - **Saltar (+50)** — cierra y da 50 de oro.
- **Al elegir** una carta: si es arma existente sube su nivel (pips); si es arma nueva y hay slot libre, se añade; si es pasivo, se añade a la fila (o sube vida máx para "Corazón de Hierro").

**Pool de mejoras** (en `hud.js`, `UPGRADE_POOL`): 5 armas (Espada Rúnica, Orbe Ardiente, Aura Sagrada, Centella, Hacha Giratoria) + 9 pasivos (Botas Veloces, Imán Arcano, Tomo Prohibido, Alas de Cuervo, Corazón de Hierro, Maldición, Vial de Vida, Trébol, Anillo de Égida). Cada entrada: `id, name, kind, rarity, desc`.

---

## 5. Pantalla de PAUSA (overlay, z-index 22)

Botón de pausa, `Esc` o `P`. Fondo `rgba(6,5,11,.72)` + blur.
- **Panel** **460px** ancho, bevel oscuro, padding `36px 44px 40px`.
- **Título** "Pausa" (UnifrakturCook 56px oro) + **regla** de acento (degradado horizontal).
- **Fila de stats** (4): `TIEMPO · NIVEL · MUERTES · ORO`. Número en `Pixelify Sans` 700 26px oro, etiqueta en `Silkscreen` 9px. Separada por líneas finas arriba/abajo.
- **3 botones** ancho completo, alto `60px`:
  - **Reanudar** (primario, fondo oro, texto oscuro) → cierra pausa.
  - **Ajustes** → engancha aquí la pantalla de Ajustes del menú (ver `design_handoff_main_menu`).
  - **Abandonar** (peligro, hover rojizo) → salir a menú principal.
- Se cierra con `Esc`/`P` o clic fuera del panel.

---

## 6. Estética compartida — bevel pixel-art

Todos los marcos (chips, slots, plaquetas, paneles, botones) usan el **bevel pixel** del tema Forja de Hierro:
```
border: 3px solid #08070c;                 /* slots/chips; paneles usan 4px */
box-shadow:
  0 0 0 3px #08070c,                        /* segundo borde exterior */
  inset 2px 2px 0 rgba(255,255,255,.16),    /* highlight sup-izq */
  inset -3px -3px 0 rgba(0,0,0,.55),        /* sombra inf-der */
  0 4px 10px rgba(0,0,0,.45);               /* sombra proyectada */
background: linear-gradient(180deg,#3c3c46 0%,#1b1b22 60%,#14141a 100%); /* metal */
```
- **`border-radius: 0`** en todo (estética pixel).
- **`image-rendering: pixelated`** en todo lo gráfico; en Unity, texturas con **Filter Mode = Point**, **Compression = None**.
- En uGUI, el bevel se hace con un **sprite 9-slice** que ya lleve pintado el doble borde + bisel.

---

## 7. Design tokens (tema Forja de Hierro)

| Token | Valor | Uso |
|---|---|---|
| accent | `#ff9a3c` | rombos, glows, studs |
| accent-2 | `#ffd06a` | rareza, hints |
| accent-glow | `rgba(255,150,50,.6)` | resplandores |
| gold | `#f6cf6b` | XP badge, oro, primario |
| gold-deep | `#c9933a` | degradado oro |
| ink / frame | `#08070c` | bordes/contornos |
| text | `#e7dcc4` | texto principal |
| text-dim | `#b6a988` | etiquetas |
| hp | `#e0473e` | corazón lleno |
| hp-dark | `#7a1f1c` | sombra corazón |
| xp fill | `#6fd0ff → #2f8fd6` | barra XP (azul) |
| panel | `#2c2832 → #16141c` | paneles overlay |

**Tipografías** (Google Fonts, OFL — ver guía de importación en `design_handoff_main_menu/README.md` §8):
- **UnifrakturCook** 700 → títulos de overlay ("¡Nivel Superior!", "Pausa").
- **Pixelify Sans** 400–700 → UI, números, nombres.
- **Silkscreen** 400/700 → etiquetas, rareza, stats.

**Escala de texto (px):** 9 (etiquetas) · 10 (rareza/timer-label) · 11 (tipo/stat-label) · 13 (subtítulo) · 16 (desc) · 18–22 (botones/oro) · 25 (nombre carta) · 26 (stat) · 28 (nº nivel) · 44 (timer) · 56–64 (títulos overlay).

**Componentes (px):** corazón `34×30` · slot arma `56×56` · slot pasivo `46×46` · icono arma `38` · icono pasivo `30` · icono carta `64` (marco 96) · pausa-btn `52×52` · minimapa `184×184` · carta `300×360` · panel pausa `460`.

---

## 8. Iconos pixel-art

Todos los iconos son **SVG pixel-art** dibujados a medida (rejilla de 16 unidades, `shape-rendering:crispEdges`), en `design_reference/hud-icons.js`. Para Unity, **recrearlos como sprites pixel** (16×16 o 32×32, Point filter) o exportar los SVG a PNG.

| id | Qué es | Tipo |
|---|---|---|
| `sword` | espada rúnica | arma |
| `orb` | orbe ardiente | arma |
| `aura` | aura sagrada | arma |
| `bolt` | centella (rayo, azul) | arma |
| `axe` | hacha giratoria | arma |
| `boot` | botas (velocidad) | pasivo |
| `clover` | trébol (suerte, verde) | pasivo |
| `magnet` | imán (rango recogida) | pasivo |
| `tome` | tomo (enfriamiento) | pasivo |
| `wing` | alas (evasión) | pasivo |
| `ring` | anillo (armadura, violeta) | pasivo |
| `potion` | vial (regen, verde) | pasivo |
| `skull` | maldición (daño) | pasivo |
| `heartUp` | corazón de hierro (+vida máx) | pasivo |
| `coin` | moneda de oro | HUD |
| `heartFull/Half/Empty` | corazones de vida | HUD |
| `pause / play` | controles de pausa | HUD |

---

## 9. Comportamiento / lógica (resumen de `hud.js`)

- **Bucle principal** (`requestAnimationFrame`): el timer cuenta hacia abajo; la XP sube y al llegar a 1.0 sube de nivel y abre el overlay de Subida de Nivel; oro y muertes suben por tics; los enemigos del minimapa orbitan. Se **congela** mientras un overlay está abierto.
- **Persistencia:** el estado (nivel, xp, vida, oro, armas, pasivos) se guarda en `localStorage` (`astral.hud`). En Unity, persistir en el `GameState`/`SaveData` de la run.
- **Hook de preview:** `window.ASTRAL_HUD` expone `.levelUp()`, `.pause()`, `.resume()`, `.closeLevelUp()`, `.state` — útil para saltar a cualquier estado desde consola al recrearlo.
- **Atajos:** `Esc`/`P` pausa · `L` fuerza subida de nivel (dev).

### Implementación sugerida (UI Toolkit)
```
Assets/UI/HUD/
  HUD.uxml            // xp-bar, lvl-badge, timer, hearts, gold, pause, loadout, minimap
  HUD.uss            // bevel, tokens, layout (equivale a hud.css)
  LevelUp.uxml/.uss   // overlay de 3 cartas
  Pause.uxml/.uss     // overlay de pausa
  HUDController.cs    // bindea stats del gameplay → HUD; abre overlays; pausa Time.timeScale
  LevelUpController.cs// genera 3 opciones del pool, hover, pick, reroll
  hud-icons/          // sprites de los iconos (§8)
```
- **Pausa real:** `Time.timeScale = 0` al abrir overlays; restaurar a `1` al cerrar.
- **Vida:** mapear `hp`/`hpMax` a la fila de corazones (soporta medios).
- **Subida de nivel:** al subir XP, instanciar 3 cartas desde el pool del juego; aplicar el efecto al elegir.

---

## 10. Assets incluidos

| Archivo | Qué es |
|---|---|
| `design_reference/assets/gameplay.png` | Captura de gameplay del usuario (1599×902), usada como fondo del mockup. **No es un asset del HUD** — solo contexto. |
| `screenshots/01-hud.png` | HUD en juego |
| `screenshots/02-levelup.png` | Pantalla de Subida de Nivel |
| `screenshots/03-pause.png` | Menú de Pausa |

Las fuentes no se incluyen (Google Fonts; ver §7 y el README del menú).

---

## 11. Archivos de referencia

En `design_reference/`:

| Archivo | Contenido |
|---|---|
| `ingame.html` | Mockup completo del HUD + overlays sobre el gameplay. Ábrelo en un navegador para ver el timer corriendo, XP subiendo, hover en cartas, pausa, level-up. |
| `hud.css` | Layout + estética (equivale a `HUD.uss`). **Fuente de verdad de medidas y colores.** |
| `hud-icons.js` | Librería de iconos pixel-art SVG (§8). |
| `hud.js` | Lógica: bucle, estados, overlays, pool de mejoras, persistencia, hook de preview. |

> El tema y las fuentes son los mismos del menú principal — empareja este paquete con `design_handoff_main_menu` para tokens y guía de fuentes compartidos.
