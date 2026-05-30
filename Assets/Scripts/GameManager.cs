using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    private HUDController hud;

    private void Start()
    {
        hud = FindObjectOfType<HUDController>();
        timeRemaining = timeToSurvive;

        if (hud != null)
        {
            hud.UpdateLevelText(currentLevel);
            hud.UpdateExperienceBar(currentExp, expToNextLevel);
            hud.UpdateTimer(timeRemaining);
            hud.UpdateGold(currentGold);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            bool levelingUp = hud != null && hud.IsLevelUpOpen;

            if (!levelingUp)
            {
                if (isPaused) ResumeGame();
                else { PauseGame(); hud?.ShowPauseMenu(true); }
            }
        }

        if (isGameOver || isPaused) return;

        timeRemaining -= Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (hud != null)
            hud.UpdateTimer(elapsedTime); // Display elapsed time instead of remaining

        if (timeRemaining <= 0)
            TriggerVictory();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        hud?.UpdateGold(currentGold);
    }
    public float GetElapsedTime() => elapsedTime;
    public int GetCurrentLevel() => currentLevel;

    public void AddExperience(int amount)
    {
        if (isGameOver || isPaused) return;

        currentExp += amount;

        if (currentExp >= expToNextLevel)
            LevelUp();

        if (hud != null)
            hud.UpdateExperienceBar(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expScalingFactor);

        if (hud != null)
        {
            hud.UpdateLevelText(currentLevel);
            hud.UpdateExperienceBar(currentExp, expToNextLevel);
        }

        ItemData[] choices = GenerateLevelUpChoices();

        if (hud != null && hud.CanShowLevelUp)
        {
            PauseGame();
            hud.ShowLevelUpChoices(choices, OnItemChosen);
        }
        else if (choices.Length > 0)
        {
            // UI no disponible: auto-elige el primer item sin pausar
            OnItemChosen(choices[0]);
        }
    }

    public ItemData[] RollNewChoices() => GenerateLevelUpChoices();

    private ItemData[] GenerateLevelUpChoices()
    {
        // Filter out null entries before picking
        List<ItemData> pool = itemPool.FindAll(item => item != null);
        List<ItemData> choices = new List<ItemData>();
        int count = Mathf.Min(3, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return choices.ToArray();
    }

    private void OnItemChosen(ItemData chosen)
    {
        if (chosen == null) { ResumeGame(); return; }

        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv != null)
        {
            if (chosen.type == ItemType.Weapon)
                inv.AddWeapon(chosen);
            else
                inv.AddItem(chosen);
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
        if (hud != null)
        {
            hud.ShowLevelUpMenu(false);
            hud.ShowPauseMenu(false);
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        PauseGame();
        if (hud != null) hud.ShowGameOver();
    }

    public void TriggerVictory()
    {
        isGameOver = true;
        PauseGame();
        if (hud != null) hud.ShowVictory();
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
