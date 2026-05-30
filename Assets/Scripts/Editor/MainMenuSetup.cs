using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Genera el menú principal completo con UI visual lista
/// Menú: Tools > Setup Main Menu
/// </summary>
public class MainMenuSetup : EditorWindow
{
    [MenuItem("Tools/Setup Main Menu")]
    public static void ShowWindow()
    {
        GetWindow<MainMenuSetup>("Main Menu Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Crear Menú Principal", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generar Menú Principal Completo", GUILayout.Height(40)))
        {
            CreateMainMenu();
        }

        GUILayout.Space(10);
        GUILayout.Label("Esto creará:", EditorStyles.helpBox);
        GUILayout.Label("• Canvas con menú principal");
        GUILayout.Label("• Botones: Jugar, Ajustes, Salir");
        GUILayout.Label("• Panel de ajustes con opciones de pantalla");
        GUILayout.Label("• Sliders de volumen (visual)");
    }

    private static void CreateMainMenu()
    {
        // Crear Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Crear fondo oscuro
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Título del juego
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "ASTRAL SWARM";
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.8f, 0.9f, 1f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(800, 150);

        // Contenedor de botones principales
        GameObject buttonsContainer = new GameObject("MainButtons");
        buttonsContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonsRect.sizeDelta = new Vector2(400, 400);

        // Botón Jugar
        CreateMenuButton(buttonsContainer.transform, "PlayButton", "JUGAR", new Vector2(0, 80), new Color(0.2f, 0.8f, 0.3f));

        // Botón Ajustes
        CreateMenuButton(buttonsContainer.transform, "SettingsButton", "AJUSTES", new Vector2(0, -20), new Color(0.3f, 0.6f, 0.9f));

        // Botón Salir
        CreateMenuButton(buttonsContainer.transform, "QuitButton", "SALIR", new Vector2(0, -120), new Color(0.9f, 0.3f, 0.3f));

        // Panel de Ajustes (oculto por defecto)
        CreateSettingsPanel(canvasObj.transform);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Main Menu");

        Debug.Log("[MainMenu] ✅ Menú principal creado!");
        EditorUtility.DisplayDialog("Menú Creado",
            "El menú principal se ha creado correctamente.\n\n" +
            "Próximo paso:\n" +
            "- Conecta los botones a la lógica (LoadScene, Quit, etc.)\n" +
            "- Opcional: Reemplaza Image por Sprites del UI pack",
            "OK");
    }

    private static GameObject CreateMenuButton(Transform parent, string name, string text, Vector2 position, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        // RectTransform
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(350, 70);

        // Button
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;

        // Texto del botón
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 36;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    private static void CreateSettingsPanel(Transform parent)
    {
        // Panel principal (oculto por defecto)
        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(parent, false);
        panel.SetActive(false); // Oculto por defecto

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Fondo semi-transparente
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // Ventana de ajustes
        GameObject window = new GameObject("Window");
        window.transform.SetParent(panel.transform, false);
        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(700, 500);

        Image windowBg = window.AddComponent<Image>();
        windowBg.color = new Color(0.15f, 0.15f, 0.2f);

        // Título "Ajustes"
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(window.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "AJUSTES";
        title.fontSize = 48;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 60);

        // Sección: Modo de Pantalla
        CreateLabel(window.transform, "ScreenModeLabel", "Modo de Pantalla:", new Vector2(-200, -120), 32);

        // Botones de modo de pantalla
        CreateToggleButton(window.transform, "WindowedButton", "Ventana", new Vector2(-200, -180), new Color(0.3f, 0.5f, 0.7f));
        CreateToggleButton(window.transform, "BorderlessButton", "Sin Borde", new Vector2(0, -180), new Color(0.3f, 0.5f, 0.7f));
        CreateToggleButton(window.transform, "FullscreenButton", "Pantalla Completa", new Vector2(200, -180), new Color(0.3f, 0.5f, 0.7f));

        // Sección: Volumen
        CreateLabel(window.transform, "VolumeLabel", "Volumen:", new Vector2(-250, -260), 32);

        // Slider Volumen General
        CreateVolumeSlider(window.transform, "MasterVolumeSlider", "General:", new Vector2(0, -300));

        // Slider Volumen Música
        CreateVolumeSlider(window.transform, "MusicVolumeSlider", "Música:", new Vector2(0, -350));

        // Slider Volumen SFX
        CreateVolumeSlider(window.transform, "SFXVolumeSlider", "Efectos:", new Vector2(0, -400));

        // Botón Cerrar
        GameObject closeButton = CreateMenuButton(window.transform, "CloseButton", "VOLVER", new Vector2(0, -220), new Color(0.6f, 0.6f, 0.6f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchoredPosition = new Vector2(0, -220);
        closeRect.anchorMin = new Vector2(0.5f, 0);
        closeRect.anchorMax = new Vector2(0.5f, 0);
        closeRect.sizeDelta = new Vector2(250, 60);
    }

    private static void CreateLabel(Transform parent, string name, string text, Vector2 position, int fontSize)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(parent, false);
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;

        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);
    }

    private static void CreateToggleButton(Transform parent, string name, string text, Vector2 position, Color color)
    {
        GameObject buttonObj = CreateMenuButton(parent, name, text, position, color);
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(180, 50);
    }

    private static void CreateVolumeSlider(Transform parent, string name, string labelText, Vector2 position)
    {
        // Contenedor
        GameObject container = new GameObject(name + "_Container");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(500, 40);

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.Left;
        label.color = Color.white;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(60, 0);
        labelRect.sizeDelta = new Vector2(100, 40);

        // Slider
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(container.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.3f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = new Vector2(0, 30);

        // Background del slider
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fill = fillObj.AddComponent<Image>();
        fill.color = new Color(0.2f, 0.7f, 0.3f);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;

        // Handle Slide Area
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleAreaObj.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        // Handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        Image handle = handleObj.AddComponent<Image>();
        handle.color = Color.white;
        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);

        slider.targetGraphic = handle;
        slider.handleRect = handleRect;
    }
}
