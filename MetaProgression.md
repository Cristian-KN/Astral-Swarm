# 🌌 Meta-Progresión y Árbol de Habilidades - Astral Swarm

Este documento define qué ocurre cuando el jugador muere y cómo evoluciona permanentemente su poder a través de las partidas.

---

## 💎 Sistema de Puntos de Astrum (PA)

Al final de cada partida (muerte o victoria), el jugador recibe **Puntos de Astrum** basados en su rendimiento:

**Fórmula de Puntos:**
`PA = (Nivel Final * 0.5) + (Tiempo Sobrevivido / 60)`

*   *Ejemplo*: Si llegas al Nivel 10 y sobrevives 5 minutos (300s):
    `PA = (10 * 0.5) + (300 / 60) = 5 + 5 = 10 Puntos.`
*   Los puntos pueden tener decimales (ej: 7.5) y se acumulan en tu cuenta global.

---

## 🌳 Árbol de Habilidades (Skill Tree)

El árbol está dividido en niveles de profundidad. Debes comprar nodos anteriores para avanzar.

### Nivel 1: Los Cimientos (Disponibles desde el inicio)
| Nodo | Efecto Permanente | Coste (PA) |
| :--- | :--- | :--- |
| **Vitalidad Astral I** | +10 Vida Base | 5 |
| **Fuerza Bruta I** | +2 Daño Base | 5 |
| **Botas de Mercurio** | +5% Velocidad de Movimiento | 8 |

### Nivel 2: Desbloqueos de Mecánicas (Requiere 2 nodos de Nivel 1)
| Nodo | Efecto Permanente | Coste (PA) |
| :--- | :--- | :--- |
| **🛠️ Maestría de Activas** | **DESBLOQUEO**: Permite equipar Habilidades Activas. | 15 |
| **🐾 Amigo de lo Ajeno** | **DESBLOQUEO**: Permite equipar Mascotas (Pets). | 15 |
| **Negociante I** | +1 Slot base en la Tienda (Total: 4) | 20 |

### Nivel 3: Potencia Avanzada (Requiere Desbloqueos de Nivel 2)
| Nodo | Efecto Permanente | Coste (PA) |
| :--- | :--- | :--- |
| **Suerte del Destino** | +5 Suerte Base (Mejor botín/críticos) | 25 |
| **Escudo de Vacío** | +2 Defensa Base (Reducción plana) | 30 |
| **Mecenas de la Tienda** | Aumenta la probabilidad de items Épicos/Raros en tienda | 40 |

### Nivel 4: El Elegido de los Astros
| Nodo | Efecto Permanente | Coste (PA) |
| :--- | :--- | :--- |
| **Pacto de Sacrificio** | Los items de Sacrificio dan un 20% más de stats | 50 |
| **Resonancia Evolutiva** | Los objetos de Crecimiento escalan un 10% más rápido | 60 |

---

## 🏪 Evolución de la Tienda

A medida que compras nodos de "Tienda" en el árbol, el NPC/Máquina expendedora mejora:
1.  **Nivel 1 (Base)**: 3 Slots, mayormente items Comunes y Raros.
2.  **Nivel 2**: 4 Slots, desbloquea aparición de items de Sacrificio con más frecuencia.
3.  **Nivel 3**: 5 Slots, posibilidad pequeña de encontrar un item Mítico (Rojo) a precio de oro.

---

## 💾 Persistencia (Technical Note)
Para implementar esto en Unity sin base de datos, usaremos **`PlayerPrefs`** o un archivo **JSON** local para guardar:
*   `total_astrum_points` (float)
*   `unlocked_nodes` (lista de IDs)
*   `permanent_attack_bonus`, `permanent_health_bonus`, etc.
