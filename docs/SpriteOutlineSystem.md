# 🎨 Sistema de Sprite Outline - Astral Swarm

## Descripción General

Sistema de outlines/bordes para sprites que mejora la visibilidad de enemigos, jugador y otros elementos sobre fondos complejos. Incluye modo adaptativo que cambia automáticamente entre blanco/negro según el bioma actual.

---

## 🎯 Problema que Resuelve

Con 11 biomas de diferentes colores y estilos, los sprites pueden perderse visualmente:
- **Enemigos rojos** en Nebulosa Carmesí
- **Enemigos verdes** en Nube Tóxica
- **Enemigos oscuros** en Materia Oscura

El sistema de outline garantiza que todos los sprites sean claramente visibles sin importar el fondo.

---

## 📦 Componentes

### 1. **SpriteOutline.cs** (Script Principal)
Añade borde a cualquier SpriteRenderer.

**Características:**
- Dos métodos de renderizado: Shader (rápido) o Instancias (fallback)
- Color adaptativo según brillo del bioma
- Grosor ajustable (1-5 píxeles)
- API pública para control dinámico

### 2. **SpriteOutline.shader** (Shader Custom)
Shader optimizado para Unity que genera el outline en GPU.

**Ventajas:**
- 1 draw call por sprite (muy eficiente)
- No crea GameObjects adicionales
- Funciona con sprite atlases
- Compatible con Pixel Perfect

### 3. **AutoApplyOutline.cs** (Configuración Automática)
Aplica outlines automáticamente a enemigos, jugador y proyectiles.

---

## 🛠️ Uso Básico

### Opción 1: Manual (Para un objeto específico)

1. Seleccionar GameObject con SpriteRenderer (ej: prefab de Enemy)
2. Add Component → `SpriteOutline`
3. Configurar:
   - **Outline Color:** Negro para fondos claros, Blanco para oscuros
   - **Outline Size:** 1-2 para enemigos normales, 2-3 para jefe/jugador
   - **Adaptive Color:** ✓ (recomendado para cambios de bioma)
   - **Method:** Shader (si el shader está instalado)

### Opción 2: Automática (Para todos los enemigos)

1. Crear GameObject vacío: `"OutlineManager"`
2. Add Component → `AutoApplyOutline`
3. Configurar:
   - `Apply To Enemies`: ✓
   - `Apply To Player`: ✓
   - `Adaptive By Default`: ✓

Todos los enemigos y el jugador tendrán outline automáticamente.

---

## ⚙️ Configuración Avanzada

### Método de Renderizado

#### **Shader (Recomendado)**
- **Performance:** Excelente (1 draw call)
- **Requisitos:** Shader `Sprites/Outline` instalado en `Assets/Shaders/`
- **Ventaja:** No crea objetos extra

#### **Instances (Fallback)**
- **Performance:** Aceptable (9 draw calls por sprite)
- **Requisitos:** Ninguno
- **Ventaja:** Funciona siempre, incluso sin el shader

El sistema detecta automáticamente si el shader existe y hace fallback a instancias si no.

### Modo Adaptativo

Cuando está activado, el outline cambia de color según el bioma:

```csharp
float brightness = (R + G + B) / 3;

if (brightness < 0.25)
    outline = Blanco;  // Fondos oscuros
else
    outline = Negro;   // Fondos claros
```

**Biomas con outline blanco:**
- Vacío Espacial (oscuro)
- Materia Oscura (muy oscuro)
- Abismo Profundo (oscuro)

**Biomas con outline negro:**
- Anomalía Dorada (dorado brillante)
- Llamarada Solar (naranja brillante)
- Tormenta Eléctrica (amarillo)

---

## 🎮 Integración con Otros Sistemas

### EnemySpawner
Modificar `SpawnEnemy()` para aplicar outline:

```csharp
void SpawnEnemy()
{
    GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
    
    // Añadir outline
    AutoApplyOutline.AddOutlineIfMissing(enemy, Color.black, 1, true);
}
```

### Player Setup
En el prefab del jugador:
- Add Component → `SpriteOutline`
- Outline Size: 2 (más visible)
- Adaptive Color: ✓

### Projectiles (Opcional)
Los proyectiles normalmente no necesitan outline porque se mueven rápido, pero puedes añadirlo:

```csharp
SpriteOutline outline = projectile.AddComponent<SpriteOutline>();
outline.SetOutlineSize(1);
outline.SetAdaptive(false); // Color fijo para proyectiles
```

---

## 🎨 Configuración Recomendada por Tipo

| Tipo | Color | Tamaño | Adaptativo | Método |
|------|-------|--------|-----------|---------|
| **Jugador** | Blanco | 2-3 | ✓ | Shader |
| **Enemigos Normales** | Negro | 1 | ✓ | Shader |
| **Jefes** | Negro | 2-3 | ✓ | Shader |
| **Proyectiles** | Blanco | 1 | ✗ | Shader |
| **Gemas de EXP** | Negro | 1 | ✗ | Shader |
| **Items** | Blanco | 1 | ✓ | Shader |

---

## 🚀 Optimización

### Performance Tips

1. **Usar Shader siempre que sea posible**
   - 1 draw call vs 9 draw calls por sprite
   - Sin GameObjects adicionales

2. **Desactivar adaptativo en objetos estáticos**
   - Si un sprite nunca se mueve, usa color fijo
   - Ejemplo: decoración, props

3. **Outline Size pequeño**
   - Size 1 = Suficiente para 90% de casos
   - Size 2+ solo para elementos importantes (jugador, jefes)

### Números de Referencia

Con 50 enemigos en pantalla:
- **Con Shader:** ~50 draw calls
- **Sin Shader:** ~450 draw calls (9x más)

**Recomendación:** Usar el shader custom siempre.

---

## 🐛 Troubleshooting

### Problema: "Shader 'Sprites/Outline' not found"

**Solución:**
1. Verificar que `Assets/Shaders/SpriteOutline.shader` existe
2. Si no, copiar el shader desde este repo
3. En Unity: Assets → Reimport All
4. Si persiste, el script hará fallback a método de instancias

### Problema: Outline no se ve

**Checklist:**
- [ ] SpriteRenderer tiene alpha > 0
- [ ] Outline Size es al menos 1
- [ ] Sorting Order del sprite no está detrás del fondo
- [ ] El shader está compilando sin errores

### Problema: Outline parpadea

**Causa:** Z-fighting entre sprite y outline

**Solución:**
- Asegurar que el sprite original está en Z = 0
- El outline se renderiza en SortingOrder - 1

### Problema: Performance baja con muchos enemigos

**Solución:**
- Verificar que estás usando método Shader, no Instances
- Reducir Outline Size a 1
- Considerar desactivar outline en enemigos muy lejanos:

```csharp
float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
outline.enabled = distanceToCamera < 20f; // Solo visible si está cerca
```

---

## 📐 Detalles Técnicos del Shader

### Algoritmo

1. Para cada píxel del sprite:
   - Si el píxel tiene alpha > 0 → renderizar sprite normal
   - Si el píxel es transparente:
     - Samplear 8 vecinos (N, NE, E, SE, S, SW, W, NW)
     - Si algún vecino tiene alpha > 0 → renderizar outline
     - Si no → píxel vacío

2. El sampling se hace con `_MainTex_TexelSize` para escalar correctamente

### Parámetros del Shader

```shader
_MainTex        // Textura del sprite (automático)
_Color          // Tint del sprite (automático)
_OutlineColor   // Color del borde (configurable)
_OutlineSize    // Grosor en píxeles (configurable)
```

### Compatibilidad

- ✅ Unity 2020.3+
- ✅ Unity 2021.3+
- ✅ Unity 6 (Unity 2022+)
- ✅ URP (Universal Render Pipeline)
- ✅ Built-in Render Pipeline
- ⚠️ HDRP (requiere ajustes menores)

---

## 🎨 Variaciones de Estilo

### Outline Grueso (Cartoon Style)
```csharp
outline.SetOutlineSize(3);
outline.SetOutlineColor(Color.black);
```

### Glow Effect (Neon)
```csharp
outline.SetOutlineSize(2);
outline.SetOutlineColor(new Color(0.5f, 1f, 1f, 0.8f)); // Cian brillante
```

### Outline de Variante (Colorear por tipo)
```csharp
// En EnemyStats.cs
void InitializeEnemy()
{
    SpriteOutline outline = GetComponent<SpriteOutline>();
    if (outline != null)
    {
        Color variantOutline = variant switch
        {
            EnemyVariantType.Verde => Color.green,
            EnemyVariantType.Amarilla => Color.yellow,
            EnemyVariantType.Roja => Color.red,
            _ => Color.black
        };
        outline.SetOutlineColor(variantOutline);
        outline.SetAdaptive(false); // Color fijo
    }
}
```

---

## 🔮 Mejoras Futuras

Ideas para expandir el sistema:

- [ ] **Outline animado** (pulsar cuando el enemigo toma daño)
- [ ] **Outline de selección** (cuando el cursor está sobre el enemigo)
- [ ] **Outline de estado** (rojo = bajo vida, azul = congelado, etc.)
- [ ] **Outline de rareza** (para drops legendarios)
- [ ] **Shader de outline suave** (anti-aliasing)
- [ ] **Outline con gradiente** (fade desde el centro)

---

## 📝 Ejemplo Completo

### Setup de un Prefab de Enemigo

```
Enemy Prefab
├─ SpriteRenderer (sprite del enemigo)
├─ EnemyStats
├─ EnemyAI
├─ EnemyColorizer
└─ SpriteOutline ← NUEVO
   ├─ Outline Color: (0, 0, 0, 1) Negro
   ├─ Outline Size: 1
   ├─ Adaptive Color: ✓
   └─ Method: Shader
```

### Código de Inicialización

```csharp
// En GameManager o similar
void Start()
{
    // Aplicar outline a todos los enemigos existentes
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    foreach (GameObject enemy in enemies)
    {
        AutoApplyOutline.AddOutlineIfMissing(enemy, Color.black, 1, true);
    }

    // Aplicar al jugador
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    AutoApplyOutline.AddOutlineIfMissing(player, Color.white, 2, true);
}
```

---

## 📚 Referencias

- **Unity Shader Documentation:** [Sprite Shaders](https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html)
- **Sprite Outline Techniques:** [Brackeys Tutorial](https://www.youtube.com/watch?v=3uyolYVsioo)
- **GPU Instancing:** [Unity Manual](https://docs.unity3d.com/Manual/GPUInstancing.html)

---

**Creado por:** Claude Code  
**Versión:** 1.0  
**Fecha:** Mayo 2026  
**Dependencias:** `BiomeManager.cs` (para modo adaptativo)
