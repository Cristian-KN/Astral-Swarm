# 🏪 Economía y Tiendas - Astral Swarm

Este documento describe el funcionamiento de los NPCs y las máquinas expendedoras (Vending Machines) dentro del juego.

## 🛒 Puntos de Venta (NPCs / Máquinas)

A lo largo del nivel, el jugador podrá encontrar estaciones de compra. Estas pueden ser:
*   **Máquinas Expendedoras**: Estáticas en el mapa.
*   **Mercaderes Errantes (NPCs)**: Aparecen por tiempo limitado o en zonas seguras.

## 🏷️ Sistema de Precios

Los precios de los objetos se calculan dinámicamente y el jugador **debe interactuar físicamente** con el NPC o la máquina para abrir el menú de compra.

**Fórmula de Precio:**
`Precio = MultiplicadorRareza * NivelJugador * FactorBase`

### 💎 Multiplicadores de Rareza

| Rareza | Multiplicador | Color Visual |
| :--- | :---: | :--- |
| **Común** | 2.5 | Blanco / Gris |
| **Poco Común** | 6.25 | Verde |
| **Raro** | 12.5 | Azul |
| **Épico** | 25 | Morado |
| **Legendario** | 62.5 | Dorado / Naranja |
| **Mítico** | 150 | Rojo |

*Ejemplo: Un objeto Raro en nivel 5 costaría `12.5 * 5 * 1 = 62.5` de Oro.*

---

## 📦 Inventario de la Tienda

Las tiendas ofrecerán una selección aleatoria de estos 4 tipos de objetos:

1.  **Armas**: Nuevas formas de ataque automático.
2.  **Stats**: Mejoras directas a las estadísticas base.
3.  **Mascotas**: Compañeros de apoyo.
4.  **Objetos Evolutivos**: Equipamiento que crece con cada enemigo derrotado.

---

## 🛠️ Lógica de Implementación (Unity)

Para implementar esto, usaremos un script `ShopManager.cs` que:
1.  Detecte cuando el jugador entra en el área de la tienda (Trigger).
2.  Muestre una UI con 3 opciones aleatorias.
3.  Verifique si el jugador tiene suficiente `Gold` acumulado.
4.  Aplique la mejora y reste el oro.
