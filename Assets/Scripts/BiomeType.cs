using UnityEngine;

public enum BiomeType
{
    // Biomas Normales (rotan cada X minutos)
    VoidSpace,          // Vacío espacial oscuro (inicio)
    CrimsonNebula,      // Nebulosa roja (peligro)
    FrozenVoid,         // Vacío helado azul
    ToxicCloud,         // Nube tóxica verde
    ElectricStorm,      // Tormenta eléctrica amarilla
    DeepAbyss,          // Abismo profundo morado
    SolarFlare,         // Llamarada solar naranja
    CosmicRift,         // Grieta cósmica multicolor
    DarkMatter,         // Materia oscura gris
    VoidEdge,           // Borde del vacío (final)

    // Bioma Especial (20% probabilidad)
    GoldenAnomalySplash       // Anomalía dorada - High risk, high reward
}

[System.Serializable]
public class BiomeData
{
    public BiomeType type;
    public string displayName;

    [Header("Colores")]
    public Color primaryColor;
    public Color secondaryColor;
    public Color accentColor;

    [Header("Efectos Visuales")]
    [Range(0f, 1f)] public float starDensity = 0.02f;
    [Range(0f, 1f)] public float nebulaDensity = 0.3f;
    public bool hasGlow = false;

    [Header("Modificadores de Gameplay")]
    [Tooltip("Multiplicador de dificultad de enemigos (1.0 = normal)")]
    public float enemyDifficultyMultiplier = 1.0f;

    [Tooltip("Multiplicador de EXP (1.0 = normal)")]
    public float expMultiplier = 1.0f;

    [Tooltip("Multiplicador de oro (1.0 = normal)")]
    public float goldMultiplier = 1.0f;

    [Tooltip("Bonus de suerte (0 = sin bonus)")]
    public float luckBonus = 0f;

    [Tooltip("Tier de variante mínimo para enemigos (0 = Normal, 1 = Verde+, etc.)")]
    public int minEnemyVariantTier = 0;

    [Header("Música/Ambiente")]
    public AudioClip ambientSound;
}
