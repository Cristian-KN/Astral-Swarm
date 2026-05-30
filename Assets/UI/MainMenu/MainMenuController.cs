using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/// <summary>
/// Lógica del menú principal en UI Toolkit (puerto de menu.js del handoff):
/// navegación, sliders, segmentado de pantalla, modal de salida, overlay de
/// transición a juego, cambio de tema (A/B/C), y ambiente animado (flicker + brasas).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    public enum Theme { Forged, Astral, Grimoire }

    [Header("Tema")]
    [SerializeField] private Theme theme = Theme.Forged;

    [Header("Escena de juego")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SfxVolume";

    [Header("UI (lo asigna el setup)")]
    [SerializeField] private PanelSettings panelSettings;

    [Header("Texturas de ambiente (las asigna el setup)")]
    [SerializeField] private Texture2D glowTexture;
    [SerializeField] private Texture2D vignetteTexture;

    [System.Serializable]
    public class AnimationData
    {
        public Sprite[] frames;
    }

    [Header("Character Animations")]
    public List<AnimationData> warriorAnims = new List<AnimationData>();
    public List<AnimationData> archerAnims = new List<AnimationData>();
    public List<AnimationData> lancerAnims = new List<AnimationData>();
    [SerializeField] private float frameDuration = 0.1f;

    private const int EmberCount = 0; // Desactivado - causaba problemas con diferentes resoluciones
    private const string PrefThemeKey = "astral.theme";
    // resolución persistente (compartida con el HUD/ajustes in-game)
    private const string ResWKey = "astral.res.w";
    private const string ResHKey = "astral.res.h";

    private VisualElement _stage;
    private VisualElement _screenMenu, _screenSettings, _screenCharSelect, _modal, _playOverlay;
    private VisualElement _embersHost;
    private List<VisualElement> _dots = new List<VisualElement>();
    private readonly List<Ember> _embers = new List<Ember>();
    private readonly List<GlowFx> _glows = new List<GlowFx>();
    private bool _embersActive;
    private bool _loadingActive;

    // Character Selection State
    private string selectedClass = "warrior";
    private int selectedColorIndex = 0;
    private readonly string[] colorNames = { "blue", "yellow", "red", "purple", "black" };
    private VisualElement[] classCards = new VisualElement[3];
    private VisualElement[] colorCards = new VisualElement[5];
    private VisualElement[] colorPreviews = new VisualElement[5];
    private VisualElement[] classIcons = new VisualElement[3];
    private int currentAnimationFrame = 0;
    private IVisualElementScheduledItem animationTask;

    private struct Ember
    {
        public VisualElement el;
        public float x, y, speed, drift, baseX, life, maxLife, size;
    }
    private struct GlowFx
    {
        public VisualElement el;
        public float seed, speed, baseOpacity;
        public bool breathe;
        public float xN, yN, sizePx;   // centro normalizado (0..1) sobre el fondo + diámetro
    }

    // tamaño nativo del fondo (menu_background.jpeg) — usado para anclar las luces
    // al rectángulo real de la imagen con scale-and-crop a cualquier resolución.
    private const float BgW = 1408f, BgH = 768f;

    // -------------------------------------------------------------------

    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc.panelSettings == null)
        {
            var ps = panelSettings != null ? panelSettings : Resources.Load<PanelSettings>("MainMenuPanelSettings");
            if (ps != null) doc.panelSettings = ps;
        }
        var root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[MainMenu] rootVisualElement es null.");
            return;
        }

        _stage = root.Q<VisualElement>("stage");
        _screenMenu = root.Q<VisualElement>("screen-menu");
        _screenSettings = root.Q<VisualElement>("screen-settings");
        _screenCharSelect = root.Q<VisualElement>("screen-character-select");
        _modal = root.Q<VisualElement>("modal-wrap");
        _playOverlay = root.Q<VisualElement>("play-overlay");
        _embersHost = root.Q<VisualElement>("embers");
        
        _dots.Clear();
        var dotElements = root.Query<VisualElement>(className: "spinner-dot").ToList();
        foreach (var d in dotElements) _dots.Add(d);

        if (PlayerPrefs.HasKey(PrefThemeKey))
        {
            string saved = PlayerPrefs.GetString(PrefThemeKey);
            if (System.Enum.TryParse(saved, out Theme t)) theme = t;
        }
        ApplyTheme();

        SetupAmbient(root);
        WireButtons(root);
        WireSegment(root);
        WireSliders(root);
        InitializeCharSelection(root);

        root.focusable = true;
        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        root.RegisterCallback<NavigationCancelEvent>(_ => OnCancel());
        root.schedule.Execute(() => root.Focus()).ExecuteLater(50);
    }

    private void OnDisable()
    {
        animationTask?.Pause();
    }

    private void InitializeCharSelection(VisualElement root)
    {
        if (_screenCharSelect == null) return;

        classCards[0] = root.Q<VisualElement>("class-warrior");
        classCards[1] = root.Q<VisualElement>("class-archer");
        classCards[2] = root.Q<VisualElement>("class-lancer");

        classIcons[0] = root.Q<VisualElement>("class-icon-warrior");
        classIcons[1] = root.Q<VisualElement>("class-icon-archer");
        classIcons[2] = root.Q<VisualElement>("class-icon-lancer");

        for (int i = 0; i < 5; i++)
        {
            colorCards[i] = root.Q<VisualElement>($"color-{colorNames[i]}");
            colorPreviews[i] = root.Q<VisualElement>($"color-preview-{i}");
        }

        classCards[0]?.RegisterCallback<ClickEvent>(evt => { AudioManager.Instance?.PlayStartButtonSound(); SelectClass("warrior"); });
        classCards[1]?.RegisterCallback<ClickEvent>(evt => { AudioManager.Instance?.PlayStartButtonSound(); SelectClass("archer"); });
        classCards[2]?.RegisterCallback<ClickEvent>(evt => { AudioManager.Instance?.PlayStartButtonSound(); SelectClass("lancer"); });

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            colorCards[i]?.RegisterCallback<ClickEvent>(evt => { AudioManager.Instance?.PlayStartButtonSound(); SelectColor(index); });
        }

        root.Q<VisualElement>("btn-char-back")?.RegisterCallback<ClickEvent>(evt => { AudioManager.Instance?.PlayGenericButtonSound(); CloseCharSelection(); });
        root.Q<VisualElement>("btn-char-play")?.RegisterCallback<ClickEvent>(evt => {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayStartButtonSound();
            PlayGame();
        });

        if (animationTask == null)
        {
            animationTask = root.schedule.Execute(AnimateCharStep).Every((long)(frameDuration * 1000));
        }
    }

    private void SelectClass(string className)
    {
        selectedClass = className;
        UpdateCharUI();
    }

    private void SelectColor(int index)
    {
        selectedColorIndex = index;
        UpdateCharUI();
    }

    private void UpdateCharUI()
    {
        if (_screenCharSelect == null) return;

        classCards[0]?.EnableInClassList("class-card--active", selectedClass == "warrior");
        classCards[1]?.EnableInClassList("class-card--active", selectedClass == "archer");
        classCards[2]?.EnableInClassList("class-card--active", selectedClass == "lancer");

        for (int i = 0; i < colorCards.Length; i++)
        {
            if (colorCards[i] != null)
                colorCards[i].EnableInClassList("color-card--active", i == selectedColorIndex);
        }

        // El cuerpo del lancero ocupa poco de su celda: en los previews de skin
        // necesita más zoom y bajarlo un poco (igual que #class-icon-lancer en USS).
        // El guerrero/arquero usan la escala base (1.45) centrada.
        bool isLancer = selectedClass == "lancer";
        var previewScale = new StyleScale(new Scale(new Vector3(isLancer ? 2.1f : 1.45f, isLancer ? 2.1f : 1.45f, 1f)));
        var previewOffset = new StyleTranslate(isLancer ? new Translate(3f, 6f, 0f) : new Translate(0f, 0f, 0f));
        for (int i = 0; i < colorPreviews.Length; i++)
        {
            if (colorPreviews[i] != null)
            {
                colorPreviews[i].style.scale = previewScale;
                colorPreviews[i].style.translate = previewOffset;
            }
        }
    }

    private void AnimateCharStep()
    {
        if (_screenCharSelect == null || _screenCharSelect.style.display == DisplayStyle.None) return;

        currentAnimationFrame++;
        
        if (warriorAnims.Count > 0) UpdateElementAnim(classIcons[0], warriorAnims[0].frames);
        if (archerAnims.Count > 0) UpdateElementAnim(classIcons[1], archerAnims[0].frames);
        if (lancerAnims.Count > 0) UpdateElementAnim(classIcons[2], lancerAnims[0].frames);

        List<AnimationData> currentAnims = warriorAnims;
        if (selectedClass == "archer") currentAnims = archerAnims;
        else if (selectedClass == "lancer") currentAnims = lancerAnims;

        if (currentAnims != null)
        {
            for (int i = 0; i < colorPreviews.Length; i++)
            {
                if (i < currentAnims.Count && currentAnims[i] != null)
                    UpdateElementAnim(colorPreviews[i], currentAnims[i].frames);
            }
        }
    }

    private void UpdateElementAnim(VisualElement el, Sprite[] frames)
    {
        if (el == null || frames == null || frames.Length == 0) return;
        int frame = currentAnimationFrame % frames.Length;
        if (frames[frame] == null) return; // saltar frames nulos (hojas con sprites no recortados)
        el.style.backgroundImage = new StyleBackground(frames[frame]);
    }

    private void OpenCharSelection()
    {
        if (_screenMenu != null) _screenMenu.style.display = DisplayStyle.None;
        if (_screenCharSelect != null) _screenCharSelect.style.display = DisplayStyle.Flex;
        UpdateCharUI();
    }

    private void CloseCharSelection()
    {
        if (_screenCharSelect != null) _screenCharSelect.style.display = DisplayStyle.None;
        if (_screenMenu != null) _screenMenu.style.display = DisplayStyle.Flex;
    }

    private void PlayGame()
    {
        Debug.Log($"[CharSelect] Iniciando con: {selectedClass} color {colorNames[selectedColorIndex]}");
        PlayerPrefs.SetString("SelectedClass", selectedClass);
        PlayerPrefs.SetString("SelectedColor", colorNames[selectedColorIndex]);
        
        if (_playOverlay != null)
        {
            _playOverlay.style.display = DisplayStyle.Flex;
            _playOverlay.schedule.Execute(() => _playOverlay.AddToClassList("show")).ExecuteLater(16);
            _loadingActive = true;
        }
        StartCoroutine(LoadGameRoutine());
    }

    // ----------------------------- THEME -----------------------------

    private void ApplyTheme()
    {
        if (_stage == null) return;
        _stage.RemoveFromClassList("theme-astral");
        _stage.RemoveFromClassList("theme-forged");
        _stage.RemoveFromClassList("theme-grimoire");
        _stage.AddToClassList(ThemeClass(theme));

        _embersActive = theme == Theme.Astral;
        if (_embersHost != null) _embersHost.style.opacity = _embersActive ? 1f : 0f;
        Color emberColor = theme == Theme.Astral ? new Color(0.72f, 0.55f, 1f) : new Color(1f, 0.7f, 0.29f);
        foreach (var e in _embers) e.el.style.backgroundColor = emberColor;
    }

    private static string ThemeClass(Theme t)
    {
        switch (t)
        {
            case Theme.Forged: return "theme-forged";
            case Theme.Grimoire: return "theme-grimoire";
            default: return "theme-astral";
        }
    }

    public void SetTheme(Theme t)
    {
        theme = t;
        PlayerPrefs.SetString(PrefThemeKey, t.ToString());
        ApplyTheme();
    }

    // --------------------------- AMBIENT ----------------------------

    private void SetupAmbient(VisualElement root)
    {
        // 6 luces ancladas a las fuentes del fondo en coordenadas NORMALIZADAS (0..1)
        // del stage, para que sigan a la imagen a cualquier resolución.
        // x = fracción horizontal (0 izq, 1 der), y = fracción vertical (0 arriba, 1 abajo), size = diámetro px.
        AddGlow(root, "glow-moon",    new Color(0.92f, 0.96f, 1f,   0.98f), 0.7f, true,  0.350f, 0.108f, 360f); // luna (blanca)
        AddGlow(root, "glow-village", new Color(1f,    0.60f, 0.20f, 1f),    3.8f, false, 0.302f, 0.732f, 150f); // antorcha izquierda
        AddGlow(root, "glow-path",    new Color(1f,    0.60f, 0.20f, 1f),    3.6f, false, 0.712f, 0.658f, 150f); // antorcha derecha
        AddGlow(root, "glow-fire",    new Color(1f,    0.55f, 0.18f, 1f),    4.2f, false, 0.835f, 0.835f, 200f); // hoguera
        AddGlow(root, "glow-lantern", new Color(1f,    0.75f, 0.35f, 0.90f), 2.2f, false, 0.935f, 0.785f, 120f); // farol
        // glow-gate eliminado: sobraba encima del título

        if (vignetteTexture != null)
        {
            var vig = root.Q<VisualElement>("vignette");
            if (vig != null) vig.style.backgroundImage = new StyleBackground(vignetteTexture);
        }

        if (_embersHost != null && glowTexture != null)
        {
            Color emberColor = theme == Theme.Astral ? new Color(0.72f, 0.55f, 1f) : new Color(1f, 0.7f, 0.29f);
            for (int i = 0; i < EmberCount; i++)
            {
                var el = new VisualElement();
                el.AddToClassList("ember");
                el.pickingMode = PickingMode.Ignore;
                el.style.backgroundColor = emberColor;
                _embersHost.Add(el);

                var em = new Ember
                {
                    el = el,
                    baseX = Random.value,
                    drift = Random.Range(-60f, 60f),
                    speed = Random.Range(40f, 95f),
                    maxLife = Random.Range(7f, 14f),
                    size = Random.Range(2f, 5f),
                };
                em.life = Random.value * em.maxLife;
                _embers.Add(em);
            }
        }
    }

    // xN/yN: posición del CENTRO del glow en fracción del stage (0..1). sizePx: diámetro.
    private void AddGlow(VisualElement root, string name, Color tint, float speed, bool breathe, float xN, float yN, float sizePx)
    {
        var el = root.Q<VisualElement>(name);
        if (el == null) return;
        if (glowTexture != null) el.style.backgroundImage = new StyleBackground(glowTexture);
        el.style.unityBackgroundImageTintColor = tint;

        // Posición en % del stage + tamaño/centrado en código (anula lo que ponga la USS,
        // incluido el display:none de .t-village, para que las luces sigan al fondo).
        el.style.display = DisplayStyle.Flex;
        el.style.position = Position.Absolute;
        el.style.width = sizePx;
        el.style.height = sizePx;
        el.style.marginLeft = -sizePx / 2f;
        el.style.marginTop = -sizePx / 2f;
        // posición inicial en % (Update la recalcula sobre el rect real del fondo)
        el.style.left = Length.Percent(xN * 100f);
        el.style.top = Length.Percent(yN * 100f);

        _glows.Add(new GlowFx { el = el, seed = Random.value * 10f, speed = speed, baseOpacity = tint.a, breathe = breathe, xN = xN, yN = yN, sizePx = sizePx });
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        // Rectángulo real del fondo (scale-and-crop = COVER) dentro del stage,
        // para que cada luz caiga sobre su fuente sea cual sea la resolución.
        float stageW = _stage != null ? _stage.resolvedStyle.width : BgW;
        float stageH = _stage != null ? _stage.resolvedStyle.height : BgH;
        if (stageW < 1f) stageW = BgW;
        if (stageH < 1f) stageH = BgH;
        float cover = Mathf.Max(stageW / BgW, stageH / BgH);
        float imgW = BgW * cover, imgH = BgH * cover;
        float offX = (stageW - imgW) * 0.5f, offY = (stageH - imgH) * 0.5f;

        for (int i = 0; i < _glows.Count; i++)
        {
            var g = _glows[i];

            // anclar al fondo: centro = offset + fracción * tamaño-imagen
            g.el.style.left = offX + g.xN * imgW;
            g.el.style.top = offY + g.yN * imgH;

            float n;
            if (g.breathe)
                n = 0.5f + 0.35f * (0.5f + 0.5f * Mathf.Sin(t * (6.28f / 6f)));
            else
            {
                float raw = Mathf.PerlinNoise(g.seed, t * g.speed * 0.8f);
                n = Mathf.Lerp(0.35f, 1.1f, Mathf.Round(raw * 10f) / 10f);
            }
            g.el.style.opacity = n;
            float s = g.breathe ? Mathf.Lerp(0.98f, 1.08f, n) : Mathf.Lerp(0.92f, 1.12f, n);
            g.el.style.scale = new StyleScale(new Scale(new Vector3(s, s, 1f)));
        }

        if (_embersActive && _embers.Count > 0)
        {
            float dt = Time.unscaledDeltaTime;
            float h = _embersHost.resolvedStyle.height;
            float w = _embersHost.resolvedStyle.width;
            if (h < 1f) h = 768f;
            if (w < 1f) w = 1408f;

            for (int i = 0; i < _embers.Count; i++)
            {
                var e = _embers[i];
                e.life += dt;
                if (e.life > e.maxLife) e.life -= e.maxLife;
                float p = e.life / e.maxLife;
                float y = h - p * (h + 40f);
                float x = e.baseX * w + e.drift * p;
                float a = p < 0.1f ? p / 0.1f : (p > 0.8f ? Mathf.Max(0f, (1f - p) / 0.2f) : 0.85f);

                e.el.style.left = x;
                e.el.style.top = y;
                e.el.style.width = e.size;
                e.el.style.height = e.size;
                e.el.style.opacity = a;
                _embers[i] = e;
            }
        }

        if (_loadingActive && _dots.Count > 0)
        {
            for (int i = 0; i < _dots.Count; i++)
            {
                float offset = i * 0.4f;
                float jump = Mathf.Sin((t * 6f) - offset) * 15f;
                jump = Mathf.Min(0, jump); 
                _dots[i].style.translate = new StyleTranslate(new Translate(0, jump, 0));
            }
        }
    }

    // --------------------------- BUTTONS ----------------------------

    // Settings state
    private List<Resolution> resolutions = new List<Resolution>();

    private void WireButtons(VisualElement root)
    {
        OnClick(root, "btn-play", () => {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayStartButtonSound();
            OpenCharSelection();
        });
        OnClick(root, "btn-settings", () => { AudioManager.Instance?.PlayGenericButtonSound(); OpenSettings(); });
        OnClick(root, "btn-back", () => { AudioManager.Instance?.PlayGenericButtonSound(); CloseSettings(); });
        OnClick(root, "btn-exit", () => { AudioManager.Instance?.PlayGenericButtonSound(); ShowModal(); });
        OnClick(root, "btn-exit-yes", () => { AudioManager.Instance?.PlayStartButtonSound(); HideModal(); DoExit(); });
        OnClick(root, "btn-exit-no", () => { AudioManager.Instance?.PlayGenericButtonSound(); HideModal(); });

        if (_modal != null)
        {
            _modal.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == _modal) HideModal();
            });
        }

        PopulateResolutions(root);
    }

    private void PopulateResolutions(VisualElement root)
    {
        var dropdown = root.Q<DropdownField>("dropdown-res");
        if (dropdown == null) return;

        var allRes = Screen.resolutions;
        var uniqueRes = new List<Resolution>();
        var seen = new HashSet<string>();

        for (int i = allRes.Length - 1; i >= 0; i--)
        {
            var r = allRes[i];
            string key = r.width + "x" + r.height;
            if (!seen.Contains(key))
            {
                uniqueRes.Add(r);
                seen.Add(key);
            }
        }
        uniqueRes.Reverse();

        resolutions = uniqueRes;
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Count; i++)
        {
            options.Add($"{resolutions[i].width}x{resolutions[i].height}");
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResIndex = i;
            }
        }

        // preferir la resolución guardada si existe
        int savedW = PlayerPrefs.GetInt(ResWKey, 0);
        int savedH = PlayerPrefs.GetInt(ResHKey, 0);
        if (savedW > 0 && savedH > 0)
        {
            for (int i = 0; i < resolutions.Count; i++)
                if (resolutions[i].width == savedW && resolutions[i].height == savedH) { currentResIndex = i; break; }
        }

        dropdown.choices = options;
        dropdown.index = currentResIndex;
        dropdown.RegisterValueChangedCallback(evt => SetResolution(dropdown.index));

        // aplicar la guardada al arrancar (persiste a la escena de juego)
        ApplyResolution(currentResIndex);
    }

    private void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count) return;
        Resolution res = resolutions[index];
        PlayerPrefs.SetInt(ResWKey, res.width);
        PlayerPrefs.SetInt(ResHKey, res.height);
        PlayerPrefs.Save();
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count) return;
        Resolution res = resolutions[index];
        if (res.width != Screen.width || res.height != Screen.height)
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    private static void OnClick(VisualElement root, string name, System.Action action)
    {
        var el = root.Q<VisualElement>(name);
        if (el == null) return;
        el.RegisterCallback<ClickEvent>(_ => action());
    }

    private void OpenSettings() { if (_screenSettings != null) _screenSettings.style.display = DisplayStyle.Flex; }
    private void CloseSettings() { if (_screenSettings != null) _screenSettings.style.display = DisplayStyle.None; }
    private void ShowModal() { if (_modal != null) _modal.style.display = DisplayStyle.Flex; }
    private void HideModal() { if (_modal != null) _modal.style.display = DisplayStyle.None; }

    private IEnumerator LoadGameRoutine()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            var op = SceneManager.LoadSceneAsync(gameSceneName);
            while (op != null && !op.isDone) yield return null;
        }
        else
            Debug.LogWarning($"[MainMenu] La escena '{gameSceneName}' no está en Build Settings.");
    }

    private void DoExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Escape) { OnCancel(); evt.StopPropagation(); }
    }

    private void OnCancel()
    {
        if (_modal != null && _modal.resolvedStyle.display == DisplayStyle.Flex) { HideModal(); return; }
        if (_screenSettings != null && _screenSettings.resolvedStyle.display == DisplayStyle.Flex) CloseSettings();
        if (_screenCharSelect != null && _screenCharSelect.resolvedStyle.display == DisplayStyle.Flex) CloseCharSelection();
    }

    // --------------------------- SEGMENT ----------------------------

    private void WireSegment(VisualElement root)
    {
        var seg = root.Q<VisualElement>("seg-display");
        if (seg == null) return;

        var opts = seg.Query<VisualElement>(className: "seg-opt").ToList();
        string prefKey = $"astral.{theme}.displaymode";
        string saved = PlayerPrefs.GetString(prefKey, "seg-fullscreen");

        foreach (var opt in opts)
        {
            opt.RegisterCallback<ClickEvent>(_ =>
            {
                foreach (var o in opts) o.RemoveFromClassList("is-active");
                opt.AddToClassList("is-active");
                ApplyDisplayMode(opt.name);
                PlayerPrefs.SetString($"astral.{theme}.displaymode", opt.name);
            });
        }

        foreach (var o in opts) o.RemoveFromClassList("is-active");
        var active = root.Q<VisualElement>(saved) ?? root.Q<VisualElement>("seg-fullscreen");
        if (active != null) active.AddToClassList("is-active");
        ApplyDisplayMode(saved);
    }

    private void ApplyDisplayMode(string name)
    {
        switch (name)
        {
            case "seg-borderless": Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case "seg-windowed": Screen.fullScreenMode = FullScreenMode.Windowed; break;
            default: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
        }
    }

    // --------------------------- SLIDERS ----------------------------

    private void WireSliders(VisualElement root)
    {
        SetupSlider(root, "slider-master", "val-master", "master", 80, masterParam);
        SetupSlider(root, "slider-music", "val-music", "music", 65, musicParam);
        SetupSlider(root, "slider-sfx", "val-sfx", "sfx", 75, sfxParam);
    }

    private void SetupSlider(VisualElement root, string sliderName, string valName, string key, int def, string mixerParam)
    {
        var slider = root.Q<VisualElement>(sliderName);
        if (slider == null) return;
        var fill = slider.Q<VisualElement>(className: "pslider__fill");
        var handle = slider.Q<VisualElement>(className: "pslider__handle");
        var valLabel = root.Q<Label>(valName);
        string prefKey = $"astral.{theme}.{key}";

        int value = Mathf.Clamp(PlayerPrefs.GetInt(prefKey, def), 0, 100);

        void Render(int v)
        {
            v = Mathf.Clamp(v, 0, 100);
            value = v;
            if (fill != null) fill.style.width = Length.Percent(v);
            if (handle != null) handle.style.left = Length.Percent(v);
            if (valLabel != null) valLabel.text = v + "%";
            PlayerPrefs.SetInt(prefKey, v);
            ApplyVolume(mixerParam, v);
        }

        int FromPointer(float localX)
        {
            float w = slider.resolvedStyle.width;
            if (w < 1f) return value;
            return Mathf.RoundToInt(Mathf.Clamp01(localX / w) * 100f);
        }

        bool dragging = false;
        slider.RegisterCallback<PointerDownEvent>(evt =>
        {
            dragging = true;
            slider.CapturePointer(evt.pointerId);
            Render(FromPointer(evt.localPosition.x));
            evt.StopPropagation();
        });
        slider.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (dragging) Render(FromPointer(evt.localPosition.x));
        });
        slider.RegisterCallback<PointerUpEvent>(evt =>
        {
            dragging = false;
            if (slider.HasPointerCapture(evt.pointerId)) slider.ReleasePointer(evt.pointerId);
        });
        slider.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.UpArrow) { Render(value + 5); evt.StopPropagation(); }
            else if (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.DownArrow) { Render(value - 5); evt.StopPropagation(); }
            else if (evt.keyCode == KeyCode.Home) { Render(0); evt.StopPropagation(); }
            else if (evt.keyCode == KeyCode.End) { Render(100); evt.StopPropagation(); }
        });

        Render(value);
    }

    private void ApplyVolume(string param, int v)
    {
        float vol = v / 100f;
        if (AudioManager.Instance != null)
        {
            if (param == masterParam) AudioManager.Instance.SetMasterVolume(vol);
            else if (param == musicParam) AudioManager.Instance.SetMusicVolume(vol);
            else if (param == sfxParam) AudioManager.Instance.SetSfxVolume(vol);
        }
        else if (audioMixer != null && !string.IsNullOrEmpty(param))
        {
            float db = v <= 0 ? -80f : Mathf.Log10(Mathf.Clamp01(v / 100f)) * 20f;
            audioMixer.SetFloat(param, db);
        }
    }
}
