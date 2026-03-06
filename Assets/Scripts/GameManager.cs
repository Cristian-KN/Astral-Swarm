using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Progresión del Jugador")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;
    [Tooltip("Multiplicador para hacer más difícil subir de nivel cada vez.")]
    public float expScalingFactor = 1.5f;

    [Header("Tiempo de Partida")]
    [Tooltip("Tiempo en segundos que el jugador debe sobrevivir (Ej: 180s = 3 mins)")]
    public float timeToSurvive = 180f;
    private float timeRemaining;

    [Header("Estado del Juego")]
    public bool isGameOver = false;
    public bool isPaused = false;

    // Referencias
    private UIManager uiManager;
    private PlayerStats playerStats;

    private void Start()
    {
        // Buscar el UIManager en la escena
        uiManager = FindObjectOfType<UIManager>();
        timeRemaining = timeToSurvive;

        // Actualizar UI inicial
        if (uiManager != null)
        {
            uiManager.UpdateLevelText(currentLevel);
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
            uiManager.UpdateTimer(timeRemaining);
        }
    }

    private void Update()
    {
        // Si el juego está en pausa, victoria o game over, no avanza el tiempo
        if (isGameOver || isPaused) return;

        // Bajar el temporizador
        timeRemaining -= Time.deltaTime;
        
        // Actualizar UI del reloj
        if (uiManager != null)
        {
            uiManager.UpdateTimer(timeRemaining);
        }

        // ¿Sobrevivió el tiempo necesario?
        if (timeRemaining <= 0)
        {
            TriggerVictory();
        }
    }

    /// <summary>
    /// Invocado por ExperienceGem.cs cuando el jugador la recoge.
    /// </summary>
    public void AddExperience(int amount)
    {
        if (isGameOver || isPaused) return;

        currentExp += amount;

        // Comprobamos si subimos de nivel
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        // Notificamos a la UI
        if (uiManager != null)
        {
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel; // Guardamos el excedente
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expScalingFactor); // Aumentamos requisito

        if (uiManager != null)
        {
            uiManager.UpdateLevelText(currentLevel);
            uiManager.UpdateExperienceBar(currentExp, expToNextLevel);
            uiManager.ShowLevelUpMenu(true);
        }

        PauseGame(); // Pausa para elegir la mejora
    }

    // ==============================================
    // CONTROL DE ESTADO GLOBAL (PAUSA Y FLUJOS)
    // ==============================================

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Congela el motor de físicas y el Update()
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (uiManager != null) uiManager.ShowLevelUpMenu(false);
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        PauseGame(); // Congelar pantalla
        if (uiManager != null) uiManager.ShowGameOver();
    }

    public void TriggerVictory()
    {
        isGameOver = true;
        PauseGame(); // Congelar pantalla
        if (uiManager != null) uiManager.ShowVictory();
    }

    // Funciones para botones de UI (Jugar de nuevo, Volver al menú)
    public void RestartGame()
    {
        Time.timeScale = 1f; // Restaurar el tiempo antes de recargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Asumiendo que nombrarán su escena 0órica como "MainMenu"
    }
}
