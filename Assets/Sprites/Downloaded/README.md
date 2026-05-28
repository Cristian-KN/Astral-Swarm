# Assets Descargados - Astral Swarm

## 📦 Packs Instalados

### 1. Tiny Swords
- **Propósito:** 🎮 MAIN - Tileset/sprites base del juego
- **URL:** https://pixelfrog-assets.itch.io/tiny-swords
- **Carpeta:** `TinySwords/`

### 2. 7Soul RPG Graphics Pack
- **Propósito:** ⚔️ ITEMS - Sistema de rarezas (añadir bordes de color)
- **URL:** https://7soul.itch.io/7souls-rpg-graphics-pack-1-icons
- **Carpeta:** `Items_7Soul/`
- **Nota:** 🔔 FUTURO: Añadir bordes según rareza (común/raro/épico/legendario)

### 2. Shikashi Fantasy Icons
- **Propósito:** 🛡️ UI + ITEMS - Stats, weapons, misc
- **URL:** https://shikashipx.itch.io/shikashis-fantasy-icons-pack
- **Carpeta:** `Icons_Shikashi/`

### 3. Complete UI Essential Pack
- **Propósito:** 🎨 UI BASE - Elementos de interfaz (backup)
- **URL:** https://crusenho.itch.io/complete-ui-essential-pack
- **Carpeta:** `UI_Complete/`

---

## 🎨 Sistema de Rarezas (Futuro)

Los iconos de ítems del pack **7Soul RPG Graphics** necesitarán bordes de color:

- 🟢 **Común** - Verde
- 🔵 **Raro** - Azul
- 🟣 **Épico** - Morado
- 🟠 **Legendario** - Naranja/Dorado

Esto se implementará mediante script de Python que procese los sprites.

---

## 📁 Estructura Recomendada

```
Assets/Sprites/
├─ Downloaded/
│  ├─ TinySwords/          (Main game assets)
│  ├─ Items_7Soul/         (Item icons con rarezas)
│  ├─ Icons_Shikashi/      (UI + stats icons)
│  └─ UI_Complete/         (UI elements backup)
├─ Terrain/                (Generated terrain)
└─ Enemies/                (Enemy sprites)
```

---

**Generado automáticamente por:** `download_game_assets.py`
