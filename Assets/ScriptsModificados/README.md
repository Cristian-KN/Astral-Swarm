# Scripts Modificados - UI Redesign (Commit 8999b12)

Esta carpeta contiene todos los scripts que tu amigo modificó o creó para el **UI Redesign** del proyecto Astral Swarm.

## 📦 Scripts NUEVOS (creados desde cero)

### 1. **CardHoverEffect.cs**
- **Qué hace:** Efecto visual cuando pasas el mouse sobre las cartas de level-up
- **Funcionalidad:** 
  - Agranda la carta 5% (escala 1.05x)
  - Muestra un borde/outline del color de la rareza
  - Usa `IPointerEnterHandler` y `IPointerExitHandler`

### 2. **ShopUI.cs**
- **Qué hace:** Sistema completo de tienda con generación dinámica de botones
- **Funcionalidad:**
  - Genera botones de ítems al abrir la tienda
  - Muestra icono, nombre y precio de cada ítem
  - Cambia color del borde según rareza (Common/Rare/Epic/Legendary/Mythic)
  - Si no tienes oro suficiente: borde rojo, precio rojo, botón desactivado
  - Calcula precios automáticamente según rareza y nivel del jugador

### 3. **MainMenuManager.cs**
- **Qué hace:** Controla el menú principal del juego
- **Funcionalidad:**
  - Botones: JUGAR, AJUSTES, SALIR
  - Panel de ajustes con:
    - Sliders de volumen (Master, Music, SFX)
    - Dropdown para cambiar modo de ventana (Ventana, Pantalla Completa, Sin Bordes)
  - Transiciones animadas entre paneles (scale 1→0→1)

### 4. **PauseManager.cs**
- **Qué hace:** Sistema de pausa del juego (tecla ESC)
- **Funcionalidad:**
  - ESC abre/cierra el menú de pausa
  - Pausa el tiempo (`Time.timeScale = 0`)
  - Botones: CONTINUAR, MENÚ PRINCIPAL, AJUSTES
  - Panel de ajustes igual que el menú principal
  - **IMPORTANTE:** Este script ahora controla ESC, se removió de GameManager

---

## 🔧 Scripts MODIFICADOS (ya existían, pero se actualizaron)

### 5. **UIManager.cs**
- **Cambios:**
  - Añadidos campos: `healthText`, `goldText`, `shopPanel`
  - Nuevos métodos:
    - `UpdateHealth(current, max)` — actualiza texto de vida
    - `UpdateGold(gold)` — actualiza texto de oro
    - `ShowShop(bool)` — muestra/oculta panel de tienda

### 6. **PlayerStats.cs**
- **Cambios:**
  - Conectado con `UIManager` para actualizar HUD de vida
  - En `Start()` y `TakeDamage()` llama a `uiManager.UpdateHealth()`

### 7. **GameManager.cs**
- **Cambios:**
  - **REMOVIDO:** bloque de código que manejaba la tecla ESC (ahora lo hace PauseManager)
  - Añadido: llamadas a `uiManager.UpdateGold()` cuando cambia el oro
  - Limpieza: eliminadas referencias a menú de pausa (ahora en PauseManager)

---

## 🎨 Paleta de Colores Usada

| Elemento | Color Hex | Uso |
|---|---|---|
| `#1A0A2E` | Morado oscuro | Fondo de paneles |
| `#2D1B4E` | Morado medio | Fondo de cartas/botones |
| `#FFD700` | Dorado | Bordes, títulos, textos importantes |
| `#FFC107` | Amarillo | Contador de oro |
| `#F44336` | Rojo | Vida, rareza Mythic |
| `#9E9E9E` | Gris | Rareza Common |
| `#2196F3` | Azul | Rareza Rare |
| `#9C27B0` | Púrpura | Rareza Epic |
| `#FF9800` | Naranja | Rareza Legendary |

---

## 📋 Para implementar en Unity:

1. **Crear Canvas** en el Editor de Unity según la especificación en `docs/superpowers/specs/2026-05-28-ui-redesign-design.md`
2. **Asignar referencias** en el Inspector:
   - UIManager: arrastra `healthText`, `goldText`, `shopPanel`
   - PauseManager: arrastra `pausePanel`, `settingsPanel`, sliders, dropdown
   - MainMenuManager: arrastra `mainPanel`, `settingsPanel`, sliders, dropdown
3. **Configurar colores** según la paleta de arriba
4. **Crear GameObjects** para los textos y paneles según la estructura del .md

---

## 🔗 Documento de Referencia

Ver especificación completa en:
`docs/superpowers/specs/2026-05-28-ui-redesign-design.md`
