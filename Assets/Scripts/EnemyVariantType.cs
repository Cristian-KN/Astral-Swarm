using UnityEngine;

/// <summary>
/// Tipos de variantes de enemigos disponibles en Astral Swarm.
/// </summary>
public enum EnemyVariantType
{
    Normal,     // Blanco/Gris - 1x stats
    Verde,      // Esmeralda - 1.5x stats + Regen
    Amarilla,   // Dorado - 2x stats + 10x Oro / 2x Exp
    Azul,       // Cian - 2x stats + 10x Exp / 2x Oro
    Morada,     // Púrpura - 3x stats + Anti-knockback
    Negra,      // Oscuro - 9x stats (3x Morada)
    Roja        // Carmesí - 27x stats + Alta Regen
}
