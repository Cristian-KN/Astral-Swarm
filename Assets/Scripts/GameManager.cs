using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Progresión del Jugador")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int currentGold = 0;
    public int expToNextLevel = 100;
    public float expScalingFactor = 1.5f;

    [Header("Tiempo de Partida")]
    public float timeRemaining;
    public float timeToSurvive = 180f;
    private float elapsedTime = 0f;

    [Header("Estado del Juego")]
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("Pool de Items para Level Up")]
    [SerializeField] private List<ItemData> itemPool = new List<ItemData>();

    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        timeRemaining = timeToSurvive;

        if (uiManager != null)
        {
            uiManager.UpdateLevelText(currentLevel);
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
            uiManager.UpdateTimer(timeRemaining);
            uiManager.UpdateGold(currentGold);
        }
    }

    private void Update()
    {
        if (isGameOver || isPaused) return;

        timeRemaining -= Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (uiManager != null)
            uiManager.UpdateTimer(timeRemaining);

        if (timeRemaining <= 0)
            TriggerVictory();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        uiManager?.UpdateGold(currentGold);
    }
    public float GetElapsedTime() => elapsedTime;
    public int GetCurrentLevel() => currentLevel;

    public void AddExperience(int amount)
    {
        if (isGameOver || isPaused) return;

        currentExp += amount;

        if (currentExp >= expToNextLevel)
            LevelUp();

        if (uiManager != null)
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expScalingFactor);

        if (uiManager != null)
        {
            uiManager.UpdateLevelText(currentLevel);
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
            ItemData[] choices = GenerateLevelUpChoices();
            uiManager.ShowLevelUpChoices(choices, OnItemChosen);
        }

        PauseGame();
    }

    private ItemData[] GenerateLevelUpChoices()
    {
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv == null) return new ItemData[0];

        List<ItemData> candidates = new List<ItemData>();
        var owned = inv.GetOwnedItems();

        // 1. Upgrades and Evolutions of owned items
        foreach (var item in owned)
        {
            if (item.nextRarityUpgrade != null) candidates.Add(item.nextRarityUpgrade);
            
            if (item.evolutionTarget != null)
            {
                // Evolution requirement: owning the required passive (Megabonk)
                if (item.requiredPassive == null || owned.Contains(item.requiredPassive))
                {
                    candidates.Add(item.evolutionTarget);
                }
            }
        }

        // 2. New items from pool (if slots available)
        foreach (var pItem in itemPool)
        {
            if (owned.Contains(pItem)) continue; // Already have this version

            bool canAdd = false;
            switch (pItem.type)
            {
                case ItemType.Weapon: if (inv.HasWeaponSpace()) canAdd = true; break;
                case ItemType.ActiveSkill: if (inv.HasActiveSpace()) canAdd = true; break;
                case ItemType.Passive:
                case ItemType.Growth: canAdd = true; break;
            }

            if (canAdd)
            {
                // Only allow adding the "Common" or "Starting" version from pool
                // Higher rarities are reached through upgrades
                if (pItem.rarity == ItemRarity.Common)
                {
                    candidates.Add(pItem);
                }
            }
        }

        // 3. Select 3 unique random choices
        List<ItemData> choices = new List<ItemData>();
        candidates = candidates.Distinct().ToList(); // Remove duplicates
        
        int count = Mathf.Min(3, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            choices.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        return choices.ToArray();
    }

    private void OnItemChosen(ItemData chosen)
    {
        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
        {
            // Check if this is an upgrade for an existing item
            var owned = inv.GetOwnedItems();
            ItemData baseItem = owned.Find(o => o.nextRarityUpgrade == chosen || o.evolutionTarget == chosen);
            
            if (baseItem != null)
            {
                inv.UpgradeItem(baseItem, chosen);
            }
            else
            {
                inv.AddItem(chosen);
            }
        }
        ResumeGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (uiManager != null)
            uiManager.ShowLevelUpMenu(false);
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        PauseGame();
        if (uiManager != null) uiManager.ShowGameOver();
    }

    public void TriggerVictory()
    {
        isGameOver = true;
        PauseGame();
        if (uiManager != null) uiManager.ShowVictory();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
