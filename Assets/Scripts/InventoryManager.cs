using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class EquippedItem
    {
        public ItemData data;
        public int stacks = 1;
        public int killCount = 0;
        public float currentGrowthBonus = 0f;
    }

    public static System.Action onInventoryChanged;

    public List<EquippedItem> items = new List<EquippedItem>();
    public PlayerClass playerClass;
    private PlayerStats playerStats;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        string selectedClass = PlayerPrefs.GetString("SelectedClass", "warrior").ToLower();
        switch (selectedClass)
        {
            case "warrior": playerClass = PlayerClass.Warrior; break;
            case "archer": playerClass = PlayerClass.Archer; break;
            case "lancer": playerClass = PlayerClass.Lancer; break;
        }
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    public void AddWeapon(ItemData weaponData)
    {
        playerAttack?.EquipWeapon(weaponData);
        onInventoryChanged?.Invoke();
    }

    public void AddItem(ItemData newData)
    {
        // Buscar si ya lo tenemos para stackear
        EquippedItem existing = items.Find(i => i.data == newData);
        if (existing != null)
        {
            existing.stacks++;
        }
        else
        {
            EquippedItem newItem = new EquippedItem { data = newData };
            items.Add(newItem);
        }

        ApplyAllStats();
        onInventoryChanged?.Invoke();
    }

    // Llamado por el EnemyStats cuando muere
    public void OnEnemyKilled()
    {
        foreach (var item in items)
        {
            if (item.data.isGrowthItem)
            {
                item.killCount++;
                // La tasa de crecimiento se suma por cada stack
                float rate = item.data.growthPerKill * item.stacks;
                item.currentGrowthBonus += rate;
            }
        }
        
        ApplyAllStats();
    }

    public void ApplyAllStats()
    {
        if (playerStats == null) return;

        float oldAtk = playerStats.attackPower;
        float oldAtkSpeed = playerStats.attackSpeed;
        float oldRange = playerStats.attackRange;
        float oldSpeed = playerStats.speedMultiplier;
        float oldLuck = playerStats.luck;
        float oldDiff = playerStats.difficulty;

        // Reset to base values
        playerStats.attackPower = playerStats.baseAttackPower;
        playerStats.attackSpeed = playerStats.baseAttackSpeed;
        playerStats.attackRange = playerStats.baseAttackRange;
        playerStats.luck = playerStats.baseLuck;
        playerStats.defense = playerStats.baseDefense;
        playerStats.difficulty = playerStats.baseDifficulty;
        playerStats.attackMultiplier = 1f;
        playerStats.speedMultiplier = 1f;

        foreach (var item in items)
        {
            playerStats.attackPower += item.data.attackBoost * item.stacks;
            playerStats.attackSpeed += item.data.attackSpeedBoost * item.stacks;
            playerStats.attackRange += item.data.rangeBoost * item.stacks;
            playerStats.speedMultiplier += item.data.speedBoost * item.stacks;
            playerStats.luck         += item.data.luckBoost * item.stacks;
            playerStats.defense      += item.data.defenseBoost * item.stacks;
            playerStats.difficulty   += item.data.difficultyIncrease * item.stacks;

            if (item.data.statMultiplier > 1f)
            {
                // Use exponential stacking: base^stacks, not linear multiplication
                playerStats.attackMultiplier *= Mathf.Pow(item.data.statMultiplier, item.stacks);
            }

            // Aplicar crecimiento
            if (item.data.isGrowthItem)
            {
                playerStats.attackPower += item.currentGrowthBonus;
                if (item.data.rarity == ItemRarity.Mythic)
                {
                     playerStats.defense += item.currentGrowthBonus * 0.1f;
                }
            }
        }

        // Notify changes
        playerStats.NotifyStatChange("attack", playerStats.attackPower, playerStats.attackPower - oldAtk);
        playerStats.NotifyStatChange("attack-speed", playerStats.attackSpeed, playerStats.attackSpeed - oldAtkSpeed);
        playerStats.NotifyStatChange("range", playerStats.attackRange, playerStats.attackRange - oldRange);
        playerStats.NotifyStatChange("speed", playerStats.speedMultiplier, playerStats.speedMultiplier - oldSpeed);
        playerStats.NotifyStatChange("luck", playerStats.luck, playerStats.luck - oldLuck);
        playerStats.NotifyStatChange("difficulty", playerStats.difficulty, playerStats.difficulty - oldDiff);
    }
}
