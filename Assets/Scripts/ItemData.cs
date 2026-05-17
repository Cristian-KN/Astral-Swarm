using UnityEngine;

public enum ItemType { Weapon, Stat, Pet, Growth }

[CreateAssetMenu(fileName = "NewItem", menuName = "Astral Swarm/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public ItemType type;
    public EnemyVariantType rarity = EnemyVariantType.Normal; // Usamos el mismo enum para simplicidad
    
    [Header("Stat Boosts (Flat)")]
    public float attackBoost = 0;
    public float speedBoost = 0;
    public float defenseBoost = 0;
    
    [Header("Multipliers")]
    public float statMultiplier = 1f;

    [Header("Sacrifice / Difficulty")]
    public float difficultyIncrease = 0f; // Los items de Sacrificio aumentan esto

    [Header("Growth Stats")]
    public bool isGrowthItem = false;
    public float growthPerKill = 0f;
    public float growthMultiStatFactor = 1f; // Para los míticos raros que suben todo
}
