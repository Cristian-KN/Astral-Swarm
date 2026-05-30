using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// HUD in-game en UI Toolkit (puerto de hud.js del handoff "Forja de Hierro").
/// Reemplaza al antiguo UIManager + Views uGUI. Bindea el estado del gameplay
/// (GameManager / PlayerStats / PlayerAttack / InventoryManager) al HUD,
/// y gestiona los overlays de subida de nivel, pausa y fin de partida.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour
{
    [Header("UI (lo asigna el setup)")]
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private StyleSheet hudStyleSheet;

    [Header("Iconos HUD (opcional)")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite playIcon;

    [Header("Vida")]
    [Tooltip("Puntos de vida por corazón.")]
    [SerializeField] private float healthPerHeart = 20f;

    [Header("Minimapa")]
    [SerializeField] private float minimapWorldRadius = 25f;
    [SerializeField] private int maxRerolls = 2;
    [SerializeField] private int skipGoldBonus = 50;

    // ---- referencias de gameplay ----
    private GameManager gameManager;
    private PlayerAttack playerAttack;
    private InventoryManager inventory;
    private Transform playerTransform;

    // ---- elementos UI ----
    private VisualElement _root;
    private VisualElement _xpFill, _hearts, _coinIco, _pauseIco;
    private VisualElement _rowWeapons, _rowPassives, _mapInner;
    private VisualElement _overlayLevelUp, _pauseOverlay, _endOverlay, _dmgVignette, _settingsOverlay;
    private VisualElement _cardsHost;
    private Label _lvlNum, _timerClock, _goldAmt, _rerollBadge;
    private Label _psTime, _psLevel, _psGold, _endTitle;

    // ---- stats elements ----
    private Dictionary<string, Label> _statValueLabels = new Dictionary<string, Label>();
    private Dictionary<string, Label> _statModLabels = new Dictionary<string, Label>();

    // ---- estado overlays ----
    private Action<ItemData> _onChosen;
    private int _rerollsLeft;
    private bool _levelUpOpen;
    private bool _pauseOpen;

    // ---- minimap blip pool ----
    private readonly List<VisualElement> _enemyBlips = new List<VisualElement>();
    private readonly List<VisualElement> _lootBlips = new List<VisualElement>();
    private VisualElement _playerBlip;

    public bool IsLevelUpOpen => _levelUpOpen;

    // ========================================================================

    private void OnEnable()
    {
        // Suscribir eventos estáticos cuanto antes para no perder la primera emisión
        PlayerStats.onHealthChanged += SetHearts;
        InventoryManager.onInventoryChanged += RefreshLoadout;
        PlayerStats.onStatChanged += HandleStatChanged;
    }

    private void OnDisable()
    {
        PlayerStats.onHealthChanged -= SetHearts;
        InventoryManager.onInventoryChanged -= RefreshLoadout;
        PlayerStats.onStatChanged -= HandleStatChanged;
    }

    private void Start()
    {
        // En Start el UIDocument ya hizo su OnEnable → rootVisualElement está listo.
        var doc = GetComponent<UIDocument>();
        if (doc.panelSettings == null)
        {
            var ps = panelSettings != null ? panelSettings : Resources.Load<PanelSettings>("HUDPanelSettings");
            if (ps != null) doc.panelSettings = ps;
        }
        _root = doc.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("[HUD] rootVisualElement es null. ¿Falta UXML/PanelSettings en el UIDocument?");
            return;
        }

        // Aplicar la hoja de estilos por código (más robusto que el <Style> del UXML)
        if (hudStyleSheet != null && !_root.styleSheets.Contains(hudStyleSheet))
            _root.styleSheets.Add(hudStyleSheet);

        QueryElements();
        WireButtons();
        BuildMinimapPool();

        gameManager  = FindObjectOfType<GameManager>();
        playerAttack = FindObjectOfType<PlayerAttack>();
        inventory    = FindObjectOfType<InventoryManager>();
        var p = GameObject.FindWithTag("Player");
        if (p) playerTransform = p.transform;

        // stats init
        var stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            UpdateStatLabel("attack", stats.attackPower);
            UpdateStatLabel("attack-speed", stats.attackSpeed);
            UpdateStatLabel("range", stats.attackRange);
            UpdateStatLabel("speed", stats.speedMultiplier);
            UpdateStatLabel("luck", stats.luck);
            UpdateStatLabel("difficulty", stats.difficulty);
        }

        // icons
        SetIcon(_coinIco, coinIcon);
        SetIcon(_pauseIco, pauseIcon);

        // Pull inicial de estado (por si los Start de otros llegaron antes que el evento)
        if (gameManager != null)
        {
            UpdateLevelText(gameManager.GetCurrentLevel());
            UpdateExperienceBar(gameManager.currentExp, gameManager.expToNextLevel);
            UpdateTimer(gameManager.timeRemaining > 0 ? gameManager.timeRemaining : gameManager.timeToSurvive);
            UpdateGold(gameManager.currentGold);
        }
        var ps2 = FindObjectOfType<PlayerStats>();
        if (ps2 != null) SetHearts(ps2.CurrentHealth > 0 ? ps2.CurrentHealth : ps2.maxHealth, ps2.maxHealth);

        RefreshLoadout();
    }

    private void QueryElements()
    {
        _xpFill        = _root.Q<VisualElement>("xp-fill");
        _lvlNum        = _root.Q<Label>("lvl-num");
        _timerClock    = _root.Q<Label>("timer-clock");
        _hearts        = _root.Q<VisualElement>("hearts");
        _coinIco       = _root.Q<VisualElement>("coin-ico");
        _goldAmt       = _root.Q<Label>("gold-amt");
        _pauseIco      = _root.Q<VisualElement>("pause-ico");
        _rowWeapons    = _root.Q<VisualElement>("row-weapons");
        _rowPassives   = _root.Q<VisualElement>("row-passives");
        _mapInner      = _root.Q<VisualElement>("map-inner");

        _overlayLevelUp = _root.Q<VisualElement>("overlay-levelup");
        _cardsHost      = _root.Q<VisualElement>("cards");
        _rerollBadge    = _root.Q<Label>("reroll-badge");

        _pauseOverlay  = _root.Q<VisualElement>("pause-overlay");
        _psTime        = _root.Q<Label>("ps-time");
        _psLevel       = _root.Q<Label>("ps-level");
        _psGold        = _root.Q<Label>("ps-gold");

        _endOverlay    = _root.Q<VisualElement>("end-overlay");
        _endTitle      = _root.Q<Label>("end-title");
        _dmgVignette   = _root.Q<VisualElement>("dmg-vignette");
        _settingsOverlay = _root.Q<VisualElement>("settings-overlay");

        // Stat labels
        string[] statKeys = { "attack", "attack-speed", "range", "speed", "luck", "difficulty" };
        foreach (var key in statKeys)
        {
            _statValueLabels[key] = _root.Q<Label>($"stat-val-{key}");
            _statModLabels[key] = _root.Q<Label>($"stat-mod-{key}");
        }
    }

    private void HandleStatChanged(PlayerStats.StatChangeInfo info)
    {
        UpdateStatLabel(info.statName, info.newValue);
        ShowStatMod(info.statName, info.difference);
    }

    private void UpdateStatLabel(string key, float val)
    {
        if (_statValueLabels.TryGetValue(key, out var label) && label != null)
        {
            label.text = val.ToString("F1");
        }
    }

    private void ShowStatMod(string key, float diff)
    {
        if (_statModLabels.TryGetValue(key, out var label) && label != null)
        {
            label.text = (diff > 0 ? "+" : "") + diff.ToString("F1");
            label.AddToClassList("show");
            
            // Hide after 3 seconds
            label.schedule.Execute(() => label.RemoveFromClassList("show")).ExecuteLater(3000);
        }
    }

    private void WireButtons()
    {
        Click(_root.Q<VisualElement>("pause-btn"), () =>
        {
            if (_levelUpOpen || gameManager == null) return;
            if (gameManager.isPaused) gameManager.ResumeGame();
            else { gameManager.PauseGame(); ShowPauseMenu(true); }
        });
        Click(_root.Q<VisualElement>("btn-reroll"), OnReroll);
        Click(_root.Q<VisualElement>("btn-skip"), OnSkip);
        Click(_root.Q<VisualElement>("pbtn-resume"), () => gameManager?.ResumeGame());
        Click(_root.Q<VisualElement>("pbtn-settings"), OpenSettings);
        Click(_root.Q<VisualElement>("sbtn-back"), CloseSettings);
        Click(_root.Q<VisualElement>("pbtn-exit"), () => gameManager?.GoToMainMenu());

        // Slider de volumen
        var vol = _root.Q<Slider>("vol-slider");
        if (vol != null)
        {
            vol.value = AudioListener.volume;
            vol.RegisterValueChangedCallback(e => AudioListener.volume = e.newValue);
        }
        Click(_root.Q<VisualElement>("ebtn-restart"), () => gameManager?.RestartGame());
        Click(_root.Q<VisualElement>("ebtn-menu"), () => gameManager?.GoToMainMenu());
    }

    private static void Click(VisualElement el, Action action)
    {
        if (el == null) return;
        el.RegisterCallback<ClickEvent>(_ => action());
    }

    private static void SetIcon(VisualElement el, Sprite sprite)
    {
        if (el == null) return;
        el.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground();
    }

    // ===================== API consumida por GameManager =====================

    public void UpdateLevelText(int level)
    {
        if (_lvlNum != null) _lvlNum.text = level.ToString();
    }

    public void UpdateExperienceBar(int current, int max)
    {
        if (_xpFill == null) return;
        float pct = max > 0 ? Mathf.Clamp01((float)current / max) * 100f : 0f;
        _xpFill.style.width = Length.Percent(pct);
    }

    public void UpdateTimer(float seconds)
    {
        if (_timerClock == null) return;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        _timerClock.text = string.Format("{0:00}:{1:00}", m, s);
    }

    public void UpdateGold(int gold)
    {
        if (_goldAmt != null) _goldAmt.text = gold.ToString("N0");
    }

    public void SetHearts(float current, float max)
    {
        if (_hearts == null) return;
        _hearts.Clear();

        int nHearts = Mathf.Max(1, Mathf.CeilToInt(max / healthPerHeart));
        for (int i = 0; i < nHearts; i++)
        {
            float filled = (current / healthPerHeart) - i;
            var heart = new VisualElement();
            heart.AddToClassList("heart");
            heart.pickingMode = PickingMode.Ignore;

            Sprite sp = filled >= 1f ? heartFull : (filled >= 0.5f ? heartHalf : heartEmpty);
            if (sp != null)
            {
                heart.style.backgroundImage = new StyleBackground(sp);
            }
            else
            {
                heart.AddToClassList(filled >= 1f ? "heart-full" : (filled >= 0.5f ? "heart-half" : "heart-empty"));
            }
            _hearts.Add(heart);
        }
    }

    public bool CanShowLevelUp => _overlayLevelUp != null;

    public void ShowLevelUpMenu(bool show)
    {
        if (_overlayLevelUp == null) return;
        _overlayLevelUp.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        _levelUpOpen = show;
    }

    public void ShowLevelUpChoices(ItemData[] choices, Action<ItemData> onChosen)
    {
        _onChosen = onChosen;
        _rerollsLeft = maxRerolls;
        UpdateRerollBadge();
        RenderCards(choices);
        ShowLevelUpMenu(true);
    }

    public void ShowPauseMenu(bool show)
    {
        if (_pauseOverlay == null) return;
        if (show) RefreshPauseStats();
        _pauseOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        if (!show && _settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.None;
        _pauseOpen = show;
        SetIcon(_pauseIco, show ? playIcon : pauseIcon);
    }

    private void OpenSettings()
    {
        if (_pauseOverlay != null) _pauseOverlay.style.display = DisplayStyle.None;
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.Flex;
    }

    private void CloseSettings()
    {
        if (_settingsOverlay != null) _settingsOverlay.style.display = DisplayStyle.None;
        if (_pauseOverlay != null) { RefreshPauseStats(); _pauseOverlay.style.display = DisplayStyle.Flex; }
    }

    public void ShowGameOver()  => ShowEnd("GAME OVER");
    public void ShowVictory()   => ShowEnd("¡VICTORIA!");

    private void ShowEnd(string title)
    {
        if (_endOverlay == null) return;
        if (_endTitle != null) _endTitle.text = title;
        _endOverlay.style.display = DisplayStyle.Flex;
    }

    // ===================== Level-up cards =====================

    private static readonly string[] RarityNames =
        { "Común", "Raro", "Épico", "Legendario", "Mítico" };

    private void RenderCards(ItemData[] choices)
    {
        if (_cardsHost == null) return;
        _cardsHost.Clear();
        if (choices == null) return;

        foreach (ItemData item in choices)
        {
            if (item == null) continue;
            _cardsHost.Add(BuildCard(item));
        }
    }

    private VisualElement BuildCard(ItemData item)
    {
        var card = new VisualElement();
        card.AddToClassList("card");

        var rarity = new Label(RarityNames[Mathf.Clamp((int)item.rarity, 0, RarityNames.Length - 1)]);
        rarity.AddToClassList("card-rarity");
        card.Add(rarity);

        var iconFrame = new VisualElement();
        iconFrame.AddToClassList("card-icon");
        var ico = new VisualElement();
        ico.AddToClassList("ico");
        if (item.icon != null) ico.style.backgroundImage = new StyleBackground(item.icon);
        iconFrame.Add(ico);
        card.Add(iconFrame);

        var name = new Label(item.itemName);
        name.AddToClassList("card-name");
        card.Add(name);

        var tier = new Label(item.type == ItemType.Weapon ? "Arma" : "Pasivo");
        tier.AddToClassList("card-tier");
        card.Add(tier);

        // Generate Stat-rich description
        string fullDesc = item.description;
        string statInfo = "";
        if (item.attackBoost != 0) statInfo += $"\nATK: {(item.attackBoost > 0 ? "+" : "")}{item.attackBoost}";
        if (item.attackSpeedBoost != 0) statInfo += $"\nVEL ATK: {(item.attackSpeedBoost > 0 ? "+" : "")}{item.attackSpeedBoost}";
        if (item.rangeBoost != 0) statInfo += $"\nRANGO: {(item.rangeBoost > 0 ? "+" : "")}{item.rangeBoost}";
        if (item.speedBoost != 0) statInfo += $"\nVELOCIDAD: {(item.speedBoost > 0 ? "+" : "")}{item.speedBoost}";
        if (item.luckBoost != 0) statInfo += $"\nSUERTE: {(item.luckBoost > 0 ? "+" : "")}{item.luckBoost}";
        if (item.difficultyIncrease != 0) statInfo += $"\nDIFICULTAD: {(item.difficultyIncrease > 0 ? "+" : "")}{item.difficultyIncrease}";
        
        if (!string.IsNullOrEmpty(statInfo))
        {
            fullDesc += "\n" + statInfo;
        }

        var desc = new Label(fullDesc);
        desc.AddToClassList("card-desc");
        card.Add(desc);

        ItemData captured = item;
        card.RegisterCallback<ClickEvent>(_ =>
        {
            var cb = _onChosen;
            _onChosen = null;
            cb?.Invoke(captured);
        });

        return card;
    }

    private void OnReroll()
    {
        if (_rerollsLeft <= 0 || gameManager == null) return;
        _rerollsLeft--;
        UpdateRerollBadge();
        RenderCards(gameManager.RollNewChoices());
    }

    private void OnSkip()
    {
        gameManager?.AddGold(skipGoldBonus);
        var cb = _onChosen;
        _onChosen = null;
        cb?.Invoke(null); // GameManager.OnItemChosen(null) → ResumeGame
    }

    private void UpdateRerollBadge()
    {
        if (_rerollBadge != null) _rerollBadge.text = _rerollsLeft.ToString();
    }

    private void RefreshPauseStats()
    {
        if (gameManager == null) return;
        float t = gameManager.GetElapsedTime();
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        if (_psTime != null)  _psTime.text  = string.Format("{0:00}:{1:00}", m, s);
        if (_psLevel != null) _psLevel.text = gameManager.GetCurrentLevel().ToString();
        if (_psGold != null)  _psGold.text  = gameManager.currentGold.ToString("N0");
    }

    // ===================== Loadout =====================

    public void RefreshLoadout()
    {
        RefreshWeapons();
        RefreshPassives();
    }

    private void RefreshWeapons()
    {
        if (_rowWeapons == null) return;
        _rowWeapons.Clear();

        var slots = playerAttack != null ? playerAttack.WeaponSlots : null;
        for (int i = 0; i < 3; i++)
        {
            bool has = slots != null && i < slots.Count && slots[i].weaponData != null;
            var slot = new VisualElement();
            slot.AddToClassList("slot");

            if (has)
            {
                int level = slots[i].level;
                if (level >= 8) slot.AddToClassList("maxed");

                var ico = new VisualElement();
                ico.AddToClassList("ico");
                if (slots[i].weaponData.icon != null)
                    ico.style.backgroundImage = new StyleBackground(slots[i].weaponData.icon);
                slot.Add(ico);

                var pips = new VisualElement();
                pips.AddToClassList("lvl-pips");
                for (int p = 0; p < level; p++)
                {
                    var pip = new VisualElement();
                    pip.AddToClassList("pip");
                    pips.Add(pip);
                }
                slot.Add(pips);
            }
            else
            {
                slot.AddToClassList("is-empty");
                var plus = new Label("+");
                plus.AddToClassList("empty-plus");
                slot.Add(plus);
            }
            _rowWeapons.Add(slot);
        }
    }

    private void RefreshPassives()
    {
        if (_rowPassives == null) return;
        _rowPassives.Clear();
        if (inventory == null) return;

        foreach (var it in inventory.items)
        {
            if (it.data == null || it.data.type == ItemType.Weapon) continue;
            var slot = new VisualElement();
            slot.AddToClassList("slot");
            slot.AddToClassList("passive");
            var ico = new VisualElement();
            ico.AddToClassList("ico");
            if (it.data.icon != null) ico.style.backgroundImage = new StyleBackground(it.data.icon);
            slot.Add(ico);
            _rowPassives.Add(slot);
        }
    }

    // ===================== Minimap =====================

    private void BuildMinimapPool()
    {
        if (_mapInner == null) return;
        _playerBlip = new VisualElement();
        _playerBlip.AddToClassList("blip");
        _playerBlip.AddToClassList("player");
        _playerBlip.pickingMode = PickingMode.Ignore;
        _playerBlip.style.left = Length.Percent(50);
        _playerBlip.style.top = Length.Percent(50);
        _mapInner.Add(_playerBlip);
    }

    private void Update()
    {
        if (playerTransform == null || _mapInner == null) return;
        if (_levelUpOpen || _pauseOpen) return;

        UpdateBlips(_enemyBlips, GameObject.FindGameObjectsWithTag("Enemy"), "enemy");

        var gems = FindObjectsOfType<ExperienceGem>();
        var lootObjs = new GameObject[gems.Length];
        for (int i = 0; i < gems.Length; i++) lootObjs[i] = gems[i].gameObject;
        UpdateBlips(_lootBlips, lootObjs, "loot");
    }

    private void UpdateBlips(List<VisualElement> pool, GameObject[] targets, string cls)
    {
        // ensure pool size
        while (pool.Count < targets.Length)
        {
            var b = new VisualElement();
            b.AddToClassList("blip");
            b.AddToClassList(cls);
            b.pickingMode = PickingMode.Ignore;
            _mapInner.Add(b);
            pool.Add(b);
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (i < targets.Length)
            {
                Vector2 offset = (Vector2)(targets[i].transform.position - playerTransform.position);
                Vector2 norm = Vector2.ClampMagnitude(offset / minimapWorldRadius, 1f);
                float xPct = 50f + norm.x * 50f;
                float yPct = 50f - norm.y * 50f; // pantalla: y crece hacia abajo
                pool[i].style.display = DisplayStyle.Flex;
                pool[i].style.left = Length.Percent(xPct);
                pool[i].style.top = Length.Percent(yPct);
            }
            else
            {
                pool[i].style.display = DisplayStyle.None;
            }
        }
    }
}
