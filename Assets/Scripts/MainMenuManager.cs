using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Window Mode")]
    public Dropdown windowModeDropdown;

    [Header("Animation")]
    [SerializeField] private float transitionTime = 0.25f;

    private void Start()
    {
        if (mainPanel != null) { mainPanel.SetActive(true); mainPanel.transform.localScale = Vector3.one; }
        if (settingsPanel != null) { settingsPanel.SetActive(false); settingsPanel.transform.localScale = Vector3.zero; }
        
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);

        if (windowModeDropdown != null)
        {
            windowModeDropdown.ClearOptions();
            windowModeDropdown.AddOptions(new System.Collections.Generic.List<string>
                { "Ventana", "Pantalla Completa", "Sin Bordes" });
            windowModeDropdown.onValueChanged.AddListener(idx => {
                switch (idx) {
                    case 0: SetWindowed();   break;
                    case 1: SetFullscreen(); break;
                    case 2: SetBorderless(); break;
                }
            });
        }
    }

    public void PlayGame() => SceneManager.LoadScene("Game");

    public void ShowSettings() => StartCoroutine(Transition(mainPanel, settingsPanel));
    public void HideSettings() => StartCoroutine(Transition(settingsPanel, mainPanel));

    private IEnumerator Transition(GameObject from, GameObject to)
    {
        float elapsed = 0;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;

        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionTime;
            if (from != null) from.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        if (from != null) from.SetActive(false);
        if (to != null)
        {
            to.SetActive(true);
            elapsed = 0;
            while (elapsed < transitionTime)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionTime;
                to.transform.localScale = Vector3.Lerp(endScale, startScale, t);
                yield return null;
            }
            to.transform.localScale = Vector3.one;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SetFullscreen() => Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    public void SetBorderless() => Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    public void SetWindowed() => Screen.fullScreenMode = FullScreenMode.Windowed;

    public void SetMasterVolume(float v) => AudioListener.volume = v;
    public void SetMusicVolume(float v) => Debug.Log("Music: " + v);
    public void SetSfxVolume(float v) => Debug.Log("SFX: " + v);
}
