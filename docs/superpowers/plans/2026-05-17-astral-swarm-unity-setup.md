# Astral Swarm — Unity Setup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hacer el loop core de Astral Swarm jugable en Unity: movimiento del jugador → disparo automático → matar enemigos → recoger XP → subir de nivel.

**Architecture:** El proyecto ya tiene 16 scripts C# completos. Este plan configura Unity (tags, layers, física), crea prefabs con sprites CC0 mínimos (Knight + Slime), monta la escena Game con jerarquía completa de Managers/Player/Camera/UI, y conecta todas las referencias del Inspector.

**Tech Stack:** Unity 6.0.3, C#, URP 2D, Unity Physics 2D, UnityEngine.UI (legacy Text), Unity Input Manager (GetAxisRaw)

---

## Archivos que se crean o modifican

| Acción | Ruta |
|--------|------|
| Modificar | `Assets/Scripts/PlayerStats.cs` — conectar Die() a GameManager |
| Modificar | `Assets/Scripts/Projectile.cs` — descomentar daño a enemigos |
| Crear | `Assets/Sprites/Player/` — frames del Knight |
| Crear | `Assets/Sprites/Enemies/` — frames del Slime |
| Crear | `Assets/Animations/PlayerAnimator.controller` |
| Crear | `Assets/Prefabs/Player.prefab` |
| Crear | `Assets/Prefabs/Enemy.prefab` |
| Crear | `Assets/Prefabs/Projectile.prefab` |
| Crear | `Assets/Prefabs/ExperienceGem.prefab` |
| Modificar | `Assets/Scenes/SampleScene.unity` → renombrar a `Game` y configurar |

---

## Task 1: Corregir PlayerStats.Die() — conectar Game Over

**Archivos:**
- Modificar: `Assets/Scripts/PlayerStats.cs:103-108`

El método `Die()` actual solo desactiva el GameObject. Necesita notificar al GameManager.

- [ ] **Paso 1: Abrir PlayerStats.cs y localizar Die()**

El método está en la línea ~103:
```csharp
private void Die()
{
    Debug.Log("¡El Hechicero ha caído! Fin de la partida.");
    gameObject.SetActive(false); // Por ahora, lo ocultamos.
}
```

- [ ] **Paso 2: Reemplazar Die() con la versión conectada**

```csharp
private void Die()
{
    GameManager gm = FindObjectOfType<GameManager>();
    if (gm != null) gm.TriggerGameOver();
    gameObject.SetActive(false);
}
```

- [ ] **Paso 3: Verificar en Unity que compila sin errores**

Abrir Unity y esperar que recompile. La consola no debe mostrar errores de compilación.

- [ ] **Paso 4: Guardar**

Guardar el archivo. Unity recompilará automáticamente.

---

## Task 2: Corregir Projectile.cs — activar daño a enemigos

**Archivos:**
- Modificar: `Assets/Scripts/Projectile.cs:44-57`

El método `OnTriggerEnter2D` tiene el daño comentado. El proyectil actualmente impacta pero no hace daño.

- [ ] **Paso 1: Abrir Projectile.cs y localizar OnTriggerEnter2D()**

```csharp
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Enemy"))
    {
        // EnemyStats enemy = collision.GetComponent<EnemyStats>();
        // if (enemy != null) enemy.TakeDamage(damage);
        Debug.Log("¡El proyectil ha impactado un enemigo!");
        Destroy(gameObject);
    }
}
```

- [ ] **Paso 2: Descomentar el daño**

```csharp
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Enemy"))
    {
        EnemyStats enemy = collision.GetComponent<EnemyStats>();
        if (enemy != null) enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
```

- [ ] **Paso 3: Verificar compilación en Unity**

Sin errores en la consola.

---

## Task 3: Configurar Tags, Layers y Physics 2D

Todo desde Unity Editor — no hay archivos de código que tocar.

- [ ] **Paso 1: Añadir Tag "Player"**

Edit → Project Settings → Tags and Layers → Tags → + → escribir `Player` → Enter.

- [ ] **Paso 2: Añadir Tag "Enemy"**

En el mismo panel → + → escribir `Enemy` → Enter.

- [ ] **Paso 3: Añadir Layer "Enemy"**

En el mismo panel → Layers → encontrar el primer slot libre (ej: User Layer 6) → escribir `Enemy`.

Anotar el número de layer (ej: 6). Se usará al asignar la LayerMask en PlayerAttack.

- [ ] **Paso 4: Desactivar colisión Enemy vs Enemy en Physics 2D**

Edit → Project Settings → Physics 2D → Layer Collision Matrix.
Encontrar la intersección de la fila "Enemy" con la columna "Enemy" → desmarcar la casilla.

Esto hace que los enemigos se ignoren entre sí (estilo Vampire Survivors).

- [ ] **Paso 5: Verificar**

Cerrar Project Settings. Debe guardarse automáticamente.

---

## Task 4: Descargar e Importar Sprites CC0

Los assets se descargan manualmente desde OpenGameArt. Unity importa cualquier imagen PNG que se ponga en la carpeta Assets.

- [ ] **Paso 1: Descargar sprite del jugador (Knight)**

Ir a: `https://opengameart.org/content/lpc-medieval-fantasy-character-sprites`

Descargar el ZIP. Extraer y buscar un PNG de sprite sheet del personaje caballero o mago con animaciones de walk/idle.

- [ ] **Paso 2: Descargar sprite del enemigo (Slime)**

Ir a: `https://opengameart.org/content/lpc-monsters`

Descargar el ZIP. Extraer y buscar el PNG del Slime.

- [ ] **Paso 3: Crear carpetas en Unity**

En la ventana Project de Unity, dentro de Assets:
- Clic derecho → Create → Folder → `Sprites`
- Dentro de Sprites: crear subcarpeta `Player`
- Dentro de Sprites: crear subcarpeta `Enemies`

- [ ] **Paso 4: Importar el sprite del jugador**

Arrastrar el PNG del Knight a `Assets/Sprites/Player/`.

Seleccionar el PNG importado en el Project → en el Inspector:
- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Multiple
- **Pixels Per Unit:** 16 (para LPC 64px; ajustar si el sprite queda demasiado grande o pequeño en escena)
- **Filter Mode:** Point (No Filter)
- **Compression:** None
- Clic en **Apply**.

- [ ] **Paso 5: Cortar el sprite sheet del jugador**

Con el PNG seleccionado en Inspector → clic en **Sprite Editor** (botón).

En Sprite Editor → Slice → Type: **Grid By Cell Size** → Cell Size: **64 x 64** (ajustar según el sheet) → Slice → Apply. Cerrar Sprite Editor.

Ahora el PNG contiene múltiples sprites numerados (ej: `knight_0`, `knight_1`, …).

Anotar qué frames corresponden a Idle (típicamente frames del row de Walk Down: frames 0-3 del row 3 en LPC) y cuáles a Run (frames 1-3 del mismo row).

- [ ] **Paso 6: Importar y cortar el sprite del Slime**

Igual que los pasos 4-5 pero arrastrar el Slime PNG a `Assets/Sprites/Enemies/`.

Mismos ajustes de importación. Cell Size típica LPC: 64x64.

---

## Task 5: Crear Animator Controller del Jugador

El script `PlayerController.cs` requiere un Animator con el parámetro bool `"IsRunning"`.

- [ ] **Paso 1: Crear carpeta Animations**

En Project → Assets → clic derecho → Create → Folder → `Animations`.

- [ ] **Paso 2: Crear el Animator Controller**

Dentro de Assets/Animations → clic derecho → Create → Animator Controller → nombrar `PlayerAnimator`.

- [ ] **Paso 3: Crear clips de animación**

Doble clic en `PlayerAnimator` para abrirlo en la ventana Animator.

En la ventana Project → Assets/Animations → clic derecho → Create → Animation → nombrar `PlayerIdle`.
Repetir para `PlayerRun`.

- [ ] **Paso 4: Asignar sprites a PlayerIdle**

Seleccionar `PlayerIdle` en Project. Abrir Animation window (Window → Animation → Animation).
En la animation window → Add Property → Sprite Renderer → Sprite.
Arrastrar el frame de idle del Knight (ej: `knight_0`) al track de sprites. Ajustar duración a 0.5s aproximadamente.

- [ ] **Paso 5: Asignar sprites a PlayerRun**

Seleccionar `PlayerRun`. En Animation window:
Add Property → Sprite Renderer → Sprite.
Arrastrar los frames de walk (ej: `knight_1`, `knight_2`, `knight_3`, `knight_1`) espaciados uniformemente. Sample rate: 8 fps.

- [ ] **Paso 6: Configurar el Animator Controller**

En la ventana Animator (con PlayerAnimator abierto):
- Arrastrar `PlayerIdle` y `PlayerRun` al panel → aparecen como estados.
- Clic derecho en `PlayerIdle` → Set as Layer Default State (se pone en naranja).
- En la pestaña Parameters (panel izquierdo) → + → Bool → nombrar exactamente `IsRunning`.
- Clic derecho en `PlayerIdle` → Make Transition → clic en `PlayerRun`. Seleccionar la flecha de transición → en Inspector: desmarcar **Has Exit Time** → Conditions: + → `IsRunning` → `true`.
- Clic derecho en `PlayerRun` → Make Transition → clic en `PlayerIdle`. En Inspector: desmarcar **Has Exit Time** → Conditions: + → `IsRunning` → `false`.

---

## Task 6: Crear Prefab del Jugador

- [ ] **Paso 1: Crear carpeta Prefabs**

Assets → clic derecho → Create → Folder → `Prefabs`.

- [ ] **Paso 2: Crear el GameObject Player en la escena**

En la ventana Hierarchy → clic derecho → Create Empty → renombrar a `Player`.

Asegurarse de que Position = (0, 0, 0).

- [ ] **Paso 3: Añadir componentes**

Con `Player` seleccionado en Hierarchy, en Inspector → Add Component:

1. **Sprite Renderer** → arrastrar el frame idle del Knight al campo Sprite.
2. **Animator** → arrastrar `PlayerAnimator` (el controller creado en Task 5) al campo Controller.
3. **Rigidbody 2D**:
   - Body Type: Dynamic
   - Gravity Scale: **0**
   - Collision Detection: Continuous
   - Freeze Rotation Z: ✓ (marcar)
4. **Capsule Collider 2D** → ajustar tamaño para encajar con el sprite (aproximadamente).
5. **Player Controller** (script).
6. **Player Stats** (script).
7. **Player Attack** (script).

- [ ] **Paso 4: Asignar Tag "Player"**

En el Inspector del GameObject `Player`, campo Tag (arriba del todo) → seleccionar `Player`.

- [ ] **Paso 5: Guardar como Prefab**

Arrastrar el GameObject `Player` desde Hierarchy al folder `Assets/Prefabs/` en el Project. Se crea `Player.prefab`.

El GameObject en Hierarchy queda azul (es instancia del prefab).

---

## Task 7: Crear Prefab del Proyectil

- [ ] **Paso 1: Crear GameObject Projectile en la escena**

Hierarchy → clic derecho → Create Empty → renombrar `Projectile`.

- [ ] **Paso 2: Añadir componentes**

1. **Sprite Renderer** → por ahora dejar Sprite vacío (se ve como rectángulo blanco); o arrastrar cualquier sprite pequeño circular si se tiene.
2. **Rigidbody 2D**:
   - Body Type: **Kinematic** (el script lo pone a kinematic en Awake, pero mejor configurarlo ya)
   - Gravity Scale: 0
3. **Circle Collider 2D**:
   - ✓ **Is Trigger** (marcar)
   - Radius: 0.2
4. **Projectile** (script).

- [ ] **Paso 3: Guardar como Prefab**

Arrastrar `Projectile` de Hierarchy → `Assets/Prefabs/`. Crear `Projectile.prefab`.

Eliminar el GameObject `Projectile` de la Hierarchy (ya está guardado como prefab).

---

## Task 8: Crear Prefab de la Gema de Experiencia

- [ ] **Paso 1: Crear GameObject ExperienceGem en la escena**

Hierarchy → clic derecho → Create Empty → renombrar `ExperienceGem`.

- [ ] **Paso 2: Añadir componentes**

1. **Sprite Renderer** → Color: Cyan (`#00FFFF`) o cualquier sprite de gema si se tiene. Por ahora un color sólido sirve. Para usar color sólido sin sprite: crear un sprite de 1x1 píxel blanco o usar un Circle sprite built-in de Unity (Assets → clic derecho → Create → 2D → Sprites → Circle).
2. **Circle Collider 2D**:
   - ✓ **Is Trigger** (marcar)
   - Radius: 0.3
3. **Experience Gem** (script).

- [ ] **Paso 3: Ajustar Scale**

Transform → Scale: (0.5, 0.5, 1) para que sea pequeña respecto al jugador.

- [ ] **Paso 4: Guardar como Prefab**

Arrastrar a `Assets/Prefabs/`. Eliminar de Hierarchy.

---

## Task 9: Crear Prefab del Enemigo

- [ ] **Paso 1: Crear GameObject Enemy en la escena**

Hierarchy → clic derecho → Create Empty → renombrar `Enemy`.

- [ ] **Paso 2: Añadir componentes**

1. **Sprite Renderer** → arrastrar el frame idle del Slime al campo Sprite.
2. **Rigidbody 2D**:
   - Body Type: Dynamic
   - Gravity Scale: **0**
   - Freeze Rotation Z: ✓
3. **Circle Collider 2D**:
   - Is Trigger: **NO** marcar (el EnemyAI usa OnCollisionStay2D, necesita colisión física real)
   - Radius: ajustar al tamaño del Slime
4. **Enemy AI** (script).
5. **Enemy Stats** (script) → en el campo `experienceGemPrefab`, arrastrar `ExperienceGem.prefab` desde Assets/Prefabs.
6. **Enemy Colorizer** (script).

- [ ] **Paso 3: Asignar Tag y Layer al enemigo**

- Tag → `Enemy`
- Layer → seleccionar `Enemy` (el layer creado en Task 3)

- [ ] **Paso 4: Guardar como Prefab**

Arrastrar a `Assets/Prefabs/`. Eliminar de Hierarchy.

---

## Task 10: Asignar Projectile Prefab al Player

El campo `magicProjectilePrefab` en `PlayerAttack` debe referenciarse desde el prefab del jugador.

- [ ] **Paso 1: Abrir el prefab Player**

Doble clic en `Assets/Prefabs/Player.prefab` para abrirlo en Prefab Edit Mode.

- [ ] **Paso 2: Conectar referencias en PlayerAttack**

Seleccionar el GameObject `Player` en Prefab Mode → en Inspector → componente **Player Attack**:
- `Magic Projectile Prefab`: arrastrar `Assets/Prefabs/Projectile.prefab`
- `Enemy Layer`: clic en el campo LayerMask → seleccionar `Enemy`

- [ ] **Paso 3: Guardar el prefab**

Clic en **Save** (parte superior del Prefab Mode) o usar el botón back (←) y confirmar.

---

## Task 11: Configurar la Escena Game

- [ ] **Paso 1: Renombrar SampleScene**

En Project → Assets/Scenes → clic derecho en `SampleScene` → Rename → escribir `Game`.

- [ ] **Paso 2: Abrir la escena Game**

Doble clic en `Game.unity`.

- [ ] **Paso 3: Eliminar objetos de ejemplo**

Si la escena tiene objetos de ejemplo (Main Camera ya existente, luz direccional, etc.), borrar todo excepto la Main Camera.

- [ ] **Paso 4: Crear el GameObject GameManager**

Hierarchy → clic derecho → Create Empty → renombrar `GameManager`.
Add Component: `Game Manager` (script) y `UI Manager` (script).

- [ ] **Paso 5: Crear el GameObject EnemySpawner**

Hierarchy → clic derecho → Create Empty → renombrar `EnemySpawner`.
Add Component: `Enemy Spawner` (script).

En el componente Enemy Spawner → campo `Enemy Prefabs` (es una List) → Size: 1 → Element 0: arrastrar `Assets/Prefabs/Enemy.prefab`.

- [ ] **Paso 6: Crear el GameObject InventoryManager**

Hierarchy → clic derecho → Create Empty → renombrar `InventoryManager`.
Add Component: `Inventory Manager` (script) y `Shop Manager` (script).

- [ ] **Paso 7: Colocar el Player en la escena**

Arrastrar `Assets/Prefabs/Player.prefab` desde Project a la Hierarchy.
En el Inspector → Transform → Position: (0, 0, 0).

- [ ] **Paso 8: Configurar la cámara**

Seleccionar `Main Camera` en Hierarchy → Add Component → `Camera Follow` (script).

El script intentará encontrar al Player por Tag automáticamente en Start(). Opcionalmente, arrastrar el Transform del Player al campo `Target Info` para no depender del FindWithTag.

Verificar que la cámara tiene:
- Projection: Orthographic
- Size: 5 (ajustar al gusto)
- Position Z: -10

---

## Task 12: Crear el Canvas de UI

- [ ] **Paso 1: Crear el Canvas**

Hierarchy → clic derecho → UI → Canvas.
En el componente **Canvas**: Render Mode → **Screen Space - Overlay**.

Dentro del Canvas se crea automáticamente un `EventSystem` — dejarlo.

- [ ] **Paso 2: Crear el HUD**

Dentro del Canvas en Hierarchy → clic derecho → Create Empty → renombrar `HUD`.

**XP Slider:**
- Dentro de HUD → UI → Slider → renombrar `XpSlider`.
- Anchors: top-stretch (anclado arriba, ancho completo).
- En el componente Slider: Min=0, Max=100, Interactable: desmarcar (no es interactivo).

**LevelText:**
- Dentro de HUD → UI → Legacy → Text → renombrar `LevelText`.
- Texto inicial: `LVL 1`. Font Size: 20. Color: blanco.
- Anchors: top-left.

**TimerText:**
- Dentro de HUD → UI → Legacy → Text → renombrar `TimerText`.
- Texto inicial: `03:00`. Font Size: 24. Color: blanco. Alineación: center.
- Anchors: top-center.

- [ ] **Paso 3: Crear LevelUpPanel**

Dentro del Canvas → UI → Panel → renombrar `LevelUpPanel`.
Ajustar para ocupar ~60% del centro de la pantalla. Color de fondo: negro semitransparente.

Dentro de LevelUpPanel:
- Crear 3 Buttons (UI → Button) renombrados `Option1Button`, `Option2Button`, `Option3Button`.
- En cada botón, cambiar el texto hijo a algo representativo: "Opción 1", "Opción 2", "Opción 3".
- Colocarlos en fila vertical o horizontal.

**Conectar botones a ResumeGame (placeholder):**
Para cada botón → en Inspector → componente Button → sección On Click() → + → arrastrar el GameObject `GameManager` → función: `GameManager.ResumeGame()`.

- [ ] **Paso 4: Crear GameOverPanel**

Dentro del Canvas → UI → Panel → renombrar `GameOverPanel`.
Ajustar para centrar en pantalla.

Dentro de GameOverPanel:
- UI → Legacy → Text → `ResultText` → Texto: `GAME OVER`. Font Size: 36. Color: rojo.
- UI → Button → `RestartButton` → texto hijo: "Reintentar".
  - On Click() → `GameManager` → `GameManager.RestartGame()`.

- [ ] **Paso 5: Crear VictoryPanel**

Igual que GameOverPanel pero renombrar `VictoryPanel`.
`ResultText` con texto: `¡VICTORIA!` y color dorado.
`RestartButton` igual.

---

## Task 13: Conectar Referencias del UIManager

El `UIManager` está en el GameObject `GameManager`. Sus campos deben apuntar a los elementos del Canvas.

- [ ] **Paso 1: Seleccionar el GameObject GameManager**

En Hierarchy → clic en `GameManager`.

- [ ] **Paso 2: Conectar campos del UI Manager**

En el componente **UI Manager** en Inspector:

| Campo | Arrastrar desde Hierarchy |
|-------|--------------------------|
| `Xp Slider` | Canvas/HUD/XpSlider |
| `Level Text` | Canvas/HUD/LevelText |
| `Timer Text` | Canvas/HUD/TimerText |
| `Level Up Panel` | Canvas/LevelUpPanel |
| `Game Over Panel` | Canvas/GameOverPanel |
| `Victory Panel` | Canvas/VictoryPanel |

- [ ] **Paso 3: Guardar la escena**

Ctrl+S (Windows).

---

## Task 14: Añadir Fondo a la Escena

Sin un fondo la cámara muestra el color azul de Unity. Un fondo sólido oscuro es suficiente.

- [ ] **Paso 1: Crear un sprite de fondo**

Opción A (más simple): Cambiar el color de fondo de la cámara.
Seleccionar `Main Camera` → en Inspector → componente Camera → `Background`: cambiar a negro o gris oscuro (#1a1a2e).

Opción B (con sprite): Importar una textura de fondo tileada CC0 o crear un Quad grande con material de color sólido.

Para el día 1, Opción A es suficiente.

- [ ] **Paso 2: Guardar escena**

Ctrl+S.

---

## Task 15: Verificación en Play Mode

- [ ] **Paso 1: Entrar en Play Mode**

Clic en el botón ▶ (Play) en Unity.

Abrir la ventana **Console** (Window → General → Console) para ver errores.

- [ ] **Paso 2: Verificar movimiento del jugador**

Pulsar WASD o las flechas del teclado. El sprite del jugador debe moverse. La cámara debe seguirlo suavemente.

Si hay error "No animator controller": volver a Task 5 y asegurarse de que `PlayerAnimator.controller` está asignado al componente Animator del prefab Player.

- [ ] **Paso 3: Verificar spawn de enemigos**

Después de 2 segundos deben aparecer enemigos en los bordes de la pantalla moviéndose hacia el jugador. Si no aparecen, revisar:
- EnemySpawner tiene el Enemy prefab asignado en `Enemy Prefabs`
- El prefab Enemy tiene el Tag "Enemy" y el Layer "Enemy"

- [ ] **Paso 4: Verificar disparo automático**

Cuando un enemigo entre en el rango de detección (círculo amarillo visible en Scene view), el jugador debe disparar proyectiles hacia él. Si no dispara:
- Verificar que `Player Attack` tiene `Magic Projectile Prefab` → Projectile.prefab
- Verificar que `Enemy Layer` está configurado en el layer "Enemy"

- [ ] **Paso 5: Verificar que los proyectiles matan enemigos**

Los proyectiles deben destruirse al tocar un enemigo. Los enemigos deben reducir vida y morir cuando llegan a 0.

Si el proyectil pasa a través del enemigo sin efecto: verificar que:
- Projectile tiene `Circle Collider 2D` con **Is Trigger = true**
- Enemy tiene `Circle Collider 2D` con **Is Trigger = false**
- Enemy tiene Tag "Enemy"

- [ ] **Paso 6: Verificar drop de gemas y recolección**

Al morir un enemigo debe aparecer una gema. Al toccarla el jugador, debe desaparecer y añadir XP. La barra de XP en el HUD debe actualizarse.

Si no aparece la gema: verificar que `Enemy Stats` tiene `Experience Gem Prefab` → ExperienceGem.prefab.

- [ ] **Paso 7: Verificar Level Up**

Acumular suficiente XP hasta que `currentExp >= expToNextLevel` (100 XP al nivel 1). El juego debe pausarse y aparecer el `LevelUpPanel` con 3 botones. Al hacer clic en cualquier botón, el juego debe reanudarse.

- [ ] **Paso 8: Verificar Game Over**

Dejar que los enemigos toquen al jugador hasta que su vida llegue a 0. Debe aparecer el `GameOverPanel`. El botón Reintentar debe recargar la escena.

- [ ] **Paso 9: Verificar Victoria**

Esperar 180 segundos (3 minutos) — o temporalmente reducir `timeToSurvive` en el GameManager Inspector a 10 segundos para probar. Debe aparecer el `VictoryPanel`.

- [ ] **Paso 10: Salir de Play Mode y guardar**

Clic en ▶ para salir de Play Mode. Ctrl+S para guardar la escena.

---

## Criterios de Éxito

- [ ] El jugador se mueve con WASD sin errores en Console
- [ ] Los enemigos aparecen y persiguen al jugador
- [ ] El jugador dispara automáticamente y mata enemigos
- [ ] Al morir un enemigo cae una gema que da XP al recogerla
- [ ] La barra de XP se actualiza en el HUD
- [ ] Al llegar al XP requerido, aparece el panel de Level Up y el juego pausa
- [ ] Los 3 botones del Level Up reanudan el juego
- [ ] Si la vida llega a 0, aparece Game Over
- [ ] Si el timer llega a 0, aparece Victoria

---

## Sistemas Pendientes (Fuera del Scope de Este Plan)

Estos sistemas están documentados pero no implementados — son el siguiente paso:

- **LevelUpPanel funcional**: los 3 botones deben ofrecer mejoras reales (armas, stats, items). Actualmente solo llaman a `ResumeGame()`.
- **Barra de salud del jugador**: `PlayerStats` tiene la lógica pero `UIManager` no tiene un `Slider` de HP conectado.
- **Texto de oro**: `GameManager.AddGold()` acumula oro pero `UIManager` no tiene un campo para mostrarlo.
- **Escenas MainMenu y GameOver separadas**: actualmente todo está en la escena Game.
- **Meta-progresión**: árbol de habilidades entre partidas.
- **Mascotas y habilidades activas**: documentadas en WeaponsCatalog.md y CharactersAndLoadout.md.
- **Audio**: música de fondo y SFX.
- **Enemigos adicionales**: Bat, Skeleton, Ghost, Golem (misma estructura que Slime).
