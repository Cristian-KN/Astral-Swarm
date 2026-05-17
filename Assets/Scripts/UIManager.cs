using UnityEngine;
using UnityEngine.UI;
// using TMPro; // Descomentar si usas TextMeshPro, que es lo moderno en Unity

public class UIManager : MonoBehaviour
{
    [Header("UI del HUD")]
    [Tooltip("Barra superior de Experiencia")]
    public Slider xpSlider;
    
    [Tooltip("Texto del Nivel Actual (Ej: LVL 5)")]
    public Text levelText; 
    
    [Tooltip("Reloj de Supervivencia")]
    public Text timerText;

    [Header("Paneles de Menús (Fase 6)")]
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private void Start()
    {
        // Nos aseguramos que al inicio los menús estén ocultos
        if (levelUpPanel) levelUpPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Actualiza la barra azul de exp a medida que recogemos gemas
    /// </summary>
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
        {
            levelText.text = "LVL " + newLevel.ToString();
        }
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            // Formatear el tiempo en Minutos:Segundos
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ShowLevelUpMenu(bool show)
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(show);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }
}
