# Astral Swarm — UI Redesign Design Spec
Date: 2026-05-28

## Overview

Complete redesign of all UI screens using Unity Legacy UI (uGUI). Visual aesthetic: fantasy/mystical — dark backgrounds, gold/purple accents, ornamental borders, rarity-color borders on item cards.

## Scope

1. HUD in-game (vida, nivel, oro, XP, timer)
2. Level Up overlay (3 cartas)
3. Tienda modal
4. Menú Principal
5. Menú Pausa (reutiliza Menú Principal)

---

## Architecture

### Scripts modificados
- `UIManager.cs` — añadir referencias a nuevos elementos HUD (healthText, goldText) y métodos `UpdateHealth`, `UpdateGold`, `ShowShop`, `HideShop`
- `PlayerStats.cs` — llamar `uiManager.UpdateHealth(currentHealth, maxHealth)` en `TakeDamage()` y en `Start()`
- `GameManager.cs` — llamar `uiManager.UpdateGold()` cuando cambie el oro. **Eliminar el bloque ESC de `Update()`** — la pausa la gestiona `PauseManager`
- `PauseManager.cs` — añadir referencias a `settingsPanel` con slider de volumen y dropdown de modo ventana, igual que el menú principal. Añadir `isPauseMenu = true` implícitamente (siempre es pausa). Los textos de botones ("CONTINUAR", "MENÚ PRINCIPAL") se configuran directamente en el prefab del Canvas, no por flag de código.
- `MainMenuManager.cs` — sin cambios en lógica; solo diseño visual del Canvas

### Scripts nuevos
- `ShopUI.cs` — gestiona la generación dinámica de botones de ítem en la tienda, conectado a `ShopManager`
- `CardHoverEffect.cs` — pequeño MonoBehaviour que aplica escala 1.05 y outline color-rareza en OnPointerEnter/Exit (EventTrigger o IPointerEnterHandler)

### Conflicto ESC resuelto
`GameManager.Update()` maneja ESC actualmente. `PauseManager.Update()` también lo hace. **Solución:** eliminar el bloque ESC de `GameManager.Update()` y dejarlo únicamente en `PauseManager`. `PauseManager.TogglePause()` llama a `GameManager.PauseGame()` / `GameManager.ResumeGame()` directamente.

### Prefabs / Canvas
No se crean prefabs nuevos desde código. Todo se configura en el Editor de Unity.

---

## HUD In-Game

**Posición:** Canvas Screen Space - Overlay

**Panel Stats** (arriba-izquierda):
- Fondo: Image negro 60% opacidad, borde Image dorado (1-2px)
- ❤ `healthText` — formato `"85 / 100"`, icono sprite corazón rojo
- ⭐ `levelText` — formato `"LVL 3"`, color dorado
- 💰 `goldText` — formato `"450"`, color amarillo
- Barra XP: Slider sin interacción, fill degradado azul→púrpura, texto `"250 / 500 XP"` superpuesto

**Panel Timer** (arriba-centro):
- Texto `"02:45"` grande, color blanco/dorado

---

## Level Up Overlay

**Activación:** `GameManager.LevelUp()` → `uiManager.ShowLevelUpChoices()`

**Estructura:**
- Overlay: Panel negro 70% opacidad, bloquea input al juego
- Panel central: fondo morado oscuro `#1A0A2E`, borde dorado ornamentado
- Título: `"¡NIVEL ALCANZADO!"` — fuente grande, color dorado, animación DOTween pulse (escala 1→1.05→1, loop)
- 3 cartas horizontales (HorizontalLayoutGroup):
  - Fondo `#2D1B4E`, borde color según rareza:
    - Common: `#9E9E9E`
    - Rare: `#2196F3`
    - Epic: `#9C27B0`
    - Legendary: `#FF9800`
    - Mythic: `#F44336`
  - Icono (Image, 64×64px) centrado arriba
  - Nombre (Text, dorado `#FFD700`)
  - Descripción (Text, blanco pequeño)
  - Hover: EventTrigger → scale 1.05, Image outline del color de rareza
  - Click: selecciona ítem, cierra overlay, reanuda juego

---

## Tienda Modal

**Activación:** llamada externa a `UIManager.ShowShop()` / botón X → `UIManager.HideShop()`

**Estructura:**
- Fondo oscuro semitransparente (Panel negro 70%)
- Panel central `#1A0A2E`, borde dorado
- Header: texto `"TIENDA"` dorado + botón X (arriba-derecha) — llama `HideShop()`
- Grid de ítems: GridLayoutGroup, hasta 6 slots, cell size ~120×150px
  - Cada slot: botón con Image fondo `#2D1B4E`, borde color rareza
  - Icono (Image, 48×48), nombre (Text dorado), precio (Text `"💰 120"`)
  - Si `currentGold < precio`: borde rojo, precio texto rojo, `button.interactable = false`
- `ShopUI.cs` genera los botones al abrir la tienda basándose en `ShopManager.availableItems`

---

## Menú Principal

**Escena:** `MainMenu`

**Canvas:** Screen Space - Overlay

**Fondo:** Image que ocupa toda la pantalla (color `#0A0014` o sprite de fondo espacial)

**Panel Central** (`mainPanel`):
- Fondo `#1A0A2E`, borde dorado ornamentado
- Título `"ASTRAL SWARM"`: fuente grande, color `#FFD700`, efecto Shadow o Outline
- Botón `"JUGAR"` → `MainMenuManager.PlayGame()`
- Botón `"AJUSTES"` → muestra `settingsPanel` con transición scale
- Botón `"SALIR"` → `MainMenuManager.QuitGame()`

**Panel Ajustes** (`settingsPanel`):
- Slider volumen maestro (0–1), label `"VOLUMEN"`
- Dropdown modo ventana: opciones `["Ventana", "Pantalla Completa", "Sin Bordes"]` → llama `SetWindowed / SetFullscreen / SetBorderless`
- Botón `"ATRÁS"` → vuelve a `mainPanel`

---

## Menú Pausa

**Activación:** tecla ESC → `PauseManager.TogglePause()` (ESC ya no se gestiona en `GameManager`)

Gestionado por `PauseManager.cs` que ya existe en la escena Game. Se amplía con `settingsPanel`.

**Diferencias respecto al Menú Principal (configuradas en el Canvas, no en código):**
- Botón "CONTINUAR" → `PauseManager.ResumeGame()` → `GameManager.ResumeGame()`
- Botón "MENÚ PRINCIPAL" → `PauseManager.QuitToMainMenu()`
- Botón "AJUSTES" → `PauseManager.OpenSettings()` (ya existe)
- Fondo del Canvas: Panel negro 50% opacidad (semitransparente, juego visible detrás)
- `settingsPanel` contiene slider de volumen + dropdown modo ventana, igual que el menú principal

**`PauseManager`** ya maneja ESC para abrir/cerrar settings o toggle pausa. Sin cambios de lógica, solo añadir las referencias UI de settings.

---

## Scripts — Cambios concretos

### UIManager.cs
Añadir:
```csharp
[Header("HUD — Nuevos")]
public Text healthText;
public Text goldText;
public GameObject shopPanel;

public void UpdateHealth(float current, float max) => healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
public void UpdateGold(int gold) => goldText.text = gold.ToString();
public void ShowShop(bool show) => shopPanel.SetActive(show);
```

### PlayerStats.cs
Conectar con UIManager para actualizar vida:
```csharp
private UIManager uiManager;
void Start() { uiManager = FindObjectOfType<UIManager>(); uiManager?.UpdateHealth(currentHealth, maxHealth); }
// En TakeDamage() y en cualquier Heal: uiManager?.UpdateHealth(currentHealth, maxHealth);
```

### GameManager.cs
- Llamar `uiManager.UpdateGold(currentGold)` en `AddGold()`
- **Eliminar bloque ESC de `Update()`** (lo gestiona `PauseManager`)
- En `ResumeGame()`, eliminar `ShowPauseMenu(false)` (lo gestiona `PauseManager`)

### PauseManager.cs
- Añadir referencias `masterVolumeSlider`, `windowModeDropdown`
- Conectar `Dropdown.onValueChanged` → `SetWindowed / SetFullscreen / SetBorderless` (mismo código que `MainMenuManager`)
- `TogglePause()` llama `gameManager.PauseGame()` / `gameManager.ResumeGame()`

### ShopUI.cs (nuevo)
```csharp
// Genera botones de ítem dinámicamente a partir de ShopManager.availableItems
// Cada botón: icono + nombre + precio, llama ShopManager.BuyItem(item)
// Actualiza color de borde y estado interactable según gameManager.currentGold
```

### CardHoverEffect.cs (nuevo)
```csharp
// IPointerEnterHandler / IPointerExitHandler
// OnEnter: transform.localScale = Vector3.one * 1.05f
// OnExit: transform.localScale = Vector3.one
// Color del outline: asignado desde UIManager al generar la carta según rareza
```

---

## Colores de referencia

| Elemento | Color |
|---|---|
| Fondo panel | `#1A0A2E` |
| Fondo carta | `#2D1B4E` |
| Borde dorado | `#FFD700` |
| Texto dorado | `#FFD700` |
| Oro/dinero | `#FFC107` |
| Vida | `#F44336` (rojo) |
| XP fill start | `#1565C0` (azul) |
| XP fill end | `#7B1FA2` (púrpura) |
| Rareza Common | `#9E9E9E` |
| Rareza Rare | `#2196F3` |
| Rareza Epic | `#9C27B0` |
| Rareza Legendary | `#FF9800` |
| Rareza Mythic | `#F44336` |

---

## Out of scope

- Animaciones DOTween (se usan solo si el paquete está disponible; si no, sin animación)
- Fuentes custom (se usa la fuente por defecto de Unity; reemplazable posteriormente)
- Audio en botones
- Localización
