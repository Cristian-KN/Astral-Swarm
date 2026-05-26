# 🌌 Sistema de Backgrounds Procedurales - Astral Swarm

## Descripción General

Sistema completo de generación procedural de fondos espaciales para juegos top-down estilo Vampire Survivors. Incluye múltiples estilos y soporte para parallax multi-capa.

---

## 📦 Scripts Incluidos

### 1. **WorldBackground.cs** (Ya existente - Mejorado)
Sistema de tiling infinito que sigue la cámara. Genera automáticamente un fondo procedural si no se asigna sprite.

**Mejoras:**
- Fallback procedural mejorado con Perlin noise
- Textura de 64x64 con estrellas aleatorias
- Estilo Deep Space por defecto

### 2. **SpaceBackgroundGenerator.cs** (NUEVO)
Generador de texturas procedurales con 5 estilos diferentes.

#### Estilos Disponibles:
- **DeepSpace**: Espacio profundo con estrellas dispersas (por defecto)
- **Nebula**: Nebulosa colorida con gas y polvo estelar
- **StarField**: Campo de estrellas denso y brillante
- **VoidPurple**: Vacío morado oscuro (estilo Vampire Survivors)
- **BinaryStars**: Sistema estelar binario con resplandor

#### Parámetros Ajustables:
- `textureSize`: Resolución de la textura (32-256)
- `seed`: Semilla para reproducibilidad
- `primaryColor`: Color base oscuro
- `secondaryColor`: Color secundario para variación
- `accentColor`: Color de acentos (nebulosas, resplandores)
- `starDensity`: Densidad de estrellas (0.0 - 1.0)
- `nebulaDensity`: Intensidad de nebulosa (0.0 - 1.0)

### 3. **ParallaxBackground.cs** (NUEVO)
Sistema de múltiples capas con efecto parallax.

**Características:**
- Capas independientes que se mueven a diferentes velocidades
- Generación automática de capas
- Factor de parallax ajustable por capa (0 = estático, 1 = sigue cámara)

---

## 🎮 Uso en Unity

### Opción 1: Fondo Simple (Tiling Infinito)

1. Crear GameObject vacío: `"Background"`
2. Agregar componente `WorldBackground`
3. Configurar:
   - `Tile Sprite`: (opcional, dejar vacío para procedural)
   - `Tiles X/Y`: 9x9 (ajustar según necesidad)
   - `Tile Size`: 2.0

**Resultado:** Fondo infinito que sigue la cámara con textura procedural.

---

### Opción 2: Fondo Procedural Personalizado

1. Crear GameObject: `"SpaceBackground"`
2. Agregar componente `SpriteRenderer`
3. Agregar componente `SpaceBackgroundGenerator`
4. Configurar:
   - `Style`: Elegir entre DeepSpace, Nebula, StarField, VoidPurple, BinaryStars
   - `Texture Size`: 128 (balance calidad/performance)
   - `Seed`: 0 para aleatorio, número específico para reproducible
   - Ajustar colores y densidades
5. Ejecutar: Click derecho en componente → "Regenerate Background"

**Sorting Order:** -10 o menor para que esté detrás del juego.

---

### Opción 3: Parallax Multi-Capa (Recomendado)

1. Crear GameObject vacío: `"ParallaxSystem"`
2. Agregar componente `ParallaxBackground`
3. Configurar:
   - `Auto Generate Layers`: ✓ (activado)
   - `Number Of Layers`: 3-5
4. Dar Play → se generan capas automáticamente

#### Configuración Manual de Capas:
Si desactivas `autoGenerateLayers`, puedes crear capas manualmente:

```
ParallaxSystem/
├─ Layer_0 (Factor: 0.2) - Estrellas muy lejanas
├─ Layer_1 (Factor: 0.5) - Nebulosa intermedia
└─ Layer_2 (Factor: 0.8) - Estrellas cercanas
```

Cada capa debe tener:
- `SpriteRenderer` con `sortingOrder` decreciente (-20, -21, -22...)
- `SpaceBackgroundGenerator` con diferente estilo/seed

---

## 🎨 Presets Recomendados

### Preset 1: "Deep Void" (Minimalista)
```
Style: VoidPurple
Primary: RGB(0.05, 0.03, 0.12)
Secondary: RGB(0.12, 0.06, 0.18)
Star Density: 0.015
```

### Preset 2: "Nebula Field" (Colorido)
```
Style: Nebula
Primary: RGB(0.08, 0.05, 0.15)
Secondary: RGB(0.15, 0.10, 0.25)
Accent: RGB(0.6, 0.3, 0.9)
Nebula Density: 0.5
Star Density: 0.02
```

### Preset 3: "Dense Starfield" (Caótico)
```
Style: StarField
Primary: RGB(0.02, 0.02, 0.05)
Star Density: 0.08
```

### Preset 4: "Astral Core" (Sistema Binario)
```
Style: BinaryStars
Primary: RGB(0.05, 0.08, 0.15)
Accent: RGB(0.8, 0.5, 0.3)
Star Density: 0.02
```

---

## ⚙️ Integración con WorldBackground

Para usar texturas procedurales con el sistema de tiling:

1. Generar textura con `SpaceBackgroundGenerator`
2. Guardar el sprite generado como asset (click derecho → Export)
3. Asignar a `WorldBackground.tileSprite`

**O** simplemente dejar `tileSprite` vacío y usar el fallback mejorado.

---

## 🔧 Optimización

### Performance:
- **Texture Size 64-128**: Óptimo para pixel art
- **Texture Size 256+**: Solo si necesitas más detalle
- Usar `FilterMode.Point` para mantener estilo pixel art
- Activar `TextureWrapMode.Repeat` para tiling seamless

### Memory:
- Cada textura de 128x128 RGBA = ~64KB
- 3 capas de parallax = ~200KB total (despreciable)

---

## 🎯 Casos de Uso por Nivel

Puedes cambiar el fondo según el nivel/bioma:

```csharp
// Ejemplo: Cambiar estilo según nivel
public void SetLevelBackground(int level)
{
    BackgroundStyle style = level switch
    {
        1 => BackgroundStyle.DeepSpace,
        2 => BackgroundStyle.Nebula,
        3 => BackgroundStyle.VoidPurple,
        4 => BackgroundStyle.BinaryStars,
        _ => BackgroundStyle.StarField
    };
    
    var generator = GetComponent<SpaceBackgroundGenerator>();
    generator.style = style;
    generator.GenerateBackground();
}
```

---

## 📝 Notas Adicionales

- **Seed 0**: Genera semilla aleatoria cada vez
- **Seed fijo**: Reproducible para testing
- **Context Menu**: Click derecho en el componente en editor para regenerar
- **OnValidate**: Los cambios en el inspector regeneran automáticamente (solo en Play mode)

---

## 🐛 Troubleshooting

**Problema:** Fondo no se ve
- Verificar Sorting Order (debe ser negativo)
- Verificar que la cámara tenga tag "MainCamera"

**Problema:** Textura pixelada
- `FilterMode` debe ser `Point` para pixel art
- Aumentar `textureSize` si se ve muy blocky

**Problema:** No se ve parallax
- Verificar que `parallaxFactor` sea < 1.0
- Asegurar que la cámara se esté moviendo

---

## 🚀 Próximas Mejoras Posibles

- [ ] Animación de estrellas parpadeantes
- [ ] Meteoritos que cruzan la pantalla
- [ ] Auroras espaciales animadas
- [ ] Sistema de partículas para polvo estelar
- [ ] Shader custom para efectos de resplandor

---

**Creado por:** Claude Code
**Versión:** 1.0
**Fecha:** Mayo 2026
