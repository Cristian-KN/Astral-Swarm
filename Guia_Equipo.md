# Guía del Proyecto: Base "Vampire Survivors" 2D (Unity)

¡Hola equipo!
Este documento es un resumen de cómo está estructurado técnica y operativamente nuestro proyecto. La idea es que cualquiera pueda entender la arquitectura y cómo funcionan las piezas para poder añadir arte, niveles o modificar valores.

## 📌 Concepto General
Es un juego 2D Top-Down estilo *Vampire Survivors*. El jugador controla únicamente el movimiento en 8 direcciones. El disparo es **automático** (hacia el enemigo más cercano). Los enemigos aparecen continuamente en hordas fuera de la pantalla (fuera de la cámara) y persiguen al jugador. Al morir, sueltan gemas de experiencia que nos permitirán subir de nivel.

## 📁 Estructura de Carpetas (Obligatoria)
Para mantener el proyecto ordenado para la entrega, todos los archivos deben ir a sus carpetas correspondientes en `Assets/`:
*   `/Scripts`: Todo el código fuente en C#.
*   `/Sprites`: Nuestras imágenes (Tilemaps, Personajes, UI). Usamos un estilo **Pixel Art**.
*   `/Scenes`: Aquí irán nuestras 3 escenas (Menu, Game, GameOver).
*   `/Prefabs`: Objetos prefabricados reusables (El misil, la gema, los enemigos base).
*   `/Sounds`: Efectos de sonido (SFX) y música de fondo.

---

## 💻 Arquitectura de Scripts (Cómo funciona por debajo)

Hemos programado el juego dividiendo radicalmente las responsabilidades para que si falla el movimiento, sepamos exactamente qué script arreglar sin romper el ataque.

### 🧙‍♂️ 1. El Jugador (Player)
En lugar de un script gigante de 500 líneas, el jugador tiene 3 pequeños:
*   **`PlayerController.cs`**: *Solo* lee el teclado (`Input.GetAxisRaw`) y mueve el `Rigidbody2D` aplicándole velocidad. También gira el sprite.
*   **`PlayerStats.cs`**: *Solo* controla la Vida. Tiene la regla de la **usabilidad visual**: cuando el jugador recibe daño de un enemigo, este script pone el sprite de color ROJO 🔴 por 0.15 segundos y lo hace invulnerable 1 segundo.
*   **`PlayerAttack.cs`**: Cada 'X' segundos lanza un círculo invisible (`Physics2D.OverlapCircleAll`) buscando la capa "Enemy". Si encuentra alguno, calcula distancias e instancia un `Prefab` de misil apuntando al más cercano.

### 💀 2. Los Enemigos
*   **`EnemyAI.cs`**: Busca al "Player" en el mapa y mueve su `Rigidbody2D` directamente hacia él en línea recta. Si su *Collider* toca al jugador, llama a `player.TakeDamage`.
*   **`EnemyStats.cs`**: Lleva la vida del bicho. Si su vida llega a 0, instancia el `Prefab` de la gema de XP y hace `Destroy()` de sí mismo.
*   **`EnemySpawner.cs`**: (El controlador de hordas). Se pone en un objeto vacío de la escena. Cada varios segundos, calcula un "anillo" gigante alrededor del jugador (fuera de lo que ve la cámara) y crea un enemigo aleatorio ahí.

### 🪄 3. Cosas Sueltas
*   **`Projectile.cs`**: Script que se le pone al misil que lanza el jugador. Se mueve en línea recta en la dirección que le dio el `PlayerAttack` y si entra en un `Trigger` de un enemigo, le quita vida y desaparece.
*   **`CameraFollow.cs`**: Asegura que la cámara principal siga la posición del jugador suavemente usando la función matemática `SmoothDamp`.

---

## 🎨 Tareas de Arte y Nivel (Para los Diseñadores/Level Designers)

1.  **Montar el Mapa**: Usaremos la herramienta **Tile Palette** (Window > 2D > Tile Palette). Deben usar el componente `Tilemap Collider 2D` y `Composite Collider 2D` en los muros del mapa para que el jugador y los enemigos no puedan salirse.
2.  **Configurar Enemigos**: Pueden crear un "Slime" o "Esqueleto" arrastrando el sprite a la escena, añadiéndole Rigidbody2D (Gravity = 0), un Collider, los scripts `EnemyAI` y `EnemyStats`. Luego **arrastrar ese bicho a la carpeta Prefabs** para guardarlo.
3.  **Animaciones**: Crear al menos `Idle` (Quieto) y `Run` (Corriendo) para el jugador usando la ventana Animator, transicionando con el parámetro Booleano `IsRunning`.

*Cualquier duda sobre valores o velocidades, ¡todo está expuesto en el Inspector de Unity con variables como MoveSpeed o AttackCooldown!*
