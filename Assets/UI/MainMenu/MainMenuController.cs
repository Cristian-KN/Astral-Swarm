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

    private const int EmberCount = 26;
    private const string PrefThemeKey = "astral.theme";

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
    }

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

        classCards[0]?.RegisterCallback<ClickEvent>(evt => SelectClass("warrior"));
        classCards[1]?.RegisterCallback<ClickEvent>(evt => SelectClass("archer"));
        classCards[2]?.RegisterCallback<ClickEvent>(evt => SelectClass("lancer"));

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            colorCards[i]?.RegisterCallback<ClickEvent>(evt => SelectColor(index));
        }

        root.Q<VisualElement>("btn-char-back")?.RegisterCallback<ClickEvent>(evt => CloseCharSelection());
        root.Q<VisualElement>("btn-char-play")?.RegisterCallback<ClickEvent>(evt => PlayGame());

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
        AddGlow(root, "glow-moon", new Color(0.59f, 0.67f, 1f, 0.6f), 0.7f, breathe: true);
        AddGlow(root, "glow-gate", new Color(1f, 0.75f, 0.35f, 0.95f), 2.5f, false);
        AddGlow(root, "glow-path", new Color(1f, 0.7f, 0.3f, 0.9f), 2.2f, false);
        AddGlow(root, "glow-village", new Color(1f, 0.75f, 0.35f, 0.92f), 2.8f, false);
        AddGlow(root, "glow-fire", new Color(1f, 0.6f, 0.2f, 1f), 4.2f, false);
        AddGlow(root, "glow-lantern", new Color(1f, 0.75f, 0.35f, 0.9f), 1.9f, false);

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

    private void AddGlow(VisualElement root, string name, Color tint, float speed, bool breathe)
    {
        var el = root.Q<VisualElement>(name);
        if (el == null) return;
        if (glowTexture != null) el.style.backgroundImage = new StyleBackground(glowTexture);
        el.style.unityBackgroundImageTintColor = tint;
        _glows.Add(new GlowFx { el = el, seed = Random.value * 10f, speed = speed, baseOpacity = tint.a, breathe = breathe });
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        for (int i = 0; i < _glows.Count; i++)
        {
            var g = _glows[i];
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

    private void WireButtons(VisualElement root)
    {
        OnClick(root, "btn-play", OpenCharSelection); // Modified to open character selection
        OnClick(root, "btn-settings", OpenSettings);
        OnClick(root, "btn-back", CloseSettings);
        OnClick(root, "btn-exit", ShowModal);
        OnClick(root, "btn-exit-yes", () => { HideModal(); DoExit(); });
        OnClick(root, "btn-exit-no", HideModal);

        if (_modal != null)
        {
            _modal.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == _modal) HideModal();
            });
        }
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
        if (audioMixer == null || string.IsNullOrEmpty(param)) return;
        float db = v <= 0 ? -80f : Mathf.Log10(Mathf.Clamp01(v / 100f)) * 20f;
        audioMixer.SetFloat(param, db);
    }
}
