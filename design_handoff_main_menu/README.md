# Handoff — ASTRAL SWARM · Menú Principal + Ajustes

Paquete de integración para **Unity 6 (6000.x)**. Diseño pixel-art dark-fantasy (referencia: Berserk / castlevania-like) para un juego *VampireSurvivors-like*.

Incluye **tres direcciones visuales intercambiables como temas** (A · Forja de Hierro, B · Grimorio, C · Enjambre Astral). El layout, las medidas y el comportamiento son **idénticos** entre las tres — sólo cambian colores, tipografías y materiales. Por eso este documento describe **una sola estructura** y luego una **tabla de tokens por tema**.

---

## 1. Sobre los archivos de este paquete

Los archivos `.html / .css / .js` de la carpeta `design_reference/` son **referencias de diseño hechas en HTML** — prototipos que muestran el aspecto y el comportamiento deseados. **No son código para copiar tal cual** a Unity.

La tarea es **recrear estos diseños dentro de Unity 6** usando su entorno de UI. Recomendación fuerte y por qué:

- **Usa Unity UI Toolkit (UXML + USS).** Mapea casi 1:1 con este HTML/CSS: USS soporta flexbox, `var(--custom-properties)`, clases, pseudo-estados (`:hover`, `:active`), transiciones y `background-image`. Cambiar de tema = cambiar una clase en el elemento raíz (igual que el atributo `data-variant` del prototipo). Esta es la ruta más rápida y fiel.
- **uGUI (Canvas + prefabs)** también es válido si el proyecto ya lo usa. En ese caso, cada panel/botón es un prefab con `Image` (sliced 9-slice) + `TextMeshPro`, y los temas se hacen con `ScriptableObject` de paleta que pinta los `Image.color` / materiales. Es más trabajo manual; ver §9.

Si el repo ya tiene un sistema de UI establecido, respétalo. Si no, **elige UI Toolkit**.

> **Fidelidad: ALTA (hi-fi).** Colores, tipografías, tamaños y espaciados son finales. Reprodúcelos con precisión (valores exactos en §6 y §7).

---

## 2. Resolución y escalado

- **Lienzo de diseño nativo: 1408 × 768 px** (relación ~16:9, es el tamaño del arte de fondo).
- Todo el menú vive en un *stage* de 1408×768 que se **escala uniformemente para encajar en la ventana** y se centra con *letterbox* (barras negras). Nunca se estira de forma no uniforme.
  - Escala = `min(viewportWidth / 1408, viewportHeight / 768)`.
- **UI Toolkit:** usa un `PanelSettings` con `Scale Mode = Scale With Screen Size`, `Reference Resolution = 1408 × 768`, `Screen Match Mode = Match Width Or Height` con `Match = 0.5` (o `Expand`/letterbox según convenga). Esto reproduce el `transform: scale()` del prototipo.
- **uGUI:** `Canvas Scaler` → `Scale With Screen Size`, `Reference Resolution = 1408×768`, `Match = 0.5`.

---

## 3. Pantallas / Vistas

Hay **dos pantallas** + dos overlays, todas sobre el mismo fondo.

### 3.1 Menú Principal (`screen--menu`)
Capas, de atrás hacia delante:

1. **Fondo** — `background.jpeg` (1408×768), `object-fit: cover`, **`image-rendering: pixelated`** (filtro Point/No Filter en Unity; NO bilinear).
2. **Luces de antorcha/fuego** (`.glows`) — 6 resplandores radiales con *blend mode screen* que parpadean (ver §5). Posiciones en §5.
3. **Brasas flotantes** (`.embers`) — partículas que suben (sólo visibles en tema C por defecto; ver §5).
4. **Viñeta** (`.vignette`) — oscurecimiento radial + degradado vertical para que la UI lea bien.
5. **Título + tagline** (`.title-wrap`) — arriba, centrado.
6. **Botones de menú** (`.menu-buttons`) — centrados vertical y horizontalmente.
7. **Footer** (`.menu-footer`) — abajo-izquierda: número de versión.

**Layout (medidas exactas):**

| Elemento | Valor |
|---|---|
| Título — margen superior | `58px` desde el borde del stage |
| Título — fuente | ver tema (§6); tamaño `104px`, line-height `0.9`, letter-spacing `2px`, `text-transform: none` |
| Título — contorno | stroke de `3px` color *title-stroke* + sombra dura (`0 4px 0` stroke) |
| Tagline | `17px`, letter-spacing `8px`, MAYÚSCULAS, fuente pixel (Silkscreen) |
| Tagline — margen superior | `10px` bajo el título |
| Bloque de botones | centrado (`margin: auto` vertical), `gap: 22px` entre botones, columna |
| Botón (`.mbtn`) | **430 × 76 px**, fuente UI `32px`, weight 600, letter-spacing `4px`, MAYÚSCULAS, borde `4px` |
| Footer | abajo, `bottom: 18px`, padding lateral `26px`, fuente pixel `13px`, letter-spacing `2px`, opacidad ~62% |

**Orden de botones (de arriba a abajo):** `JUGAR` (primario), `AJUSTES`, `SALIR` (peligro).

### 3.2 Ajustes (`screen--settings`)
Overlay modal sobre el menú. Fondo del overlay: `rgba(6,5,11,.6)` + `backdrop-filter: blur(2px)` (en Unity, un blur de fondo opcional; si no, basta el oscurecimiento).

**Panel (`.panel`):**

| Propiedad | Valor |
|---|---|
| Ancho | `760px` |
| Padding | `30px 46px 40px` |
| Borde | `4px` color *panel-border* + segundo borde externo `4px` (box-shadow `0 0 0 4px`) |
| Sombra interior | bisel: highlight arriba-izq, sombra abajo-der (efecto pixel-art 3D) |
| Studs decorativos | 2 cuadrados de `12px` (color acento) en las esquinas superiores izq/der, a `10px` del borde |
| Animación de entrada | `translateY(16px) scale(.985)` → normal, `~0.22s`, en pasos (steps) para look retro |

**Contenido del panel, de arriba a abajo:**

1. **Título "Ajustes"** — `58px`, fuente título del tema, centrado.
2. **Regla** (`.panel-rule`) — línea de `4px`, 60% de ancho, degradado horizontal con color acento al centro, opacidad ~70%.
3. **Grupo "MODO DE PANTALLA"** — etiqueta (`14px`, letter-spacing `3px`, MAYÚSC, fuente pixel) + **control segmentado** de 3 opciones:
   - `PANTALLA COMPLETA` (activo por defecto) · `SIN BORDES` · `VENTANA`
   - Grid de 3 columnas, `gap: 10px`, cada opción `height: 50px`, fuente UI `17px`.
   - La opción activa usa el degradado de acento del tema; las inactivas, el material oscuro.
4. **3 sliders** (filas, `margin-bottom: 20px` cada una):
   - `VOLUMEN GENERAL` — valor por defecto **80%**
   - `MÚSICA` — **65%**
   - `EFECTOS DE SONIDO` — **75%**
   - Cada fila: etiqueta a la izquierda (fuente pixel `14px`) + **porcentaje a la derecha** (fuente UI `20px`, color acento).
   - Pista (`track`): alto `14px`, fondo oscuro, borde `3px`. Relleno (`fill`): degradado de acento con glow. Mango (`handle`): **22 × 34 px**, bisel claro, borde `3px`.
5. **Botón "VOLVER"** (`.mbtn--back`) — `300 × 64 px`, fuente `26px`, centrado. Vuelve al menú.

### 3.3 Overlay de confirmación de salida (`.modal-wrap`)
Se abre al pulsar `SALIR`. Fondo `rgba(4,3,8,.72)`. Caja `520px` de ancho, centrada:
- Título **"¿Abandonar?"** (`40px`, fuente título).
- Texto: *"Tu progreso sin guardar se perderá."* (`19px`, fuente UI).
- Dos botones (`200 × 60 px`, `gap: 18px`): **SALIR** (estilo peligro) y **QUEDARSE** (estilo primario/gold).
- Se cierra con `Escape` o clic fuera de la caja.

### 3.4 Overlay de transición "Jugar" (`.play-overlay`)
Al pulsar `JUGAR`: overlay a pantalla completa, fondo radial oscuro, fade-in `0.4s`. Contenido centrado:
- Título **"Invocando el Enjambre"** (`64px`, pulsa suavemente, glow de acento).
- 4 cuadrados de `12px` que parpadean en secuencia (spinner pixel).
- Texto *"Preparando la incursión…"* (`15px`, letter-spacing `6px`, MAYÚSC).
- En el prototipo dura 2.2s y vuelve; **en Unity, aquí va la carga real de la escena de juego** (`SceneManager.LoadSceneAsync`).

---

## 4. Interacciones y comportamiento

| Acción | Trigger | Resultado |
|---|---|---|
| `JUGAR` | clic | Muestra overlay de transición → cargar escena de partida |
| `AJUSTES` | clic | Abre panel de Ajustes (sobre el menú) |
| `VOLVER` | clic | Cierra Ajustes |
| `SALIR` | clic | Abre modal de confirmación |
| `SALIR` (en modal) | clic | `Application.Quit()` (en editor: parar play) |
| `QUEDARSE` | clic | Cierra modal |
| `Escape` | tecla | Cierra modal si está abierto; si no, cierra Ajustes |
| Clic fuera del modal | clic en el fondo | Cierra modal |

**Estados de los botones (hi-fi, reprodúcelos):**
- **Hover:** se desplaza `-3px, -3px` (efecto "se levanta"), brillo +14%, aparece un glow de acento alrededor, y aparecen dos rombos `◆` de acento a izquierda y derecha del texto. Transiciones cortas y en *steps* (look retro), ~`0.09–0.14s`.
- **Active (pressed):** vuelve a `0,0`, se "hunde" (bisel invertido), brillo −4%.
- **Botón primario `JUGAR`:** relleno con degradado *gold/acento* del tema, texto oscuro.
- **Botón peligro `SALIR`:** acento rojo en el glow/rombos.

**Segmented control (modo pantalla):** al hacer clic en una opción, se desactivan las demás y se activa esa. Hover = brillo +18%.

**Sliders:** arrastrables (pointer down/move/up) y por teclado (←/→ = ±5, Home/End = 0/100). El relleno y el mango siguen el valor; el porcentaje a la derecha se actualiza en vivo.

**Animaciones ambientales:** ver §5. Son puramente decorativas; no bloquean input.

---

## 5. Ambiente animado (luces + brasas)

### Luces de antorcha/fuego (`flicker`)
6 resplandores radiales posicionados como **% del stage** (x, y desde la esquina superior izquierda), cada uno parpadeando con su propio ritmo. Usar *additive/screen blending*.

| Luz | Pos (x%, y%) | Tamaño aprox | Color | Ritmo |
|---|---|---|---|---|
| Luna (halo) | 37.5%, 12.5% | 260px | azul frío `rgba(150,170,255,…)` | "respira" 7s (suave) |
| Portón del castillo | 32.2%, 39% | 120px | cálido naranja | parpadeo 2.1s |
| Sendero | 29.5%, 71% | 200×220px | cálido | 1.7s |
| Aldea | 71.3%, 69% | 180×200px | cálido | 2.3s |
| Hoguera (der) | 83.4%, 84% | 300×280px | brasa intensa `rgba(255,140,40,…)` | 1.3s (rápido) |
| Farol | 92.3%, 80% | 130px | cálido | 2.8s |

Parpadeo = oscilar opacidad entre ~0.55 y 1.0 y escala entre ~0.96 y 1.05, en *steps* (6–10 pasos) para sensación de fuego pixelado. En Unity: animar `opacity`/`scale` de los elementos (UI Toolkit) o `Image.color.a`/`localScale` con una corrutina/`AnimationCurve`, o un shader de flicker.

### Brasas flotantes (`rise`)
~26 partículas pequeñas (`2–5px`) que suben desde abajo, con deriva horizontal aleatoria (`±60px`), duración `7–14s`, fade-in/out. Color = acento del tema. **Por defecto sólo activas en el tema C (Enjambre Astral)** (en A y B su opacidad es 0). En Unity, un `ParticleSystem` simple lo resuelve mejor que reproducir el CSS.

> Todo el ambiente es opcional para un primer pase funcional, pero da mucho carácter. Prioriza el flicker de las luces.

---

## 6. Temas (3 direcciones intercambiables)

Cada tema sólo remapea un set de **tokens** (colores + fuentes). La estructura y medidas no cambian. En UI Toolkit, define cada tema como un bloque de `var(--*)` en `:root.theme-forged` / `.theme-grimoire` / `.theme-astral` y cambia la clase del elemento raíz para cambiar de tema.

### Tokens comunes (concepto)
`accent`, `accent-glow`, `title-color`, `title-stroke`, `tagline-color`, `btn-text`, `btn-bg`, `btn-border`, `play-bg` (relleno botón primario), `play-text`, `panel-bg`, `panel-border`, `label-color`.

### A · FORJA DE HIERRO — *hierro, sangre y oro de brasa*
| Token | Valor |
|---|---|
| Fuente título | **UnifrakturCook** (blackletter) |
| Fuente UI | **Pixelify Sans** |
| Fuente etiquetas | **Silkscreen** |
| accent | `#ff9a3c` |
| accent-glow | `rgba(255,150,50,.6)` |
| title-color | `#f0cf7e` |
| title-stroke | `#1a0f06` |
| tagline-color | `#cbb892` |
| btn-text | `#e7dcc4` (hover `#fff3d6`) |
| btn-bg | degradado `#3c3c46 → #1b1b22 → #14141a` (metal) + remaches en esquinas |
| btn-border / outline | `#08070c` |
| play-bg (JUGAR) | degradado `#f6cf6b → #d4a443 → #b27e2c` (oro) |
| play-text | `#1c1206` |
| panel-bg | degradado `#2c2832 → #16141c` (piedra oscura) |
| panel-border | `#08070c` |
| label-color | `#cbb892` |

### B · GRIMORIO — *cuero quemado, pergamino y cera carmesí*
| Token | Valor |
|---|---|
| Fuente título | **Pirata One** |
| Fuente UI | **Jersey 25** |
| Fuente etiquetas | **Silkscreen** |
| accent | `#d24b3a` (rojo carmesí) |
| accent-glow | `rgba(200,50,35,.6)` |
| title-color | `#ecdcb6` |
| title-stroke | `#1c0d09` |
| btn-text | `#ecdcb6` |
| btn-bg | degradado `#4a3526 → #2c1d13 → #20140c` (cuero) |
| btn-border | `#160c07` |
| play-bg (JUGAR) | degradado `#d24b3a → #a32f25 → #7c2019` (rojo) |
| play-text | `#fff0e6` |
| **panel-bg** | **pergamino claro**: degradado `#e9d4a4 → #d2b783` |
| panel-border | `#3a2414` |
| panel — texto | etiquetas `#5a3a1f`, título `#6e231a`, valores `#9e2b1e` |

> Nota: en el Grimorio el panel de ajustes es **claro (pergamino)** para contrastar con A y C, que son oscuros. Los textos del panel se invierten a marrones oscuros. Ver `B-grimoire-settings.png`.

### C · ENJAMBRE ASTRAL — *obsidiana y runas violetas*
| Token | Valor |
|---|---|
| Fuente título | **UnifrakturCook** |
| Fuente UI | **Pixelify Sans** |
| Fuente etiquetas | **Silkscreen** |
| accent | `#b18bff` (violeta) |
| accent-glow | `rgba(150,110,255,.6)` |
| title-color | `#ece4ff` (con glow violeta fuerte) |
| title-stroke | `#1a1030` |
| btn-text | `#d9d0f5` (hover `#ffffff`) |
| btn-bg | degradado `#241d3a → #150e26 → #0d0918` (obsidiana) |
| btn-border | `#080612`; **outline con tinte violeta** `#4a3a86` |
| play-bg (JUGAR) | degradado `#a98bff → #7a55e6 → #5b3fc4` (violeta) |
| play-text | `#130b28` |
| panel-bg | degradado `#1c1632 → #0d0a18` |
| panel-border | `#4a3a86` (violeta) |
| label-color | `#bdb0e6` |
| Extra | botones con **rune-glow** violeta permanente; **brasas violetas activas** |

---

## 7. Design tokens — escalas globales

**Tipografía (px):** 13 (footer) · 14 (etiquetas) · 15 (texto overlay) · 17 (tagline / segment) · 19 (texto modal) · 20 (valor slider) · 26 (botón volver) · 32 (botón menú) · 40 (título modal) · 58 (título panel) · 64 (título overlay) · 104 (título principal).

**Pesos:** UI 500/600; títulos 700.
**Letter-spacing:** botones `4px`, título `2px`, tagline `8px`, etiquetas/segment `3px`, footer `2px`.

**Tamaños de componentes (px):**
- Botón menú: `430 × 76`
- Botón volver: `300 × 64`
- Botón modal: `200 × 60`
- Opción segment: alto `50`
- Slider track: alto `14`; handle: `22 × 34`
- Panel ajustes: ancho `760`
- Modal salir: ancho `520`
- Stud de esquina: `12 × 12`

**Espaciados clave (px):** gap botones `22` · padding panel `30/46/40` · gap segment `10` · margin filas slider `20` · gap botones modal `18`.

**Bordes:** todo es **recto (border-radius: 0)** — estética pixel. Grosores: botones `4px`, segment/slider `3px`, panel `4px` (+ 4px externo).

**Sombras / bisel:** efecto 3D pixel = highlight interior arriba-izquierda (`inset 3px 3px 0 rgba(255,255,255,.16)`) + sombra interior abajo-derecha (`inset -4px -4px 0 rgba(0,0,0,.5)`) + sombra proyectada (`0 8px 14px rgba(0,0,0,.45)`). En UI Toolkit se aproxima con `border` claro/oscuro o `background` de 9-slice; en uGUI, sprites 9-slice con el bisel pintado.

**`image-rendering: pixelated`** en TODO lo gráfico (fondo, botones, mangos). En Unity: texturas con `Filter Mode = Point (no filter)` y `Compression = None` para arte pixel nítido.

---

## 8. Fuentes — importar a Unity

Las fuentes son de **Google Fonts** (OFL, libres para uso comercial). Descárgalas y conviértelas a **TextMeshPro Font Assets** (uGUI) o regístralas como `FontDefinition` (UI Toolkit).

| Uso | Familia | Google Fonts | Pesos |
|---|---|---|---|
| Título (A, C) | **UnifrakturCook** | fonts.google.com/specimen/UnifrakturCook | 700 |
| Título (B) | **Pirata One** | fonts.google.com/specimen/Pirata+One | 400 |
| UI (A, C) | **Pixelify Sans** | fonts.google.com/specimen/Pixelify+Sans | 400–700 |
| UI (B) | **Jersey 25** | fonts.google.com/specimen/Jersey+25 | 400 |
| Etiquetas/pixel | **Silkscreen** | fonts.google.com/specimen/Silkscreen | 400, 700 |

**Pasos (TextMeshPro):**
1. Descarga los `.ttf` de Google Fonts.
2. Colócalos en `Assets/Fonts/`.
3. `Window → TextMeshPro → Font Asset Creator`. Para cada fuente: *Sampling Point Size* alto (p.ej. 90), *Atlas Resolution* 1024×1024 (2048 para el título), *Character Set = ASCII* + añade `¿ ¡ á é í ó ú ñ` (textos en español). Genera y guarda el Font Asset.
4. **Para look pixel nítido**, en el material del Font Asset baja/anula el suavizado: pon *Filter Mode = Point* en el atlas y reduce *Gradient/Softness* a 0. Las fuentes pixel (Pixelify, Silkscreen, Jersey 25) deben verse duras, no borrosas.

> Si prefieres no usar estas fuentes, cualquier fuente pixel blackletter (título) + pixel sans (UI) equivalente sirve; mantén la jerarquía de tamaños de §7.

---

## 9. Implementación sugerida (UI Toolkit)

```
Assets/UI/MainMenu/
  MainMenu.uxml          // estructura: stage > (bg, glows, embers, vignette, screen-menu, screen-settings, modal, play-overlay)
  MainMenu.uss           // estilos base (medidas, bisel, estados :hover/:active) — equivale a menu.css
  Theme.Forged.uss       // bloque de var(--*)  ← tema A
  Theme.Grimoire.uss     // tema B
  Theme.Astral.uss       // tema C
  MainMenuController.cs   // navegación, sliders, modal, carga de escena, audio
```

- Cambiar de tema: `root.ClassListRemove(...)` / `root.AddToClassList("theme-astral")`.
- Sliders: usa el `Slider` nativo de UI Toolkit con USS para clonar el look (track/fill/handle), o un `VisualElement` custom con `PointerMoveEvent`.
- Conecta los valores de volumen a un `AudioMixer` (`SetFloat("MasterVolume", Mathf.Log10(v)*20)`), y persístelos con `PlayerPrefs` (el prototipo usa `localStorage` con claves `astral.<tema>.master|music|sfx|displaymode`).
- Modo de pantalla: `Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen | FullScreenWindow | Windowed` para las 3 opciones del segmented.

**Si usas uGUI:** un `Canvas` con `CanvasScaler` (§2); cada botón = `Button` + `Image` 9-slice + hijo `TextMeshProUGUI`; estados hover/press con `Animator` o `ColorTint`+script; temas vía un `ScriptableObject ThemePalette` que un `ThemeApplier` reparte a las `Image`/texto al iniciar.

---

## 10. Assets incluidos

| Archivo | Qué es |
|---|---|
| `design_reference/assets/background.jpeg` | Arte de fondo del menú, **1408×768**. Úsalo tal cual como textura (Point filter). Es del usuario. |
| `screenshots/A-forged-*.png` | Referencia visual tema A (menú + ajustes) |
| `screenshots/B-grimoire-*.png` | Referencia visual tema B |
| `screenshots/C-astral-*.png` | Referencia visual tema C |

Las fuentes **no** se incluyen (descárgalas de Google Fonts, §8).

---

## 11. Archivos de referencia (prototipo HTML)

En `design_reference/`:

| Archivo | Contenido |
|---|---|
| `forged.html` / `grimoire.html` / `astral.html` | Una dirección cada uno. Mismo DOM; cambia `data-variant` en `.stage`. Ábrelos en un navegador para ver hover, sliders, navegación y ambiente reales. |
| `menu.css` | Estilos base + ambiente (equivale a `MainMenu.uss`). |
| `variants.css` | Los 3 temas como tokens (equivale a los `Theme.*.uss`). |
| `menu.js` | Lógica: navegación, sliders, segment, modal, transición, embers, auto-escalado del stage. |

Para inspeccionar valores exactos, abre `menu.css` (medidas/estados) y `variants.css` (colores por tema). Todo está parametrizado con `var(--*)`, así que los nombres de token coinciden con §6.

---

*Cualquier duda de interpretación, los `.png` de `screenshots/` son la fuente de verdad visual; los `.css` son la fuente de verdad de medidas y colores.*
