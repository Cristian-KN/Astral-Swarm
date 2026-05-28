# 🌍 Sistema de Biomas Dinámicos - Astral Swarm

## Descripción General

Sistema de biomas rotativos que cambian cada cierto tiempo (3 minutos por defecto), afectando la dificultad, recompensas y apariencia visual del juego. Incluye un bioma especial con 20% de probabilidad que ofrece mayores recompensas pero enemigos más fuertes.

---

## 📋 Tipos de Biomas

### 🟢 Biomas Normales (Rotan cada 3 minutos)

| # | Nombre | Color | Dificultad | EXP | Oro | Suerte | Tier Mín |
|---|--------|-------|-----------|-----|-----|--------|----------|
| 1 | **Vacío Espacial** | Azul oscuro | 1.0x | 1.0x | 1.0x | 0% | Normal |
| 2 | **Nebulosa Carmesí** | Rojo | 1.2x | 1.0x | 1.0x | 0% | Normal |
| 3 | **Vacío Helado** | Azul claro | 1.1x | 1.0x | 1.0x | 0% | Normal |
| 4 | **Nube Tóxica** | Verde | 1.3x | 1.1x | 1.0x | 0% | Verde+ |
| 5 | **Tormenta Eléctrica** | Amarillo | 1.4x | 1.1x | 1.1x | +5% | Verde+ |
| 6 | **Abismo Profundo** | Morado oscuro | 1.5x | 1.2x | 1.1x | 0% | Verde+ |
| 7 | **Llamarada Solar** | Naranja | 1.6x | 1.2x | 1.2x | +5% | Amarilla+ |
| 8 | **Grieta Cósmica** | Multicolor | 1.7x | 1.3x | 1.2x | +10% | Amarilla+ |
| 9 | **Materia Oscura** | Gris/Negro | 1.8x | 1.3x | 1.3x | 0% | Amarilla+ |
| 10 | **Borde del Vacío** | Morado brillante | 2.0x | 1.5x | 1.4x | +15% | Azul+ |

### ⭐ Bioma Especial (20% probabilidad)

**Anomalía Dorada** ✨
- **Color:** Dorado brillante
- **Dificultad:** 2.5x (¡MUY difícil en early game!)
- **EXP:** 2.0x (+100%)
- **Oro:** 2.5x (+150%)
- **Suerte:** +25%
- **Tier mínimo:** Verde+ (todos los enemigos son al menos variante verde)

**Mecánica especial:** Alta recompensa, alto riesgo. En early game es brutal, pero en late game es la mejor oportunidad para farmear.

---

## 🎮 Mecánicas del Sistema

### Rotación Automática
- Cada **3 minutos** el bioma cambia automáticamente
- Secuencia: Biomas normales rotan en orden (1 → 2 → 3 → ... → 10 → 1)
- Cada cambio tiene **20% de probabilidad** de activar el bioma especial en su lugar

### Avisos
- **10 segundos antes** del cambio, se muestra una advertencia
- Flash visual en pantalla
- Sonido de alerta (configurable)

### Efectos en el Gameplay

#### 1. **Dificultad de Enemigos**
- Multiplica la dificultad base calculada (Tiempo + Nivel + Sacrificio)
- Afecta la probabilidad de variantes superiores
- Ejemplo: Bioma 2.0x hace que aparezcan más enemigos Morados/Negros/Rojos

#### 2. **Tier Mínimo de Enemigos**
- Algunos biomas fuerzan un tier mínimo
- Ejemplo: "Nube Tóxica" (tier 1) → todos los enemigos normales se upgraden a Verde
- "Anomalía Dorada" → todos los enemigos son mínimo Verde

#### 3. **Multiplicadores de Recompensas**
- **EXP:** Se aplica al recoger gemas de experiencia
- **Oro:** Se aplica al recoger monedas (cuando implementes el sistema)
- Ejemplo: En "Anomalía Dorada" una gema de 10 EXP da 20 EXP

#### 4. **Bonus de Suerte**
- Aumenta las probabilidades de drops raros
- Afecta la calidad de ítems en cofres
- Mejora las probabilidades de críticos

---

## 🛠️ Configuración en Unity

### Setup Básico (3 pasos)

#### 1. Crear el BiomeManager
```
Hierarchy:
└─ GameManager (o crear nuevo GameObject "Systems")
   └─ BiomeManager (agregar componente BiomeManager)
```

**Configuración:**
- `Biome Duration`: 180 (3 minutos)
- `Special Biome Chance`: 0.2 (20%)
- `Warning Time`: 10 (avisar 10s antes)
- `Background Generator`: Arrastrar el objeto que tenga `SpaceBackgroundGenerator`

#### 2. Configurar el Background
```
Hierarchy:
└─ Background (objeto con WorldBackground o SpaceBackgroundGenerator)
```

El `BiomeManager` automáticamente cambiará los colores del fondo.

#### 3. (Opcional) UI del Bioma
```
Canvas/
└─ BiomeDisplay (agregar componente BiomeUIDisplay)
   ├─ BiomeNameText (TextMeshPro)
   ├─ TimerText (TextMeshPro)
   ├─ ProgressBar (Image con Fill Amount)
   └─ WarningPanel (Panel que parpadea)
```

---

## 📊 Integración con Otros Sistemas

### EnemySpawner.cs ✅
Ya integrado. Los enemigos automáticamente:
- Se vuelven más fuertes según el multiplicador del bioma
- Respetan el tier mínimo del bioma
- Ajustan variantes según dificultad total

### ExperienceGem.cs ✅
Ya integrado. Las gemas dan EXP multiplicada por el bioma.

### Sistema de Oro (Pendiente)
```csharp
// En tu script de monedas/oro:
float goldAmount = baseGold;
if (BiomeManager.Instance != null)
{
    goldAmount *= BiomeManager.Instance.GetGoldMultiplier();
}
```

### Sistema de Drop/Loot (Pendiente)
```csharp
// Al calcular drops:
float luckBonus = 0f;
if (BiomeManager.Instance != null)
{
    luckBonus = BiomeManager.Instance.GetLuckBonus();
}

float dropChance = baseChance + luckBonus;
if (Random.value < dropChance) { /* drop item */ }
```

---

## 🎯 Estrategia para el Jugador

### Early Game (0-5 minutos)
- **Evitar peleas** si aparece Anomalía Dorada
- Enfocarse en sobrevivir en biomas 1-4
- Aprovechar Tormenta Eléctrica para farmear EXP

### Mid Game (5-15 minutos)
- Buscar activamente Anomalía Dorada para subir rápido
- Biomas 5-7 son el sweet spot de dificultad/recompensa
- Usar avisos de cambio para posicionarse estratégicamente

### Late Game (15+ minutos)
- Anomalía Dorada = jackpot de recompensas
- Biomas finales (8-10) mantienen la tensión alta
- "Borde del Vacío" es el test final de habilidad

---

## ⚙️ Parámetros Ajustables

### En BiomeManager:
```csharp
[SerializeField] private float biomeDuration = 180f; // Duración de cada bioma
[SerializeField] private float specialBiomeChance = 0.2f; // Probabilidad de especial
[SerializeField] private float warningTime = 10f; // Tiempo de aviso previo
```

### Para Balanceo:
Editar valores en `InitializeDefaultBiomes()`:

```csharp
CreateBiome(
    BiomeType.CustomBiome,
    "Nombre del Bioma",
    primaryColor, secondaryColor, accentColor, // Colores
    starDensity, nebulaDensity,                // Visual
    diffMultiplier,  // 1.0 = normal, 2.0 = doble dificultad
    expMultiplier,   // 1.5 = +50% EXP
    goldMultiplier,  // 2.0 = doble oro
    luckBonus,       // 0.1 = +10% suerte
    minTier          // 0=Normal, 1=Verde, 2=Amarilla, etc.
);
```

---

## 🎨 Paletas de Colores por Bioma

### Vacío Espacial
```
Primary:   RGB(13,  13,  31)   #0D0D1F
Secondary: RGB(20,  20,  46)   #14142E
Accent:    RGB(51,  51, 102)   #333366
```

### Anomalía Dorada ⭐
```
Primary:   RGB(38,  31,   5)   #261F05
Secondary: RGB(64,  51,  13)   #40330D
Accent:    RGB(242, 204, 77)   #F2CC4D
```

### Nebulosa Carmesí
```
Primary:   RGB(38,   5,  13)   #26050D
Secondary: RGB(64,  13,  26)   #400D1A
Accent:    RGB(204, 51,  77)   #CC334D
```

(Ver archivo `BiomeManager.cs` para el resto)

---

## 🐛 Debugging

### Comandos de Test
En el editor, click derecho en `BiomeManager`:
- **Force Biome Change** → Fuerza cambio al siguiente bioma
- **Force Special Biome** → Activa la Anomalía Dorada inmediatamente

### Logs
El sistema loguea automáticamente:
- `[BiomeManager] Bioma cambiado a: X (Dif: xY, EXP: xZ...)`
- `[BiomeManager] ⭐ BIOMA ESPECIAL ACTIVADO`
- `[BiomeManager] ⚠️ El bioma cambiará en Xs`

### Verificar Integración
```csharp
// En cualquier script:
BiomeManager bm = BiomeManager.Instance;
if (bm != null)
{
    Debug.Log($"Bioma actual: {bm.GetCurrentBiome().displayName}");
    Debug.Log($"Es especial: {bm.IsSpecialBiome()}");
    Debug.Log($"Multiplicador EXP: {bm.GetExpMultiplier()}");
}
```

---

## 🚀 Extensiones Futuras

### Ideas para Expandir:
- [ ] **Biomas Secretos** (1% probabilidad, recompensas legendarias)
- [ ] **Eventos de Bioma** (mini-boss aparece al cambiar)
- [ ] **Sinergias entre Biomas** (bonus si sobrevives X biomas seguidos)
- [ ] **Bioma Elegible** (powerup que permite elegir el siguiente bioma)
- [ ] **Mutadores de Bioma** (variantes con reglas especiales)
- [ ] **Achievements por Bioma** (sobrevive 30min en Anomalía Dorada)
- [ ] **Música Dinámica** (AudioClip por bioma)
- [ ] **Efectos de Partículas** (nieve en Vacío Helado, etc.)

---

## 📝 Notas de Diseño

### Balance Philosophy
- **Early Game:** Biomas 1-3 son tutoriales suaves
- **Mid Game:** Biomas 4-7 escalan con el jugador
- **Late Game:** Biomas 8-10 son desafíos extremos
- **Anomalía Dorada:** Siempre es riesgo/recompensa sin importar el momento

### Por qué 3 minutos
- Lo suficientemente largo para establecer estrategia
- Lo suficientemente corto para mantener variedad
- ~10 biomas en una run de 30 minutos

### Por qué 20% especial
- No tan raro que nunca lo veas
- No tan común que pierda impacto
- ~2 apariciones en una run promedio de 30min

---

**Creado por:** Claude Code  
**Versión:** 1.0  
**Fecha:** Mayo 2026  
**Dependencias:** `SpaceBackgroundGenerator.cs`, `EnemySpawner.cs`, `ExperienceGem.cs`
