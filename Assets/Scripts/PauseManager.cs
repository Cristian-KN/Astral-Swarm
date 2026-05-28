using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Settings")]
    public Slider masterVolumeSlider;
    public Dropdown windowModeDropdown;

    private bool isPaused = false;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (windowModeDropdown != null)
        {
            windowModeDropdown.ClearOptions();
            windowModeDropdown.AddOptions(new List<string> { "Ventana", "Pantalla Completa", "Sin Bordes" });
            windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                CloseSettings();
            else
                TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused) gameManager?.PauseGame();
        else          gameManager?.ResumeGame();

        if (pausePanel != null) pausePanel.SetActive(isPaused);
        if (!isPaused && settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame() => TogglePause();

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void SetMasterVolume(float v)
    {
        AudioListener.volume = v;
    }

    private void OnWindowModeChanged(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            case 1: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 2: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
        }
    }
}
