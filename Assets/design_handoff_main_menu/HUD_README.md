# HUD In-Game · Grimorio Theme

Interfaz de juego estilo **Binding of Isaac** con el tema visual **Grimorio** (cuero quemado, pergamino y cera carmesí).

---

## 📦 Archivos incluidos

```
design_handoff_main_menu/
├── design_reference/
│   ├── hud_ingame.html        ← Referencia visual HTML (abre en navegador)
│   └── hud_ingame.css         ← Estilos de referencia
├── HUD_InGame.uxml            ← Estructura UI Toolkit
├── HUD_InGame.uss             ← Estilos Unity
├── HUDController.cs           ← Script controlador
└── HUD_README.md              ← Este archivo
```

---

## 🎨 Diseño

### Layout
- **Resolución nativa**: 1408 × 768 px (igual que el menú principal)
- **Escalado**: Automático con letterbox (igual que el menú)

### Elementos

#### 1. Stats superiores (esquinas)
**Izquierda**:
- ♥ Vida (actual / máximo)
- ⚔ Daño
- ➤ Velocidad

**Derecha**:
- ◎ Rango
- 💧 Cadencia (Tears)
- ☘ Suerte

#### 2. Timer (centro superior)
- ⌛ Tiempo transcurrido en formato `MM:SS`

#### 3. Contenedor de armas (centro inferior)
- **3 slots horizontales**
- Cada slot muestra:
  - Icono del arma (56×56 px)
  - Nivel actual (`Nv X`)
- El slot activo tiene borde rojo y brilla
- Slots vacíos muestran `?` y nivel `--`

---

## 🛠️ Integración en Unity

### Paso 1: Importar fuentes
Ya deberías tenerlas del menú principal, pero si no:

1. Descarga de Google Fonts:
   - [Jersey 25](https://fonts.google.com/specimen/Jersey+25) (UI)
   - [Silkscreen](https://fonts.google.com/specimen/Silkscreen) (Pixel)

2. Coloca los `.ttf` en `Assets/Fonts/`

3. Crea Font Assets (TextMeshPro):
   - `Window → TextMeshPro → Font Asset Creator`
   - Sampling: 90, Atlas: 1024×1024
   - Character Set: ASCII + `¿¡áéíóúñ`

### Paso 2: Configurar UI Document

1. **Crear GameObject HUD**:
   ```
   Jerarquía → Click derecho → UI Toolkit → UI Document
   Nombre: "HUD_InGame"
   ```

2. **Asignar archivos**:
   - En el Inspector del `UI Document`:
     - `Source Asset`: Arrastra `HUD_InGame.uxml`
   - La `Panel Settings` usa la misma que el menú principal (1408×768)

3. **Añadir el script**:
   - `Add Component → HUDController`

### Paso 3: Ajustar rutas de fuentes en USS

Abre [HUD_InGame.uss](HUD_InGame.uss) y actualiza las rutas:

```css
-unity-font-definition: url('project://database/Assets/Fonts/Jersey25-Regular.ttf');
```

Cambia `Assets/Fonts/` por la ruta real donde están tus fuentes.

### Paso 4: Asignar sprites

En el Inspector del `HUDController`:

#### Stat Icons (28×28 px recomendado)
- Health Icon
- Damage Icon
- Speed Icon
- Range Icon
- Tears Icon
- Luck Icon
- Timer Icon

#### Weapon Sprites (56×56 px)
- `Weapon Sprites`: Array con tus sprites de armas
- `Empty Slot Sprite`: Sprite para slots vacíos (ej: signo de interrogación)

---

## 📝 Uso del script

### Actualizar stats del jugador

```csharp
HUDController hud = FindObjectOfType<HUDController>();

// Stats
hud.UpdateHealth(8, 12);      // Vida: 8/12
hud.UpdateDamage(3.5f);        // Daño: 3.5
hud.UpdateSpeed(1.2f);         // Velocidad: 1.2
hud.UpdateRange(6.5f);         // Rango: 6.5
hud.UpdateTears(2.1f);         // Cadencia: 2.1
hud.UpdateLuck(0);             // Suerte: 0

// Timer
hud.UpdateTimer(Time.time);    // Actualiza el cronómetro
```

### Gestionar armas

```csharp
// Equipar arma en slot 0, nivel 3
hud.SetWeaponSlot(0, swordSprite, 3);

// Equipar arma en slot 1, nivel 1
hud.SetWeaponSlot(1, bowSprite, 1);

// Vaciar slot 2
hud.ClearWeaponSlot(2);

// Actualizar nivel de un arma
hud.UpdateWeaponLevel(0, 4);   // Sword ahora es nivel 4

// Cambiar arma activa (resaltado visual)
hud.SetActiveWeapon(1);        // Activa el slot 1 (bow)

// Obtener arma activa actual
int activeSlot = hud.GetActiveWeapon();
```

### Testing rápido

El script incluye input de prueba:
- **Teclas 1, 2, 3**: Cambia el arma activa visualmente

---

## 🎨 Paleta de colores (Grimorio)

```css
--accent: #d24b3a;                  /* Rojo carmesí */
--accent-glow: rgba(200,50,35,.6);  /* Brillo rojo */
--bg-leather: #4a3526 → #2c1d13;    /* Degradado cuero quemado */
--bg-parchment: #e9d4a4 → #d2b783;  /* Degradado pergamino */
--border-dark: #3a2414;             /* Bordes oscuros */
--text-light: #ecdcb6;              /* Texto claro */
--text-dark: #5a3a1f;               /* Texto oscuro (sobre pergamino) */
```

---

## 🔧 Personalización

### Cambiar número de slots de armas

1. En `HUD_InGame.uxml`: duplica/elimina bloques `<ui:VisualElement name="weapon-slot-X">`
2. En `HUDController.cs`: ajusta el tamaño del array `weaponSlots[]`
3. Actualiza los loops en `InitializeUI()`

### Añadir más stats

1. En `HUD_InGame.uxml`: añade otro `stat-item`
2. En `HUD_InGame.uss`: opcional, define clase custom si necesitas estilo especial
3. En `HUDController.cs`: añade `Label` y método `UpdateXXX()`

### Cambiar posición del weapon container

En `HUD_InGame.uss`:

```css
.weapon-container {
    bottom: 30px;     /* Distancia desde abajo */
    left: 50%;        /* Centrado horizontal */
    translate: -50% 0;
}
```

Cambia `bottom` por `top` para moverlo arriba, o ajusta `left` para posicionarlo a un lado.

---

## 🖼️ Referencia visual

Abre [hud_ingame.html](design_reference/hud_ingame.html) en tu navegador para ver el diseño interactivo:

- **Hover**: Los stats y slots reaccionan al pasar el ratón
- **Teclas 1-2-3**: Cambia el arma activa
- **Auto**: La vida cambia cada 3 segundos (demo)

---

## 🎯 Fidelidad visual

**ALTA (hi-fi)**: Los colores, tamaños y espaciados son **finales**. Reprodúcelos exactamente.

### Medidas clave (px)

| Elemento | Valor |
|----------|-------|
| Stat item | 110 × 40 (mín) |
| Stat icon | 28 × 28 |
| Weapon slot | 80 × auto |
| Weapon icon | 56 × 56 |
| Weapon container padding | 14px 18px |
| Gap entre slots | 14px |
| Bordes stats | 3px |
| Bordes armas | 4px (container), 3px (slots) |

### Fuentes

| Elemento | Familia | Tamaño | Peso |
|----------|---------|--------|------|
| Stat value | Jersey 25 | 22px | 600 |
| Timer | Jersey 25 | 28px | 600 |
| Level label | Silkscreen | 11px | 400 |
| Level value | Silkscreen | 18px | 700 |

---

## 🐛 Troubleshooting

### Las fuentes no se ven
- Verifica que las rutas en `.uss` apunten a tus Font Assets
- Usa rutas relativas: `url('project://database/Assets/Fonts/...')`

### Los iconos no aparecen
- Asegúrate de asignar los sprites en el Inspector del `HUDController`
- Los sprites deben tener `Texture Type = Sprite (2D and UI)`

### El HUD no escala bien
- Verifica que la `Panel Settings` tenga:
  - `Scale Mode = Scale With Screen Size`
  - `Reference Resolution = 1408 × 768`
  - `Match = 0.5`

### Los bordes se ven borrosos
- En cada sprite, marca:
  - `Filter Mode = Point (no filter)`
  - `Compression = None`
- Añade a `.uss` (aunque USS no lo soporta nativamente, Unity lo infiere del sprite)

---

## 📚 Compatibilidad

- **Unity**: 6 (6000.x) o superior
- **UI System**: UI Toolkit (UXML/USS)
- **Render Pipeline**: Compatible con URP/HDRP/Built-in

---

## 🎮 Integración con tu sistema de juego

### Ejemplo: Actualizar HUD desde PlayerController

```csharp
public class PlayerController : MonoBehaviour
{
    private HUDController hud;
    
    [SerializeField] private float health = 12f;
    [SerializeField] private float maxHealth = 12f;
    [SerializeField] private float damage = 3.5f;
    
    void Start()
    {
        hud = FindObjectOfType<HUDController>();
        UpdateHUD();
    }
    
    void UpdateHUD()
    {
        hud.UpdateHealth((int)health, (int)maxHealth);
        hud.UpdateDamage(damage);
        // ... resto de stats
    }
    
    public void TakeDamage(float amount)
    {
        health -= amount;
        UpdateHUD();
    }
}
```

### Ejemplo: Sistema de armas

```csharp
public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public Sprite icon;
        public int level = 1;
    }
    
    [SerializeField] private Weapon[] weapons = new Weapon[3];
    private HUDController hud;
    private int currentWeapon = 0;
    
    void Start()
    {
        hud = FindObjectOfType<HUDController>();
        
        // Inicializar slots
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].icon != null)
                hud.SetWeaponSlot(i, weapons[i].icon, weapons[i].level);
            else
                hud.ClearWeaponSlot(i);
        }
        
        hud.SetActiveWeapon(currentWeapon);
    }
    
    public void EquipWeapon(int slot, Weapon weapon)
    {
        weapons[slot] = weapon;
        hud.SetWeaponSlot(slot, weapon.icon, weapon.level);
    }
    
    public void LevelUpWeapon(int slot)
    {
        if (weapons[slot] != null)
        {
            weapons[slot].level++;
            hud.UpdateWeaponLevel(slot, weapons[slot].level);
        }
    }
    
    void Update()
    {
        // Cambiar arma con scroll del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
            SwitchWeapon(1);
        else if (scroll < 0)
            SwitchWeapon(-1);
    }
    
    void SwitchWeapon(int direction)
    {
        currentWeapon = (currentWeapon + direction + 3) % 3;
        hud.SetActiveWeapon(currentWeapon);
    }
}
```

---

## ✅ Checklist de integración

- [ ] Importar fuentes Jersey 25 y Silkscreen
- [ ] Crear Font Assets en TextMeshPro
- [ ] Crear GameObject con UI Document
- [ ] Asignar `HUD_InGame.uxml` al UI Document
- [ ] Añadir script `HUDController`
- [ ] Actualizar rutas de fuentes en `.uss`
- [ ] Crear sprites de iconos (28×28 px, Point filter)
- [ ] Asignar sprites en el Inspector
- [ ] Conectar con tu PlayerController/WeaponManager
- [ ] Testear con teclas 1-2-3

---

¡Listo! 🎉 Ahora tienes un HUD funcional con el estilo Grimorio.
