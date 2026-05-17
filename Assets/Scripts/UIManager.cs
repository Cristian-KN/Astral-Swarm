using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public Slider xpSlider;
    public Text levelText;
    public Text timerText;

    [Header("Paneles")]
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Pause Menu")]
    public GameObject pausePanel;

    [Header("Cartas de Level Up (3 botones)")]
    public Button[] levelUpCards;
    public Text[] cardNameTexts;
    public Text[] cardDescTexts;

    private void Start()
    {
        if (levelUpPanel) levelUpPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void UpdateExperienceBar(int currentXp, int xpToNextLevel)
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXp;
        }
    }

    public void UpdateLevelText(int newLevel)
    {
        if (levelText != null)
            levelText.text = "LVL " + newLevel;
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ShowLevelUpMenu(bool show)
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(show);
    }

    /// <summary>
    /// Popula las 3 cartas con los datos del item y conecta el callback de selección.
    /// </summary>
    public void ShowLevelUpChoices(ItemData[] choices, System.Action<ItemData> onChosen)
    {
        ShowLevelUpMenu(true);

        if (levelUpCards == null) return;

        for (int i = 0; i < levelUpCards.Length; i++)
        {
            if (levelUpCards[i] == null) continue;

            bool hasChoice = i < choices.Length && choices[i] != null;
            levelUpCards[i].gameObject.SetActive(hasChoice);

            if (!hasChoice) continue;

            ItemData item = choices[i];
            if (cardNameTexts != null && i < cardNameTexts.Length && cardNameTexts[i] != null)
                cardNameTexts[i].text = item.itemName;
            if (cardDescTexts != null && i < cardDescTexts.Length && cardDescTexts[i] != null)
                cardDescTexts[i].text = item.description;

            levelUpCards[i].onClick.RemoveAllListeners();
            // Capture loop variable
            ItemData captured = item;
            levelUpCards[i].onClick.AddListener(() => onChosen(captured));
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }
}
