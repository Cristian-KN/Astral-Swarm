using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BiomeUIDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI biomeNameText;
    [SerializeField] private Image biomeProgressBar;
    [SerializeField] private TextMeshProUGUI biomeTimerText;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private Image warningFlashImage;

    [Header("Colores")]
    [SerializeField] private Color normalBiomeColor = Color.white;
    [SerializeField] private Color specialBiomeColor = new Color(1f, 0.84f, 0f); // Dorado
    [SerializeField] private Color warningFlashColor = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Header("Configuración")]
    [SerializeField] private bool showTimer = true;
    [SerializeField] private bool showProgressBar = true;

    private BiomeManager biomeManager;
    private bool isWarningActive = false;
    private float warningFlashTimer = 0f;

    private void Start()
    {
        biomeManager = BiomeManager.Instance;

        if (biomeManager != null)
        {
            biomeManager.OnBiomeChange += HandleBiomeChange;
            biomeManager.OnBiomeWarning += HandleBiomeWarning;
        }

        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    private void Update()
    {
        if (biomeManager == null) return;

        UpdateBiomeUI();

        if (isWarningActive)
        {
            UpdateWarningFlash();
        }
    }

    private void UpdateBiomeUI()
    {
        // Actualizar barra de progreso
        if (biomeProgressBar != null && showProgressBar)
        {
            biomeProgressBar.fillAmount = biomeManager.GetBiomeProgress();
        }

        // Actualizar timer
        if (biomeTimerText != null && showTimer)
        {
            float timeRemaining = biomeManager.GetBiomeTimeRemaining();
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            biomeTimerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void HandleBiomeChange(BiomeData newBiome, bool isSpecial)
    {
        // Actualizar nombre del bioma
        if (biomeNameText != null)
        {
            biomeNameText.text = newBiome.displayName;
            biomeNameText.color = isSpecial ? specialBiomeColor : normalBiomeColor;

            // Animación de entrada (simple scale pulse)
            LeanTween.cancel(biomeNameText.gameObject);
            biomeNameText.transform.localScale = Vector3.one * 1.5f;
            LeanTween.scale(biomeNameText.gameObject, Vector3.one, 0.5f).setEaseOutBack();
        }

        // Mostrar notificación temporal
        ShowBiomeNotification(newBiome, isSpecial);

        // Desactivar warning
        isWarningActive = false;
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    private void HandleBiomeWarning(float timeRemaining)
    {
        isWarningActive = true;
        if (warningPanel != null)
            warningPanel.SetActive(true);

        Debug.Log($"[BiomeUI] ⚠️ Cambio de bioma en {timeRemaining:F0}s");
    }

    private void UpdateWarningFlash()
    {
        if (warningFlashImage == null) return;

        warningFlashTimer += Time.deltaTime * 2f;
        float alpha = (Mathf.Sin(warningFlashTimer * Mathf.PI) + 1f) * 0.5f;
        Color c = warningFlashColor;
        c.a = alpha * 0.3f;
        warningFlashImage.color = c;
    }

    private void ShowBiomeNotification(BiomeData biome, bool isSpecial)
    {
        // Crear notificación flotante temporal
        string message = isSpecial
            ? $"⭐ {biome.displayName} ⭐\n+{(biome.expMultiplier - 1f) * 100:F0}% EXP | +{(biome.goldMultiplier - 1f) * 100:F0}% ORO"
            : $"{biome.displayName}\nDificultad: {biome.enemyDifficultyMultiplier:F1}x";

        Debug.Log($"[BiomeUI] 🌍 {message}");

        // TODO: Aquí puedes instanciar un prefab de notificación UI
        // Por ahora solo lo loguea
    }

    private void OnDestroy()
    {
        if (biomeManager != null)
        {
            biomeManager.OnBiomeChange -= HandleBiomeChange;
            biomeManager.OnBiomeWarning -= HandleBiomeWarning;
        }
    }
}
