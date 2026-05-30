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

        // Reset temporal de multiplicadores para recalcular
        playerStats.attackMultiplier = 1f;
        playerStats.speedMultiplier = 1f;
        playerStats.difficulty = 0f;
        playerStats.defense = 0f;

        foreach (var item in items)
        {
            playerStats.attackPower += item.data.attackBoost * item.stacks;
            playerStats.defense     += item.data.defenseBoost * item.stacks;
            playerStats.difficulty  += item.data.difficultyIncrease * item.stacks;
            playerStats.speedMultiplier += item.data.speedBoost * item.stacks;

            if (item.data.statMultiplier > 1f)
            {
                playerStats.attackMultiplier *= item.data.statMultiplier * item.stacks;
            }

            // Aplicar crecimiento
            if (item.data.isGrowthItem)
            {
                playerStats.attackPower += item.currentGrowthBonus;
                // Si es un mítico raro, podría subir todo
                if (item.data.rarity == ItemRarity.Mythic)
                {
                     playerStats.defense += item.currentGrowthBonus * 0.1f;
                }
            }
        }
    }
}
