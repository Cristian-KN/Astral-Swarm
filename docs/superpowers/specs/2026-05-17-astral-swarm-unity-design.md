# Astral Swarm — Implementación Completa en Unity
**Fecha:** 2026-05-17  
**Enfoque:** B — Escena core con assets CC0 mínimos para día 1 jugable, expansión incremental

---

## Estado del Proyecto

El proyecto tiene **16 scripts C# completos** con toda la lógica del juego ya implementada. No requiere migración de motor — ya es un proyecto Unity 6.0.3 con URP. Lo que falta es:

- Escenas configuradas
- Prefabs con sprites asignados
- Assets visuales (sprites CC0)
- UI Canvas conectado
- Referencias del Inspector asignadas

---

## Arquitectura de Escenas

El juego usa **3 escenas**:

| Escena | Propósito |
|--------|-----------|
| `MainMenu` | Pantalla de inicio, botón Play |
| `Game` | Loop principal (basada en SampleScene.unity existente) |
| `GameOver` | Panel de resultado con stats y reinicio |

La escena `Game` se construye primero — es la que contiene todo el gameplay.

### Jerarquía de la Escena Game

```
Game (Scene)
├── --- MANAGERS ---
│   ├── GameManager          (GameManager.cs, UIManager.cs)
│   ├── EnemySpawner         (EnemySpawner.cs)
│   └── InventoryManager     (InventoryManager.cs, ShopManager.cs)
│
├── --- PLAYER ---
│   └── Player               (PlayerController, PlayerStats, PlayerAttack,
│                             Rigidbody2D, CapsuleCollider2D, SpriteRenderer)
│
├── --- CAMERA ---
│   └── Main Camera          (CameraFollow.cs)
│
├── --- UI ---
│   └── Canvas (Screen Space - Overlay)
│       ├── HUD              (ExpBar, LevelText, TimerText, GoldText)
│       ├── LevelUpPanel     (3 botones de elección de mejora)
│       └── GameOverPanel    (ResultText, StatsText, RestartButton)
│
└── --- WORLD ---
    └── Background           (Tilemap o sprite de fondo tileado)
```

---

## Prefabs

| Prefab | Componentes Unity | Sprite fuente |
|--------|------------------|---------------|
| `Player` | PlayerController, PlayerStats, PlayerAttack, Rigidbody2D (gravityScale=0), CapsuleCollider2D, SpriteRenderer, Animator | LPC Character Assets — Knight o Mage |
| `Enemy` | EnemyAI, EnemyStats, EnemyColorizer, Rigidbody2D (gravityScale=0), CircleCollider2D, SpriteRenderer, Animator | LPC Monsters o 50+ Monsters Pack |
| `Projectile` | Projectile.cs, Rigidbody2D, CircleCollider2D (trigger), SpriteRenderer | Projectile Art Collection (OpenGameArt) |
| `ExperienceGem` | ExperienceGem.cs, CircleCollider2D (trigger, isTrigger=true), SpriteRenderer | Sprite simple CC0 o color sólido |

---

## Assets CC0 — Orden de Descarga

### Día 1 (mínimo jugable)
1. **Jugador** — [LPC Medieval Fantasy Character Sprites](https://opengameart.org/content/lpc-medieval-fantasy-character-sprites) — Knight con idle/walk
2. **Enemigo base** — Slime del pack [LPC Monsters](https://opengameart.org/content/lpc-monsters) — compatible con EnemyColorizer para las 7 variantes

### Expansión posterior (sin romper gameplay)
3. Bat, Skeleton, Ghost, Golem — mismo pack LPC Monsters
4. Proyectiles — [Projectile Art Collection](https://opengameart.org/content/projectile-attacks-art-collection)
5. Enemigos adicionales — [50+ Monsters Pack](https://opengameart.org/content/50-monsters-pack-2d)

---

## UI Canvas

```
Canvas (Screen Space - Overlay)
├── HUD
│   ├── ExpBar          → Slider — UIManager.UpdateExperienceBar()
│   ├── LevelText       → TMP_Text — "Nivel 3"
│   ├── TimerText       → TMP_Text — "1:45"
│   └── GoldText        → TMP_Text — "Gold: 120"
│
├── LevelUpPanel        → activado por UIManager.ShowLevelUpMenu(true)
│   ├── Option1Button
│   ├── Option2Button
│   └── Option3Button
│
└── GameOverPanel       → activado por GameManager.GameOver() / Victory()
    ├── ResultText      → "¡Victoria!" / "Game Over"
    ├── StatsText       → tiempo, kills, nivel alcanzado
    └── RestartButton   → SceneManager.LoadScene("Game")
```

---

## Tags y Layers Requeridos

| Tag | Requerido por |
|-----|--------------|
| `Player` | EnemyAI.cs, ExperienceGem.cs |
| `Enemy` | PlayerAttack.cs |

| Layer | Requerido por |
|-------|--------------|
| `Enemy` | PlayerAttack.cs — Physics2D.OverlapCircleAll con enemyLayer |

---

## Conexiones del Inspector (Referencias Manuales)

| GameObject / Script | Campo | Se arrastra |
|--------------------|-------|-------------|
| GameManager → GameManager.cs | `uiManager` | GameObject con UIManager |
| Player → PlayerAttack.cs | `magicProjectilePrefab` | Prefab Projectile |
| Player → PlayerAttack.cs | `enemyLayer` | Layer "Enemy" |
| EnemySpawner → EnemySpawner.cs | `enemyPrefab` | Prefab Enemy |
| EnemySpawner → EnemySpawner.cs | `player` | GameObject Player |
| Main Camera → CameraFollow.cs | `target` | Transform Player |
| ExperienceGem (Prefab) → ExperienceGem.cs | `player` | — (se asigna en runtime via GameObject.FindWithTag) |

---

## Flujo de Datos Principal

```
EnemyStats.Die()
  → GameManager.AddExperience(amount)
      → UIManager.UpdateExperienceBar()
      → GameManager.LevelUp() si currentExp >= expToNextLevel
          → UIManager.ShowLevelUpMenu(true)
          → GameManager.PauseGame()
  → GameManager.AddGold(amount)
  → InventoryManager.OnEnemyKilled()
      → GrowthItem.currentGrowthBonus += growthPerKill
      → ApplyAllStats()

PlayerStats.TakeDamage(amount)
  → UIManager.UpdateHealthBar()
  → GameManager.GameOver() si health <= 0
```

---

## Sistemas Pendientes (Post-Core)

Estos sistemas están documentados pero no implementados en scripts. Se abordan en fases posteriores:

| Sistema | Documentación | Estado |
|---------|--------------|--------|
| Meta-Progresión (árbol de habilidades) | MetaProgression.md | No implementado |
| Mascotas | CharactersAndLoadout.md, WeaponsCatalog.md | No implementado |
| Habilidades Activas | WeaponsCatalog.md | Parcialmente (slots en InventoryManager) |
| Tienda con mercader | EconomyAndShop.md | ShopManager.cs existe, falta UI y prefab |
| Escenas MainMenu y GameOver | — | Pendientes |

---

## Criterios de Éxito (Día 1)

- [ ] Jugador se mueve con WASD/flechas
- [ ] Disparo automático al enemigo más cercano
- [ ] Enemigos aparecen, persiguen y dañan al jugador
- [ ] Al matar enemigo: cae gema, se recoge, sube XP
- [ ] Al acumular XP suficiente: pausa + panel de level-up con 3 opciones
- [ ] Timer visible y panel de victoria/derrota funcionando
