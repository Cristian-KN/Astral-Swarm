# 👾 Variantes de Enemigos - Astral Swarm

Este documento detalla las variantes de color para los 5 tipos de enemigos base del juego, incluyendo sus multiplicadores de fuerza, rasgos especiales y sistema de recompensas (Drops).

## 📊 Sistema de Tiers y Multiplicadores

| Variante | Fuerza | Rasgo Especial | Drops (Oro) | Drops (Exp) |
| :--- | :---: | :--- | :--- | :--- |
| **Normal** | 1x | Ninguno | 1x | 1x |
| **Verde** | 1.5x | Regeneración moderada | 3x | 3x |
| **Amarilla** | 2x | Rápido (Loot Oro) | **10x** | **2x** |
| **Azul** | 2x | Errático (Loot Exp) | **2x** | **10x** |
| **Morada** | 3x | Tanque / Anti-knockback | 4x | 4x |
| **Negra** | 9x | Gran daño y resistencia | 15x | 15x |
| **Roja** | 27x | Regeneración alta | 100x | 100x |

---

## 💰 Sistema de Recompensas (Drop Logic)

El drop base de un enemigo **Normal** se calcula con la siguiente fórmula:
`DropBase = 1 + NivelJugador + (TiempoTranscurrido / 60)`

Esto asegura que las recompensas escalen conforme avanza la partida y el jugador progresa.

---

## 🧬 Detalles de las Variantes

### ⚪ Variante Normal
*   **Descripción**: El bicho estándar.
*   **Drop**: Base (1x).

### 🟢 Variante Verde (El Persistente)
*   **Fuerza**: 1.5x.
*   **Trait**: Regeneración de vida constante.
*   **Drop**: 3 veces más que el Normal.

### 🟡 Variante Amarilla (El Tesoro - Oro)
*   **Fuerza**: 2x.
*   **Trait**: Se mueve rápido intentando escapar.
*   **Drop**: **10x Oro** y **2x Exp**.

### 🔵 Variante Azul (La Sabiduría - Exp)
*   **Fuerza**: 2x.
*   **Trait**: Movimiento difícil de predecir.
*   **Drop**: **10x Exp** y **2x Oro**.

### 🟣 Variante Morada (El Tanque)
*   **Fuerza**: 3x.
*   **Trait**: Resistente a empujones.
*   **Drop**: 4 veces más que el Normal.

### 🌑 Variante Negra (La Pesadilla)
*   **Fuerza**: 9x (3x Morada).
*   **Trait**: Muy alta resistencia y daño.
*   **Drop**: 15 veces más que el Normal.

### 🔴 Variante Roja (El Azote Divino)
*   **Fuerza**: 27x (3x Negra).
*   **Trait: Regeneración Alta**: Se ha nerfeado ligeramente la regeneración respecto a la versión original para que sea matable con un build fuerte, pero sigue siendo un reto masivo.
*   **Drop**: **100x** (Recompensa legendaria por derrotar al bicho más roto).

---

## 🎨 Guía de Implementación Visual (Recolors)
1.  **Normal**: Colores base.
2.  **Verde**: Esmeralda.
3.  **Amarillo**: Dorado brillante.
4.  **Azul**: Cian / Azul Eléctrico.
5.  **Morado**: Púrpura oscuro.
6.  **Negro**: Desaturado / Oscuro.
7.  **Rojo**: Carmesí intenso.

