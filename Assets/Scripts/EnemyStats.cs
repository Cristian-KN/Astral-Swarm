using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Variante")]
    public EnemyVariantType variant = EnemyVariantType.Normal;

    [Header("Estadísticas Base (Normal)")]
    [SerializeField] private float baseMaxHealth = 50;
    private float currentHealth;
    [SerializeField] private float baseRegenPerSecond = 0f;

    [Header("Recompensas")]
    [SerializeField] private GameObject[] gemPrefabs;
    [SerializeField] private GameObject[] moneyPrefabs;

    private float healthMultiplier = 1f;
private float expMultiplier = 1f;
    private float goldMultiplier = 1f;
    private float regenAmount = 0f;

    private void Start()
    {
        ApplyVariantStats();
        currentHealth = baseMaxHealth * healthMultiplier;
        
        // Aplicar color visual
        EnemyColorizer colorizer = GetComponent<EnemyColorizer>();
        if (colorizer != null) colorizer.ApplyColor(variant);
    }

    private void Update()
    {
        if (regenAmount > 0 && currentHealth < baseMaxHealth * healthMultiplier)
        {
            currentHealth += regenAmount * Time.deltaTime;
        }
    }

    private void ApplyVariantStats()
    {
        switch (variant)
        {
            case EnemyVariantType.Normal:
                healthMultiplier = 1f;
                expMultiplier = 1f;
                goldMultiplier = 1f;
                break;
            case EnemyVariantType.Verde:
                healthMultiplier = 1.5f;
                expMultiplier = 3f;
                goldMultiplier = 3f;
                regenAmount = 2f; // Regeneración moderada
                break;
            case EnemyVariantType.Amarilla:
                healthMultiplier = 2f;
                expMultiplier = 2f;
                goldMultiplier = 10f; // Especializado en Oro
                break;
            case EnemyVariantType.Azul:
                healthMultiplier = 2f;
                expMultiplier = 10f; // Especializado en Exp
                goldMultiplier = 2f;
                break;
            case EnemyVariantType.Morada:
                healthMultiplier = 3f;
                expMultiplier = 4f;
                goldMultiplier = 4f;
                // Aquí se podría añadir lógica de anti-knockback
                break;
            case EnemyVariantType.Negra:
                healthMultiplier = 9f;
                expMultiplier = 15f;
                goldMultiplier = 15f;
                break;
            case EnemyVariantType.Roja:
                healthMultiplier = 27f;
                expMultiplier = 100f;
                goldMultiplier = 100f;
                regenAmount = 10f; // Regeneración alta (nerfeada)
                break;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        StartCoroutine(HitFlashCoroutine());
        if (currentHealth <= 0) Die();
    }

    private System.Collections.IEnumerator HitFlashCoroutine()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = new Color(2f, 0f, 0f, 1f); 
            yield return new WaitForSeconds(0.15f); // Increased duration (3x)
            sr.color = originalColor;
        }
    }

    private void Die()
    {
        CalculateAndDropRewards();
        Destroy(gameObject); 
    }

    private void CalculateAndDropRewards()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // Fórmula Base: 1 + Nivel + (Tiempo / 60)
        float baseDrop = 1 + gm.GetCurrentLevel() + (gm.GetElapsedTime() / 60f);

        int totalExp = Mathf.RoundToInt(baseDrop * expMultiplier);
        int totalGold = Mathf.RoundToInt(baseDrop * goldMultiplier);

        // Notificar al Inventario para objetos evolutivos
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            InventoryManager inv = player.GetComponent<InventoryManager>();
            if (inv != null) inv.OnEnemyKilled();
        }

        // Spawn EXP Gems
        DropTieredPickups(gemPrefabs, totalExp, true);
        
        // Spawn Money
        DropTieredPickups(moneyPrefabs, totalGold, false);
    }

    private void DropTieredPickups(GameObject[] prefabs, int totalAmount, bool isExp)
    {
        if (prefabs == null || prefabs.Length == 0 || totalAmount <= 0) return;

        int tier = 0;
        if (variant == EnemyVariantType.Roja || variant == EnemyVariantType.Negra) tier = 4;
        else if (variant == EnemyVariantType.Morada) tier = 3;
        else if (variant == EnemyVariantType.Azul || variant == EnemyVariantType.Amarilla) tier = 2;
        else if (variant == EnemyVariantType.Verde) tier = 1;
        
        tier = Mathf.Clamp(tier, 0, prefabs.Length - 1);
        
        GameObject go = Instantiate(prefabs[tier], transform.position, Quaternion.identity);
        if (isExp)
        {
            var exp = go.GetComponent<ExperienceGem>();
            if (exp != null) exp.SetAmount(totalAmount);
        }
        else
        {
            var money = go.GetComponent<MoneyPickup>();
            if (money != null) money.SetAmount(totalAmount);
        }
    }
}
