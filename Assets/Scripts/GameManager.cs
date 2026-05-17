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
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            bool levelingUp = uiManager != null
                && uiManager.levelUpPanel != null
                && uiManager.levelUpPanel.activeSelf;

            if (!levelingUp)
            {
                if (isPaused) ResumeGame();
                else { PauseGame(); uiManager?.ShowPauseMenu(true); }
            }
        }

        if (isGameOver || isPaused) return;

        timeRemaining -= Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (uiManager != null)
            uiManager.UpdateTimer(timeRemaining);

        if (timeRemaining <= 0)
            TriggerVictory();
    }

    public void AddGold(int amount) => currentGold += amount;
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
        List<ItemData> pool = new List<ItemData>(itemPool);
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
        if (uiManager != null)
        {
            uiManager.ShowLevelUpMenu(false);
            uiManager.ShowPauseMenu(false);
        }
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
