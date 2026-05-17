# 👾 Variantes de Enemigos - Astral Swarm

Este documento detalla las **Variantes (Tiers)** que se aplican sobre cualquier Arquetipo. Definen el color y los multiplicadores de estadísticas.

## 📊 Sistema de Tiers (Modificadores)

| Variante | Color Visual | Multiplicador | Rasgo Especial | Drops (Oro/Exp) |
| :--- | :--- | :---: | :--- | :--- |
| **Normal** | Blanco/Original | 1x | Ninguno | 1x |
| **Verde** | Esmeralda | 1.5x | Regeneración | 3x |
| **Amarilla** | Dorado | 2x | Rápido (+Velocidad) | 10x Oro / 2x Exp |
| **Azul** | Cian | 2x | Movimiento Errático | 2x Oro / 10x Exp |
| **Morada** | Púrpura | 3x | Pesado (No Knockback) | 4x |
| **Negra** | Oscuro | 9x | Daño Masivo | 15x |
| **Roja** | Carmesí | 27x | Regeneración Alta | 100x |

---

## 🎨 Lógica de Color (Unity)
Para aplicar estas variantes sin tener miles de archivos, se usa el componente `SpriteRenderer.color`.
1. **Normal**: (255, 255, 255)
2. **Verde**: (100, 255, 100)
3. **Amarillo**: (255, 255, 100)
4. **Azul**: (100, 200, 255)
5. **Morado**: (200, 100, 255)
6. **Negro**: (50, 50, 50)
7. **Rojo**: (255, 50, 50)

---
