using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BiomeManager : MonoBehaviour
{
    public static BiomeManager Instance { get; private set; }

    [Header("Configuración de Biomas")]
    [SerializeField] private BiomeData[] normalBiomes;
    [SerializeField] private BiomeData specialBiome; // Golden Anomaly

    [Header("Configuración de Rotación")]
    [Tooltip("Tiempo en segundos entre cambios de bioma")]
    [SerializeField] private float biomeDuration = 180f; // 3 minutos por defecto

    [Tooltip("Probabilidad de que aparezca el bioma especial (0.2 = 20%)")]
    [Range(0f, 1f)]
    [SerializeField] private float specialBiomeChance = 0.2f;

    [Header("Avisos")]
    [SerializeField] private float warningTime = 10f; // Avisar 10s antes del cambio
    [SerializeField] private Color warningColor = Color.red;

    [Header("Referencias")]
    [SerializeField] private SpaceBackgroundGenerator backgroundGenerator;

    // Estado actual
    private BiomeData currentBiome;
    private float biomeTimer;
    private bool isSpecialBiome;
    private int biomeIndex = 0;
    private bool warningShown = false;

    // Eventos
    public delegate void BiomeChangeEvent(BiomeData newBiome, bool isSpecial);
    public event BiomeChangeEvent OnBiomeChange;

    public delegate void BiomeWarningEvent(float timeRemaining);
    public event BiomeWarningEvent OnBiomeWarning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeDefaultBiomes();
    }

    private void Start()
    {
        if (backgroundGenerator == null)
        {
            backgroundGenerator = FindObjectOfType<SpaceBackgroundGenerator>();
        }

        // Decidir si la partida empieza con bioma especial
        if (Random.value < specialBiomeChance)
        {
            ActivateSpecialBiome();
        }
        else
        {
            ActivateBiome(normalBiomes[0], false);
        }
    }

    private void Update()
    {
        biomeTimer += Time.deltaTime;

        // Aviso antes del cambio
        float timeRemaining = biomeDuration - biomeTimer;
        if (!warningShown && timeRemaining <= warningTime && timeRemaining > 0)
        {
            warningShown = true;
            OnBiomeWarning?.Invoke(timeRemaining);
            ShowBiomeWarning();
        }

        // Cambio de bioma
        if (biomeTimer >= biomeDuration)
        {
            ChangeBiome();
        }
    }

    private void ChangeBiome()
    {
        biomeTimer = 0f;
        warningShown = false;

        // 20% chance de bioma especial (si no está ya activo)
        if (!isSpecialBiome && Random.value < specialBiomeChance)
        {
            ActivateSpecialBiome();
        }
        else
        {
            // Rotar al siguiente bioma normal
            biomeIndex = (biomeIndex + 1) % normalBiomes.Length;
            ActivateBiome(normalBiomes[biomeIndex], false);
        }
    }

    private void ActivateSpecialBiome()
    {
        isSpecialBiome = true;
        ActivateBiome(specialBiome, true);
        Debug.Log($"[BiomeManager] ⭐ BIOMA ESPECIAL ACTIVADO: {specialBiome.displayName}");
    }

    private void ActivateBiome(BiomeData biome, bool special)
    {
        currentBiome = biome;
        isSpecialBiome = special;

        // Actualizar fondo visual
        if (backgroundGenerator != null)
        {
            ApplyBiomeToBackground(biome);
        }

        // Notificar a otros sistemas
        OnBiomeChange?.Invoke(biome, special);

        Debug.Log($"[BiomeManager] Bioma cambiado a: {biome.displayName} " +
                  $"(Dif: x{biome.enemyDifficultyMultiplier}, " +
                  $"EXP: x{biome.expMultiplier}, " +
                  $"Oro: x{biome.goldMultiplier})");
    }

    private void ApplyBiomeToBackground(BiomeData biome)
    {
        // Usar reflexión para cambiar los valores privados del generador
        var genType = backgroundGenerator.GetType();

        var primaryField = genType.GetField("primaryColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var secondaryField = genType.GetField("secondaryColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var accentField = genType.GetField("accentColor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var starField = genType.GetField("starDensity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nebulaField = genType.GetField("nebulaDensity",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (primaryField != null) primaryField.SetValue(backgroundGenerator, biome.primaryColor);
        if (secondaryField != null) secondaryField.SetValue(backgroundGenerator, biome.secondaryColor);
        if (accentField != null) accentField.SetValue(backgroundGenerator, biome.accentColor);
        if (starField != null) starField.SetValue(backgroundGenerator, biome.starDensity);
        if (nebulaField != null) nebulaField.SetValue(backgroundGenerator, biome.nebulaDensity);

        backgroundGenerator.GenerateBackground();
    }

    private void ShowBiomeWarning()
    {
        // Aquí puedes añadir efectos visuales de aviso
        // Por ejemplo: parpadeo de pantalla, sonido, UI warning, etc.
        Debug.Log($"[BiomeManager] ⚠️ El bioma cambiará en {warningTime} segundos!");
    }

    // Getters públicos
    public BiomeData GetCurrentBiome() => currentBiome;
    public bool IsSpecialBiome() => isSpecialBiome;
    public float GetBiomeTimeRemaining() => biomeDuration - biomeTimer;
    public float GetBiomeProgress() => biomeTimer / biomeDuration;

    // Modificadores aplicables a otros sistemas
    public float GetEnemyDifficultyMultiplier() => currentBiome?.enemyDifficultyMultiplier ?? 1f;
    public float GetExpMultiplier() => currentBiome?.expMultiplier ?? 1f;
    public float GetGoldMultiplier() => currentBiome?.goldMultiplier ?? 1f;
    public float GetLuckBonus() => currentBiome?.luckBonus ?? 0f;
    public int GetMinEnemyVariantTier() => currentBiome?.minEnemyVariantTier ?? 0;

    // Forzar cambio manual (para testing)
    [ContextMenu("Force Biome Change")]
    public void ForceChangeBiome()
    {
        ChangeBiome();
    }

    [ContextMenu("Force Special Biome")]
    public void ForceSpecialBiome()
    {
        ActivateSpecialBiome();
    }

    private void InitializeDefaultBiomes()
    {
        if (normalBiomes == null || normalBiomes.Length == 0)
        {
            normalBiomes = new BiomeData[]
            {
                // Biomas terrestres tipo Vampire Survivors
                CreateBiome(BiomeType.VoidSpace, "Pradera Verde",
                    new Color(0.15f, 0.35f, 0.15f), new Color(0.2f, 0.5f, 0.2f), new Color(0.4f, 0.7f, 0.3f),
                    0.015f, 0.2f, 1.0f, 1.0f, 1.0f, 0f, 0),

                CreateBiome(BiomeType.CrimsonNebula, "Bosque Carmesí",
                    new Color(0.3f, 0.1f, 0.1f), new Color(0.4f, 0.15f, 0.1f), new Color(0.8f, 0.2f, 0.2f),
                    0.02f, 0.5f, 1.2f, 1.0f, 1.0f, 0f, 0),

                CreateBiome(BiomeType.FrozenVoid, "Tundra Helada",
                    new Color(0.7f, 0.8f, 0.9f), new Color(0.8f, 0.85f, 0.95f), new Color(0.9f, 0.95f, 1.0f),
                    0.025f, 0.3f, 1.1f, 1.0f, 1.0f, 0f, 0),

                CreateBiome(BiomeType.ToxicCloud, "Pantano Tóxico",
                    new Color(0.2f, 0.3f, 0.1f), new Color(0.25f, 0.4f, 0.15f), new Color(0.4f, 0.7f, 0.2f),
                    0.01f, 0.6f, 1.3f, 1.1f, 1.0f, 0f, 1),

                CreateBiome(BiomeType.ElectricStorm, "Desierto de Tormenta",
                    new Color(0.6f, 0.5f, 0.3f), new Color(0.7f, 0.6f, 0.4f), new Color(0.9f, 0.8f, 0.4f),
                    0.03f, 0.4f, 1.4f, 1.1f, 1.1f, 0.05f, 1),

                CreateBiome(BiomeType.DeepAbyss, "Caverna Oscura",
                    new Color(0.1f, 0.08f, 0.12f), new Color(0.15f, 0.12f, 0.18f), new Color(0.3f, 0.2f, 0.4f),
                    0.018f, 0.5f, 1.5f, 1.2f, 1.1f, 0f, 1),

                CreateBiome(BiomeType.SolarFlare, "Volcán Ardiente",
                    new Color(0.4f, 0.2f, 0.1f), new Color(0.5f, 0.25f, 0.1f), new Color(0.9f, 0.4f, 0.1f),
                    0.02f, 0.4f, 1.6f, 1.2f, 1.2f, 0.05f, 2),

                CreateBiome(BiomeType.CosmicRift, "Jardín Místico",
                    new Color(0.25f, 0.2f, 0.35f), new Color(0.35f, 0.25f, 0.45f), new Color(0.6f, 0.4f, 0.8f),
                    0.035f, 0.6f, 1.7f, 1.3f, 1.2f, 0.1f, 2),

                CreateBiome(BiomeType.DarkMatter, "Cementerio Sombrío",
                    new Color(0.1f, 0.1f, 0.12f), new Color(0.15f, 0.15f, 0.18f), new Color(0.25f, 0.25f, 0.3f),
                    0.008f, 0.2f, 1.8f, 1.3f, 1.3f, 0f, 2),

                CreateBiome(BiomeType.VoidEdge, "Templo Corrupto",
                    new Color(0.3f, 0.2f, 0.35f), new Color(0.4f, 0.25f, 0.45f), new Color(0.7f, 0.4f, 0.8f),
                    0.04f, 0.7f, 2.0f, 1.5f, 1.4f, 0.15f, 3),
            };
        }

        if (specialBiome == null)
        {
            specialBiome = CreateBiome(BiomeType.GoldenAnomalySplash, "⭐ Jardín Dorado ⭐",
                new Color(0.6f, 0.5f, 0.2f), new Color(0.7f, 0.6f, 0.3f), new Color(0.95f, 0.8f, 0.3f),
                0.05f, 0.4f, 2.5f, 2.0f, 2.5f, 0.25f, 1);
        }
    }

    private BiomeData CreateBiome(BiomeType type, string name,
        Color primary, Color secondary, Color accent,
        float starDens, float nebulaDens,
        float diffMult, float expMult, float goldMult, float luck, int minTier)
    {
        return new BiomeData
        {
            type = type,
            displayName = name,
            primaryColor = primary,
            secondaryColor = secondary,
            accentColor = accent,
            starDensity = starDens,
            nebulaDensity = nebulaDens,
            enemyDifficultyMultiplier = diffMult,
            expMultiplier = expMult,
            goldMultiplier = goldMult,
            luckBonus = luck,
            minEnemyVariantTier = minTier
        };
    }
}
