using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("Configuración Base")]
    public EnemyArchetype archetype;
    public EnemyVariantType variant;

    [Header("Estadísticas Finales (Calculadas)")]
    public float currentHealth;
    public float maxHealth;
    public float moveSpeed;
    public float damage;
    public int experienceDrop;
    public int goldDrop;

    private EnemyColorizer colorizer;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeEnemy();
    }

    public void InitializeEnemy()
    {
        // 1. Obtener valores base del arquetipo
        float baseHp = 100;
        float baseSpeed = 3f;
        float baseDamage = 10f;
        float visualScale = 1f;
        int baseExp = 10;
        int baseGold = 5;

        switch (archetype)
        {
            case EnemyArchetype.Fast: 
                baseHp = 50; baseSpeed = 5.5f; baseDamage = 5; visualScale = 0.8f; break;
            case EnemyArchetype.Ranged: 
                baseHp = 80; baseSpeed = 2f; baseDamage = 15; visualScale = 1f; break;
            case EnemyArchetype.Tank: 
                baseHp = 400; baseSpeed = 1.5f; baseDamage = 25; visualScale = 1.6f; break;
            case EnemyArchetype.Boss: 
                baseHp = 2000; baseSpeed = 2.5f; baseDamage = 50; visualScale = 3f; break;
        }

        // 2. Aplicar multiplicadores de variante
        float statMult = GetVariantMultiplier(variant);
        
        maxHealth = baseHp * statMult;
        currentHealth = maxHealth;
        damage = baseDamage * statMult;
        
        // La velocidad solo aumenta significativamente en la variante Amarilla
        moveSpeed = baseSpeed * (variant == EnemyVariantType.Amarilla ? 1.5f : 1.0f);

        // 3. Calcular Drops según la tabla de EnemyVariants.md
        CalculateDrops(baseExp, baseGold);

        // 4. Aplicar transformaciones visuales
        transform.localScale = Vector3.one * visualScale;
        ApplyVariantColor();
    }

    float GetVariantMultiplier(EnemyVariantType v)
    {
        switch (v) {
            case EnemyVariantType.Verde:    return 1.5f;
            case EnemyVariantType.Amarilla: case EnemyVariantType.Azul: return 2f;
            case EnemyVariantType.Morada:   return 3f;
            case EnemyVariantType.Negra:    return 9f;
            case EnemyVariantType.Roja:     return 27f;
            default: return 1f;
        }
    }

    void CalculateDrops(int bExp, int bGold)
    {
        switch (variant)
        {
            case EnemyVariantType.Verde:    experienceDrop = bExp * 3; goldDrop = bGold * 3; break;
            case EnemyVariantType.Amarilla: experienceDrop = bExp * 2; goldDrop = bGold * 10; break;
            case EnemyVariantType.Azul:     experienceDrop = bExp * 10; goldDrop = bGold * 2; break;
            case EnemyVariantType.Morada:   experienceDrop = bExp * 4; goldDrop = bGold * 4; break;
            case EnemyVariantType.Negra:    experienceDrop = bExp * 15; goldDrop = bGold * 15; break;
            case EnemyVariantType.Roja:     experienceDrop = bExp * 100; goldDrop = bGold * 100; break;
            default:                        experienceDrop = bExp; goldDrop = bGold; break;
        }
    }

    void ApplyVariantColor() 
    { 
        if (colorizer == null) colorizer = GetComponent<EnemyColorizer>();
        if (colorizer != null)
        {
            colorizer.ApplyColor(variant);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // Notificar al inventario para items evolutivos
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null) inv.OnEnemyKilled();

        Destroy(gameObject);
    }
}