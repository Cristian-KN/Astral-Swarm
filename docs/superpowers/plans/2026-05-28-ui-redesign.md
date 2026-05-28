# UI Redesign — Astral Swarm Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rediseñar toda la UI del juego (HUD, Level Up, Tienda, Menú Principal, Pausa) con estética fantasy/místico usando Unity Legacy UI.

**Architecture:** Todo el Canvas de la escena Game se genera programáticamente desde `AstralSwarmSetup.cs` (editor script). Los scripts de lógica (UIManager, PlayerStats, GameManager, PauseManager) se modifican para conectar los nuevos elementos. Se añaden dos scripts nuevos: `ShopUI.cs` y `CardHoverEffect.cs`. El Menú Principal se genera con un nuevo método `SetupMainMenuScene()` en `AstralSwarmSetup.cs`.

**Tech Stack:** Unity Legacy uGUI (Canvas, Image, Text, Button, Slider, Dropdown), C#, UnityEditor API

---

## Paleta de colores de referencia

```
PanelBg     = new Color(0.102f, 0.039f, 0.180f, 1f)   // #1A0A2E
CardBg      = new Color(0.176f, 0.106f, 0.306f, 1f)   // #2D1B4E
Gold        = new Color(1f,    0.843f, 0f,    1f)      // #FFD700
GoldMoney   = new Color(1f,    0.757f, 0.027f, 1f)     // #FFC107
HealthRed   = new Color(0.957f,0.263f, 0.212f, 1f)     // #F44336
XpBlue      = new Color(0.086f,0.396f, 0.753f, 1f)     // #1565C0
XpPurple    = new Color(0.482f,0.122f, 0.635f, 1f)     // #7B1FA2
BorderGold  = new Color(1f,    0.843f, 0f,    1f)      // #FFD700
OverlayDark = new Color(0f,    0f,    0f,    0.72f)
OverlaySemi = new Color(0f,    0f,    0f,    0.50f)
RarityCommon    = new Color(0.620f, 0.620f, 0.620f, 1f) // #9E9E9E
RarityRare      = new Color(0.129f, 0.588f, 0.953f, 1f) // #2196F3
RarityEpic      = new Color(0.612f, 0.153f, 0.690f, 1f) // #9C27B0
RarityLegendary = new Color(1f,    0.596f, 0f,    1f)   // #FF9800
RarityMythic    = new Color(0.957f,0.263f, 0.212f, 1f)  // #F44336
```

---

## Mapa de archivos

| Archivo | Acción |
|---|---|
| `Assets/Scripts/UIManager.cs` | Modificar — añadir healthText, goldText, shopPanel, xpLabelText |
| `Assets/Scripts/PlayerStats.cs` | Modificar — conectar UIManager en Start y TakeDamage |
| `Assets/Scripts/GameManager.cs` | Modificar — eliminar bloque ESC, añadir UpdateGold en AddGold, limpiar ResumeGame |
| `Assets/Scripts/PauseManager.cs` | Modificar — añadir Slider/Dropdown ajustes, conectar a GameManager |
| `Assets/Scripts/ShopUI.cs` | Crear — genera botones de tienda dinámicamente |
| `Assets/Scripts/CardHoverEffect.cs` | Crear — hover scale en cartas de level up |
| `Assets/Editor/AstralSwarmSetup.cs` | Modificar — reemplazar CreatePanel/CreateButton/CreateSlider/SetupScene con versiones estilizadas + añadir SetupMainMenuScene |

---

## Task 1: CardHoverEffect.cs

**Files:**
- Create: `Assets/Scripts/CardHoverEffect.cs`

- [ ] **Step 1: Crear el script**

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color rarityColor = Color.white;

    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = rarityColor;
        outline.effectDistance = new Vector2(3f, -3f);
        outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.05f;
        outline.effectColor = rarityColor;
        outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        outline.enabled = false;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/CardHoverEffect.cs Assets/Scripts/CardHoverEffect.cs.meta
git commit -m "feat: CardHoverEffect — hover scale + rarity outline on level-up cards"
```

---

## Task 2: ShopUI.cs

**Files:**
- Create: `Assets/Scripts/ShopUI.cs`

- [ ] **Step 1: Crear el script**

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject shopPanel;
    public Transform itemGrid;
    public GameObject itemButtonPrefab; // instanciado en runtime si es null

    private ShopManager shopManager;
    private GameManager gameManager;

    private void Awake()
    {
        shopManager = FindObjectOfType<ShopManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        PopulateItems();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void PopulateItems()
    {
        foreach (Transform child in itemGrid) Destroy(child.gameObject);

        List<ItemData> items = shopManager.availableItems;
        int totalSlots = Mathf.Min(6, shopManager.baseSlots);

        for (int i = 0; i < totalSlots && i < items.Count; i++)
        {
            ItemData item = items[i];
            CreateItemButton(item);
        }
    }

    private void CreateItemButton(ItemData item)
    {
        GameObject btnGO = new GameObject(item.itemName, typeof(RectTransform));
        btnGO.transform.SetParent(itemGrid, false);

        Image bg = btnGO.AddComponent<Image>();
        bg.color = new Color(0.176f, 0.106f, 0.306f, 1f); // CardBg

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;

        // Borde rareza
        Outline border = btnGO.AddComponent<Outline>();
        border.effectColor = GetRarityColor(item.rarity);
        border.effectDistance = new Vector2(2f, -2f);

        int price = CalculatePrice(item);
        bool canAfford = gameManager.currentGold >= price;

        if (!canAfford)
        {
            border.effectColor = new Color(0.957f, 0.263f, 0.212f, 1f);
            btn.interactable = false;
        }

        // Icono
        if (item.icon != null)
        {
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(btnGO.transform, false);
            Image icon = iconGO.AddComponent<Image>();
            icon.sprite = item.icon;
            icon.preserveAspect = true;
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.65f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.offsetMin = new Vector2(-24f, -4f);
            iconRT.offsetMax = new Vector2(24f, -4f);
        }

        // Nombre
        GameObject nameGO = new GameObject("Name", typeof(RectTransform));
        nameGO.transform.SetParent(btnGO.transform, false);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = item.itemName;
        nameText.fontSize = 13;
        nameText.color = new Color(1f, 0.843f, 0f, 1f);
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.35f);
        nameRT.anchorMax = new Vector2(1f, 0.65f);
        nameRT.offsetMin = new Vector2(4f, 0f);
        nameRT.offsetMax = new Vector2(-4f, 0f);

        // Precio
        GameObject priceGO = new GameObject("Price", typeof(RectTransform));
        priceGO.transform.SetParent(btnGO.transform, false);
        Text priceText = priceGO.AddComponent<Text>();
        priceText.text = "💰 " + price;
        priceText.fontSize = 12;
        priceText.color = canAfford ? new Color(1f, 0.757f, 0.027f, 1f) : new Color(0.957f, 0.263f, 0.212f, 1f);
        priceText.alignment = TextAnchor.MiddleCenter;
        priceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                      ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        RectTransform priceRT = priceGO.GetComponent<RectTransform>();
        priceRT.anchorMin = new Vector2(0f, 0f);
        priceRT.anchorMax = new Vector2(1f, 0.35f);
        priceRT.offsetMin = Vector2.zero;
        priceRT.offsetMax = Vector2.zero;

        ItemData captured = item;
        btn.onClick.AddListener(() => { shopManager.BuyItem(captured); CloseShop(); });
    }

    private int CalculatePrice(ItemData item)
    {
        float mult = item.rarity switch
        {
            ItemRarity.Common    => 2.5f,
            ItemRarity.Rare      => 6.25f,
            ItemRarity.Epic      => 12.5f,
            ItemRarity.Legendary => 25f,
            ItemRarity.Mythic    => 150f,
            _                    => 2.5f
        };
        return Mathf.RoundToInt(mult * gameManager.GetCurrentLevel() * 10f);
    }

    private Color GetRarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common    => new Color(0.620f, 0.620f, 0.620f, 1f),
        ItemRarity.Rare      => new Color(0.129f, 0.588f, 0.953f, 1f),
        ItemRarity.Epic      => new Color(0.612f, 0.153f, 0.690f, 1f),
        ItemRarity.Legendary => new Color(1f,     0.596f, 0f,     1f),
        ItemRarity.Mythic    => new Color(0.957f, 0.263f, 0.212f, 1f),
        _                    => Color.white
    };
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/ShopUI.cs Assets/Scripts/ShopUI.cs.meta
git commit -m "feat: ShopUI — dynamic shop buttons with icon/name/price and rarity borders"
```

---

## Task 3: Modificar UIManager.cs

**Files:**
- Modify: `Assets/Scripts/UIManager.cs`

- [ ] **Step 1: Añadir campos y métodos nuevos**

En `UIManager.cs`, añadir bajo el header `[Header("HUD")]` existente:

```csharp
[Header("HUD — Stats")]
public Text healthText;
public Text goldText;
public Text xpLabelText;

[Header("Shop")]
public GameObject shopPanel;
```

Añadir los métodos al final de la clase (antes del último `}`):

```csharp
public void UpdateHealth(float current, float max)
{
    if (healthText != null)
        healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
}

public void UpdateGold(int gold)
{
    if (goldText != null)
        goldText.text = gold.ToString();
}

public void UpdateXpLabel(int current, int max)
{
    if (xpLabelText != null)
        xpLabelText.text = current + " / " + max + " XP";
}

public void ShowShop(bool show)
{
    if (shopPanel != null) shopPanel.SetActive(show);
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/UIManager.cs
git commit -m "feat: UIManager — add health/gold/xpLabel/shop references and methods"
```

---

## Task 4: Modificar PlayerStats.cs

**Files:**
- Modify: `Assets/Scripts/PlayerStats.cs`

- [ ] **Step 1: Conectar UIManager**

Añadir campo privado al principio de la clase:

```csharp
private UIManager uiManager;
```

En `Start()`, después de `currentHealth = maxHealth;`:

```csharp
uiManager = FindObjectOfType<UIManager>();
uiManager?.UpdateHealth(currentHealth, maxHealth);
```

En `TakeDamage()`, después de `currentHealth = Mathf.Clamp(...)`:

```csharp
uiManager?.UpdateHealth(currentHealth, maxHealth);
```

Eliminar la línea:
```csharp
Debug.Log("Vida actual del jugador: " + currentHealth);
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/PlayerStats.cs
git commit -m "feat: PlayerStats — wire UIManager.UpdateHealth on damage and start"
```

---

## Task 5: Modificar GameManager.cs

**Files:**
- Modify: `Assets/Scripts/GameManager.cs`

- [ ] **Step 1: Eliminar bloque ESC de Update()**

Localizar en `Update()` el bloque:
```csharp
if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
{
    bool levelingUp = uiManager != null
        && uiManager.levelUpPanel != null
        && uiManager.levelUpPanel.activeSelf;

    if (!levelingUp)
    {
        if (isPaused) ResumeGame();
        else { PauseGame(); uiManager?.ShowPauseMenu(true); }
    }
}
```
Eliminarlo completamente. El ESC lo gestiona `PauseManager`.

- [ ] **Step 2: Añadir UpdateGold en AddGold()**

Reemplazar:
```csharp
public void AddGold(int amount) => currentGold += amount;
```
Por:
```csharp
public void AddGold(int amount)
{
    currentGold += amount;
    uiManager?.UpdateGold(currentGold);
}
```

- [ ] **Step 3: Limpiar ResumeGame()**

En `ResumeGame()`, eliminar la llamada a `uiManager.ShowPauseMenu(false)` — la pausa la cierra `PauseManager`. El método debe quedar:

```csharp
public void ResumeGame()
{
    isPaused = false;
    Time.timeScale = 1f;
    if (uiManager != null)
    {
        uiManager.ShowLevelUpMenu(false);
    }
}
```

- [ ] **Step 4: Inicializar gold en Start()**

En `Start()`, después de `uiManager.UpdateTimer(timeRemaining)`:
```csharp
uiManager.UpdateGold(currentGold);
```

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/GameManager.cs
git commit -m "feat: GameManager — remove ESC block, wire UpdateGold, clean ResumeGame"
```

---

## Task 6: Modificar PauseManager.cs

**Files:**
- Modify: `Assets/Scripts/PauseManager.cs`

- [ ] **Step 1: Añadir referencias de ajustes y GameManager**

Reemplazar el contenido completo de `PauseManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            windowModeDropdown.AddOptions(new System.Collections.Generic.List<string>
                { "Ventana", "Pantalla Completa", "Sin Bordes" });
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

        pausePanel.SetActive(isPaused);
        if (!isPaused && settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
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
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/PauseManager.cs
git commit -m "feat: PauseManager — add settings (volume slider, window dropdown), wire GameManager"
```

---

## Task 7: Actualizar AstralSwarmSetup.cs — helpers estilizados

**Files:**
- Modify: `Assets/Editor/AstralSwarmSetup.cs`

Este task reemplaza los helpers visuales del setup script para que generen UI con la estética fantasy/místico.

- [ ] **Step 1: Añadir helper de color de rareza (estático privado)**

Añadir al final de la región `// UI HELPERS`, antes del cierre `#endif`:

```csharp
private static Color RarityColor(ItemRarity r)
{
    switch (r)
    {
        case ItemRarity.Common:    return new Color(0.620f, 0.620f, 0.620f, 1f);
        case ItemRarity.Rare:      return new Color(0.129f, 0.588f, 0.953f, 1f);
        case ItemRarity.Epic:      return new Color(0.612f, 0.153f, 0.690f, 1f);
        case ItemRarity.Legendary: return new Color(1f,     0.596f, 0f,     1f);
        case ItemRarity.Mythic:    return new Color(0.957f, 0.263f, 0.212f, 1f);
        default:                   return Color.white;
    }
}
```

- [ ] **Step 2: Reemplazar CreatePanel para usar estilo fantasy**

Reemplazar el método `CreatePanel` existente:

```csharp
private static GameObject CreatePanel(GameObject parent, string name, Color color)
{
    GameObject panel = new GameObject(name, typeof(RectTransform));
    panel.transform.SetParent(parent.transform, false);
    Image img = panel.AddComponent<Image>();
    img.color = color;
    StretchToFill(panel.GetComponent<RectTransform>());
    return panel;
}
```

Por:

```csharp
private static GameObject CreatePanel(GameObject parent, string name, Color color)
{
    GameObject panel = new GameObject(name, typeof(RectTransform));
    panel.transform.SetParent(parent.transform, false);
    Image img = panel.AddComponent<Image>();
    img.color = color;
    StretchToFill(panel.GetComponent<RectTransform>());
    return panel;
}

private static GameObject CreateStyledPanel(GameObject parent, string name)
{
    GameObject panel = CreatePanel(parent, name, new Color(0.102f, 0.039f, 0.180f, 0.97f));
    Outline border = panel.AddComponent<Outline>();
    border.effectColor = new Color(1f, 0.843f, 0f, 1f);
    border.effectDistance = new Vector2(3f, -3f);
    return panel;
}
```

- [ ] **Step 3: Reemplazar CreateButton para estilo dorado**

Reemplazar el método `CreateButton` existente:

```csharp
private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos)
{
    GameObject btnGO = new GameObject(name, typeof(RectTransform));
    btnGO.transform.SetParent(parent.transform, false);
    Image img = btnGO.AddComponent<Image>();
    img.color = new Color(0.25f, 0.25f, 0.35f, 1f);
    Button btn = btnGO.AddComponent<Button>();
    btn.targetGraphic = img;

    RectTransform rt = btnGO.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0.5f, 0.5f);
    rt.anchorMax = new Vector2(0.5f, 0.5f);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.sizeDelta = new Vector2(300f, 80f);
    rt.anchoredPosition = anchoredPos;

    Text lbl = CreateLegacyText(btnGO, "Text", label, 22, Color.white, TextAnchor.MiddleCenter);
    StretchToFill(lbl.GetComponent<RectTransform>());
    return btn;
}
```

Por:

```csharp
private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos)
{
    GameObject btnGO = new GameObject(name, typeof(RectTransform));
    btnGO.transform.SetParent(parent.transform, false);
    Image img = btnGO.AddComponent<Image>();
    img.color = new Color(0.176f, 0.106f, 0.306f, 1f); // CardBg
    Button btn = btnGO.AddComponent<Button>();
    btn.targetGraphic = img;

    ColorBlock cb = btn.colors;
    cb.normalColor      = new Color(0.176f, 0.106f, 0.306f, 1f);
    cb.highlightedColor = new Color(0.280f, 0.180f, 0.450f, 1f);
    cb.pressedColor     = new Color(0.100f, 0.060f, 0.190f, 1f);
    btn.colors = cb;

    Outline border = btnGO.AddComponent<Outline>();
    border.effectColor    = new Color(1f, 0.843f, 0f, 0.6f);
    border.effectDistance = new Vector2(2f, -2f);

    RectTransform rt = btnGO.GetComponent<RectTransform>();
    rt.anchorMin       = new Vector2(0.5f, 0.5f);
    rt.anchorMax       = new Vector2(0.5f, 0.5f);
    rt.pivot           = new Vector2(0.5f, 0.5f);
    rt.sizeDelta       = new Vector2(300f, 70f);
    rt.anchoredPosition = anchoredPos;

    Text lbl = CreateLegacyText(btnGO, "Text", label, 22, new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleCenter);
    StretchToFill(lbl.GetComponent<RectTransform>());
    return btn;
}
```

- [ ] **Step 4: Commit**

```bash
git add Assets/Editor/AstralSwarmSetup.cs
git commit -m "feat: AstralSwarmSetup — styled helpers (CreateStyledPanel, CreateButton fantasy)"
```

---

## Task 8: Actualizar SetupScene — HUD estilizado

**Files:**
- Modify: `Assets/Editor/AstralSwarmSetup.cs`

- [ ] **Step 1: Reemplazar bloque HUD en SetupScene**

Localizar en `SetupScene()` el bloque que comienza con `// HUD` y termina antes de `// ----- Level Up Panel`. Reemplazarlo con:

```csharp
// HUD
GameObject hud = CreateUIChild(canvasGO, "HUD");
StretchToFill(hud.GetComponent<RectTransform>());

// Panel Stats (arriba-izquierda)
GameObject statsPanel = new GameObject("StatsPanel", typeof(RectTransform));
statsPanel.transform.SetParent(hud.transform, false);
Image statsBg = statsPanel.AddComponent<Image>();
statsBg.color = new Color(0.102f, 0.039f, 0.180f, 0.85f);
Outline statsBorder = statsPanel.AddComponent<Outline>();
statsBorder.effectColor = new Color(1f, 0.843f, 0f, 1f);
statsBorder.effectDistance = new Vector2(2f, -2f);
RectTransform statsRT = statsPanel.GetComponent<RectTransform>();
statsRT.anchorMin = new Vector2(0f, 1f);
statsRT.anchorMax = new Vector2(0f, 1f);
statsRT.pivot     = new Vector2(0f, 1f);
statsRT.sizeDelta = new Vector2(200f, 120f);
statsRT.anchoredPosition = new Vector2(10f, -10f);

// Vida
Text healthText = CreateLegacyText(statsPanel, "HealthText", "❤ 100 / 100", 14,
    new Color(0.957f, 0.263f, 0.212f, 1f), TextAnchor.MiddleLeft);
RectTransform healthRT = healthText.GetComponent<RectTransform>();
healthRT.anchorMin = new Vector2(0f, 0.66f); healthRT.anchorMax = new Vector2(1f, 1f);
healthRT.offsetMin = new Vector2(8f, 0f);    healthRT.offsetMax = new Vector2(-4f, 0f);

// Nivel
Text levelText = CreateLegacyText(statsPanel, "LevelText", "⭐ LVL 1", 14,
    new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleLeft);
RectTransform lvlRT = levelText.GetComponent<RectTransform>();
lvlRT.anchorMin = new Vector2(0f, 0.33f); lvlRT.anchorMax = new Vector2(1f, 0.66f);
lvlRT.offsetMin = new Vector2(8f, 0f);   lvlRT.offsetMax = new Vector2(-4f, 0f);

// Oro
Text goldText = CreateLegacyText(statsPanel, "GoldText", "💰 0", 14,
    new Color(1f, 0.757f, 0.027f, 1f), TextAnchor.MiddleLeft);
RectTransform goldRT = goldText.GetComponent<RectTransform>();
goldRT.anchorMin = new Vector2(0f, 0f);  goldRT.anchorMax = new Vector2(1f, 0.33f);
goldRT.offsetMin = new Vector2(8f, 0f); goldRT.offsetMax = new Vector2(-4f, 0f);

// Panel XP (arriba-centro)
GameObject xpPanel = new GameObject("XpPanel", typeof(RectTransform));
xpPanel.transform.SetParent(hud.transform, false);
Image xpBg = xpPanel.AddComponent<Image>();
xpBg.color = new Color(0.102f, 0.039f, 0.180f, 0.85f);
Outline xpBorder = xpPanel.AddComponent<Outline>();
xpBorder.effectColor = new Color(1f, 0.843f, 0f, 1f);
xpBorder.effectDistance = new Vector2(2f, -2f);
RectTransform xpPanelRT = xpPanel.GetComponent<RectTransform>();
xpPanelRT.anchorMin = new Vector2(0.5f, 1f);
xpPanelRT.anchorMax = new Vector2(0.5f, 1f);
xpPanelRT.pivot     = new Vector2(0.5f, 1f);
xpPanelRT.sizeDelta = new Vector2(500f, 50f);
xpPanelRT.anchoredPosition = new Vector2(0f, -10f);

// Slider XP
Slider xpSlider = CreateSlider(xpPanel, "XpSlider");
xpSlider.minValue = 0f; xpSlider.maxValue = 100f; xpSlider.interactable = false;
// Fill color = purple (XpPurple)
Transform fillT = xpSlider.transform.Find("Fill Area/Fill");
if (fillT != null) fillT.GetComponent<Image>().color = new Color(0.482f, 0.122f, 0.635f, 1f);
RectTransform xpRT = xpSlider.GetComponent<RectTransform>();
xpRT.anchorMin = Vector2.zero; xpRT.anchorMax = Vector2.one;
xpRT.offsetMin = new Vector2(6f, 6f); xpRT.offsetMax = new Vector2(-6f, -22f);

// Label XP sobre slider
Text xpLabelText = CreateLegacyText(xpPanel, "XpLabel", "0 / 100 XP", 12,
    Color.white, TextAnchor.MiddleCenter);
RectTransform xpLabelRT = xpLabelText.GetComponent<RectTransform>();
xpLabelRT.anchorMin = Vector2.zero; xpLabelRT.anchorMax = Vector2.one;
xpLabelRT.offsetMin = new Vector2(6f, 0f); xpLabelRT.offsetMax = new Vector2(-6f, -4f);

// Timer (arriba-derecha)
Text timerText = CreateLegacyText(hud, "TimerText", "03:00", 26,
    new Color(1f, 0.843f, 0f, 1f), TextAnchor.UpperRight);
RectTransform timerRT = timerText.GetComponent<RectTransform>();
timerRT.anchorMin = new Vector2(1f, 1f); timerRT.anchorMax = new Vector2(1f, 1f);
timerRT.pivot     = new Vector2(1f, 1f);
timerRT.sizeDelta = new Vector2(180f, 50f);
timerRT.anchoredPosition = new Vector2(-10f, -10f);
```

- [ ] **Step 2: Actualizar Wire UIManager para incluir nuevos campos**

En el bloque `// ----- Wire UIManager -----`, añadir después de `SetRef(uiSO, "timerText", timerText)`:

```csharp
SetRef(uiSO, "healthText",  healthText);
SetRef(uiSO, "goldText",    goldText);
SetRef(uiSO, "xpLabelText", xpLabelText);
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Editor/AstralSwarmSetup.cs
git commit -m "feat: AstralSwarmSetup — styled HUD (stats panel, XP bar, timer gold)"
```

---

## Task 9: Actualizar SetupScene — Level Up y Paneles

**Files:**
- Modify: `Assets/Editor/AstralSwarmSetup.cs`

- [ ] **Step 1: Reemplazar Level Up Panel**

Localizar el bloque `// ----- Level Up Panel (3 cards side by side) -----` hasta el cierre del bucle for. Reemplazarlo con:

```csharp
// ----- Level Up Panel -----
GameObject levelUpPanel = CreatePanel(canvasGO, "LevelUpPanel", new Color(0f, 0f, 0f, 0.72f));
levelUpPanel.SetActive(false);

// Panel central estilizado
GameObject levelUpCenter = CreateStyledPanel(levelUpPanel, "LevelUpCenter");
RectTransform lupCenterRT = levelUpCenter.GetComponent<RectTransform>();
lupCenterRT.anchorMin = new Vector2(0.5f, 0.5f);
lupCenterRT.anchorMax = new Vector2(0.5f, 0.5f);
lupCenterRT.pivot     = new Vector2(0.5f, 0.5f);
lupCenterRT.sizeDelta = new Vector2(1000f, 380f);
lupCenterRT.anchoredPosition = Vector2.zero;

// Título
Text lupTitle = CreateLegacyText(levelUpCenter, "TitleText", "¡NIVEL ALCANZADO!", 32,
    new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleCenter);
Shadow lupShadow = lupTitle.gameObject.AddComponent<Shadow>();
lupShadow.effectColor = new Color(0.5f, 0.3f, 0f, 0.8f);
lupShadow.effectDistance = new Vector2(2f, -2f);
RectTransform lupTitleRT = lupTitle.GetComponent<RectTransform>();
lupTitleRT.anchorMin = new Vector2(0f, 0.75f);
lupTitleRT.anchorMax = new Vector2(1f, 1f);
lupTitleRT.offsetMin = Vector2.zero; lupTitleRT.offsetMax = Vector2.zero;

var cards = new Button[3];
var cardNames = new Text[3];
var cardDescs = new Text[3];
var cardIcons = new Image[3];
Vector2[] cardPositions = { new Vector2(-320f, -20f), new Vector2(0f, -20f), new Vector2(320f, -20f) };
string[] defaultNames = { "Opción 1", "Opción 2", "Opción 3" };

for (int i = 0; i < 3; i++)
{
    (cards[i], cardNames[i], cardDescs[i], cardIcons[i]) = CreateStyledLevelUpCard(
        levelUpCenter, "Card" + (i + 1), defaultNames[i], "...", cardPositions[i]);
}
```

- [ ] **Step 2: Añadir método CreateStyledLevelUpCard**

Añadir este método en la región `// UI HELPERS`:

```csharp
private static (Button btn, Text nameText, Text descText, Image iconImg) CreateStyledLevelUpCard(
    GameObject parent, string name, string itemName, string desc, Vector2 anchoredPos)
{
    GameObject cardGO = new GameObject(name, typeof(RectTransform));
    cardGO.transform.SetParent(parent.transform, false);
    Image img = cardGO.AddComponent<Image>();
    img.color = new Color(0.176f, 0.106f, 0.306f, 1f);
    Button btn = cardGO.AddComponent<Button>();
    btn.targetGraphic = img;

    ColorBlock cb = btn.colors;
    cb.highlightedColor = new Color(0.250f, 0.160f, 0.420f, 1f);
    cb.pressedColor     = new Color(0.100f, 0.060f, 0.190f, 1f);
    btn.colors = cb;

    Outline border = cardGO.AddComponent<Outline>();
    border.effectColor    = new Color(1f, 0.843f, 0f, 0.5f);
    border.effectDistance = new Vector2(2f, -2f);

    cardGO.AddComponent<CardHoverEffect>();

    RectTransform rt = cardGO.GetComponent<RectTransform>();
    rt.anchorMin       = new Vector2(0.5f, 0.5f);
    rt.anchorMax       = new Vector2(0.5f, 0.5f);
    rt.pivot           = new Vector2(0.5f, 0.5f);
    rt.sizeDelta       = new Vector2(280f, 220f);
    rt.anchoredPosition = anchoredPos;

    // Icono
    GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
    iconGO.transform.SetParent(cardGO.transform, false);
    Image iconImg = iconGO.AddComponent<Image>();
    iconImg.color = Color.white;
    iconImg.preserveAspect = true;
    RectTransform iconRT = iconGO.GetComponent<RectTransform>();
    iconRT.anchorMin = new Vector2(0.5f, 0.65f);
    iconRT.anchorMax = new Vector2(0.5f, 1f);
    iconRT.offsetMin = new Vector2(-28f, -8f);
    iconRT.offsetMax = new Vector2(28f,  -8f);

    // Nombre
    Text nameT = CreateLegacyText(cardGO, "CardName", itemName, 16,
        new Color(1f, 0.843f, 0f, 1f), TextAnchor.UpperCenter);
    RectTransform nameRT = nameT.GetComponent<RectTransform>();
    nameRT.anchorMin = new Vector2(0f, 0.45f);
    nameRT.anchorMax = new Vector2(1f, 0.65f);
    nameRT.offsetMin = new Vector2(8f, 0f);
    nameRT.offsetMax = new Vector2(-8f, 0f);
    nameT.horizontalOverflow = HorizontalWrapMode.Wrap;
    nameT.verticalOverflow   = VerticalWrapMode.Truncate;

    // Descripción
    Text descT = CreateLegacyText(cardGO, "CardDesc", desc, 12,
        Color.white, TextAnchor.UpperLeft);
    RectTransform descRT = descT.GetComponent<RectTransform>();
    descRT.anchorMin = new Vector2(0f, 0f);
    descRT.anchorMax = new Vector2(1f, 0.45f);
    descRT.offsetMin = new Vector2(8f, 6f);
    descRT.offsetMax = new Vector2(-8f, 0f);
    descT.horizontalOverflow = HorizontalWrapMode.Wrap;
    descT.verticalOverflow   = VerticalWrapMode.Truncate;

    return (btn, nameT, descT, iconImg);
}
```

- [ ] **Step 3: Actualizar Wire UIManager para cardIcons**

En el bloque `// ----- Wire UIManager -----`, añadir:

```csharp
SetRefArray(uiSO, "cardIcons", cardIcons);
```

- [ ] **Step 4: Reemplazar paneles Game Over, Victory y Pause con estilo**

Localizar `// ----- Game Over Panel -----` y reemplazar hasta el final de `// ----- Pause Panel -----`:

```csharp
// ----- Game Over Panel -----
GameObject gameOverPanel = CreatePanel(canvasGO, "GameOverPanel", new Color(0f, 0f, 0f, 0.80f));
gameOverPanel.SetActive(false);
GameObject goCenter = CreateStyledPanel(gameOverPanel, "GameOverCenter");
RectTransform goCenterRT = goCenter.GetComponent<RectTransform>();
goCenterRT.anchorMin = new Vector2(0.5f, 0.5f); goCenterRT.anchorMax = new Vector2(0.5f, 0.5f);
goCenterRT.pivot = new Vector2(0.5f, 0.5f);
goCenterRT.sizeDelta = new Vector2(500f, 300f); goCenterRT.anchoredPosition = Vector2.zero;
CreateResultText(goCenter, "ResultText", "GAME OVER", 36, new Color(0.957f, 0.263f, 0.212f, 1f), new Vector2(0f, 80f));
CreateRestartButton(goCenter, "RestartButton", "Reintentar", new Vector2(0f, -20f), gameManager);
Button goMenuBtn = CreateButton(goCenter, "MenuButton", "Menú Principal", new Vector2(0f, -110f));
UnityEventTools.AddPersistentListener(goMenuBtn.onClick,
    new UnityEngine.Events.UnityAction(gameManager.GoToMainMenu));

// ----- Victory Panel -----
GameObject victoryPanel = CreatePanel(canvasGO, "VictoryPanel", new Color(0f, 0f, 0f, 0.80f));
victoryPanel.SetActive(false);
GameObject vicCenter = CreateStyledPanel(victoryPanel, "VictoryCenter");
RectTransform vicCenterRT = vicCenter.GetComponent<RectTransform>();
vicCenterRT.anchorMin = new Vector2(0.5f, 0.5f); vicCenterRT.anchorMax = new Vector2(0.5f, 0.5f);
vicCenterRT.pivot = new Vector2(0.5f, 0.5f);
vicCenterRT.sizeDelta = new Vector2(500f, 300f); vicCenterRT.anchoredPosition = Vector2.zero;
CreateResultText(vicCenter, "ResultText", "¡VICTORIA!", 36, new Color(1f, 0.843f, 0f, 1f), new Vector2(0f, 80f));
CreateRestartButton(vicCenter, "RestartButton", "Reintentar", new Vector2(0f, -20f), gameManager);
Button vicMenuBtn = CreateButton(vicCenter, "MenuButton", "Menú Principal", new Vector2(0f, -110f));
UnityEventTools.AddPersistentListener(vicMenuBtn.onClick,
    new UnityEngine.Events.UnityAction(gameManager.GoToMainMenu));

// ----- Pause Panel -----
GameObject pausePanel = CreatePanel(canvasGO, "PausePanel", new Color(0f, 0f, 0f, 0.50f));
pausePanel.SetActive(false);
GameObject pauseCenter = CreateStyledPanel(pausePanel, "PauseCenter");
RectTransform pauseCenterRT = pauseCenter.GetComponent<RectTransform>();
pauseCenterRT.anchorMin = new Vector2(0.5f, 0.5f); pauseCenterRT.anchorMax = new Vector2(0.5f, 0.5f);
pauseCenterRT.pivot = new Vector2(0.5f, 0.5f);
pauseCenterRT.sizeDelta = new Vector2(400f, 420f); pauseCenterRT.anchoredPosition = Vector2.zero;
CreateResultText(pauseCenter, "PauseTitle", "PAUSA", 30, new Color(1f, 0.843f, 0f, 1f), new Vector2(0f, 160f));

PauseManager pauseManager = gmGO.AddComponent<PauseManager>();

Button continueBtn = CreateButton(pauseCenter, "ContinueButton", "Continuar",       new Vector2(0f,  60f));
Button settingsBtn = CreateButton(pauseCenter, "SettingsButton", "Ajustes",         new Vector2(0f, -30f));
Button menuBtn2    = CreateButton(pauseCenter, "MenuButton",     "Menú Principal",  new Vector2(0f, -120f));

UnityEventTools.AddPersistentListener(continueBtn.onClick,
    new UnityEngine.Events.UnityAction(pauseManager.ResumeGame));
UnityEventTools.AddPersistentListener(settingsBtn.onClick,
    new UnityEngine.Events.UnityAction(pauseManager.OpenSettings));
UnityEventTools.AddPersistentListener(menuBtn2.onClick,
    new UnityEngine.Events.UnityAction(pauseManager.QuitToMainMenu));

// ----- Settings Panel (pausa) -----
GameObject pauseSettingsPanel = CreateStyledPanel(canvasGO, "PauseSettingsPanel");
pauseSettingsPanel.SetActive(false);
RectTransform pspRT = pauseSettingsPanel.GetComponent<RectTransform>();
pspRT.anchorMin = new Vector2(0.5f, 0.5f); pspRT.anchorMax = new Vector2(0.5f, 0.5f);
pspRT.pivot = new Vector2(0.5f, 0.5f);
pspRT.sizeDelta = new Vector2(400f, 300f); pspRT.anchoredPosition = Vector2.zero;

CreateResultText(pauseSettingsPanel, "SettingsTitle", "AJUSTES", 24,
    new Color(1f, 0.843f, 0f, 1f), new Vector2(0f, 110f));

// Slider volumen
Slider volSlider = CreateSlider(pauseSettingsPanel, "VolumeSlider");
volSlider.minValue = 0f; volSlider.maxValue = 1f; volSlider.value = 1f;
RectTransform volRT = volSlider.GetComponent<RectTransform>();
volRT.anchorMin = new Vector2(0.5f, 0.5f); volRT.anchorMax = new Vector2(0.5f, 0.5f);
volRT.pivot = new Vector2(0.5f, 0.5f);
volRT.sizeDelta = new Vector2(320f, 30f);
volRT.anchoredPosition = new Vector2(0f, 30f);

CreateLegacyText(pauseSettingsPanel, "VolLabel", "VOLUMEN", 14,
    new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleCenter)
    .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 70f);

// Dropdown ventana
GameObject ddGO = new GameObject("WindowDropdown", typeof(RectTransform));
ddGO.transform.SetParent(pauseSettingsPanel.transform, false);
Image ddBg = ddGO.AddComponent<Image>();
ddBg.color = new Color(0.176f, 0.106f, 0.306f, 1f);
Dropdown windowDD = ddGO.AddComponent<Dropdown>();
windowDD.targetGraphic = ddBg;
RectTransform ddRT = ddGO.GetComponent<RectTransform>();
ddRT.anchorMin = new Vector2(0.5f, 0.5f); ddRT.anchorMax = new Vector2(0.5f, 0.5f);
ddRT.pivot = new Vector2(0.5f, 0.5f);
ddRT.sizeDelta = new Vector2(320f, 40f);
ddRT.anchoredPosition = new Vector2(0f, -30f);
Text ddLabel = CreateLegacyText(ddGO, "Label", "Ventana", 14,
    new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleLeft);
StretchToFill(ddLabel.GetComponent<RectTransform>());
windowDD.captionText = ddLabel;

Button backBtn = CreateButton(pauseSettingsPanel, "BackButton", "Atrás", new Vector2(0f, -110f));

// Wire PauseManager references
SerializedObject pmSO = new SerializedObject(pauseManager);
SetRef(pmSO, "pausePanel",         pausePanel);
SetRef(pmSO, "settingsPanel",      pauseSettingsPanel);
SetRef(pmSO, "masterVolumeSlider", volSlider);
SetRef(pmSO, "windowModeDropdown", windowDD);
pmSO.ApplyModifiedPropertiesWithoutUndo();

// Back button event
UnityEventTools.AddPersistentListener(backBtn.onClick,
    new UnityEngine.Events.UnityAction(pauseManager.CloseSettings));

// Shop Panel
GameObject shopPanel = CreateStyledPanel(canvasGO, "ShopPanel");
shopPanel.SetActive(false);
RectTransform shopRT = shopPanel.GetComponent<RectTransform>();
shopRT.anchorMin = new Vector2(0.5f, 0.5f); shopRT.anchorMax = new Vector2(0.5f, 0.5f);
shopRT.pivot = new Vector2(0.5f, 0.5f);
shopRT.sizeDelta = new Vector2(700f, 500f); shopRT.anchoredPosition = Vector2.zero;

CreateResultText(shopPanel, "ShopTitle", "TIENDA", 28,
    new Color(1f, 0.843f, 0f, 1f), new Vector2(0f, 210f));

Button closeShopBtn = CreateButton(shopPanel, "CloseButton", "✕", new Vector2(290f, 210f));
closeShopBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);

// Grid para ítems
GameObject gridGO = new GameObject("ItemGrid", typeof(RectTransform));
gridGO.transform.SetParent(shopPanel.transform, false);
GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
grid.cellSize = new Vector2(120f, 150f);
grid.spacing  = new Vector2(15f, 15f);
grid.padding  = new RectOffset(15, 15, 15, 15);
RectTransform gridRT = gridGO.GetComponent<RectTransform>();
gridRT.anchorMin = Vector2.zero; gridRT.anchorMax = Vector2.one;
gridRT.offsetMin = new Vector2(0f, 0f); gridRT.offsetMax = new Vector2(0f, -60f);

// ShopManager con ShopUI
ShopUI shopUI = shopGO.AddComponent<ShopUI>();
SerializedObject shopUISO = new SerializedObject(shopUI);
SetRef(shopUISO, "shopPanel", shopPanel);
SetRef(shopUISO, "itemGrid",  gridGO.transform as Object);
shopUISO.ApplyModifiedPropertiesWithoutUndo();
```

- [ ] **Step 5: Actualizar Wire UIManager para shopPanel**

En el bloque Wire UIManager, añadir:

```csharp
SetRef(uiSO, "shopPanel", shopPanel);
```

Y en el `SetRefArray` de cardIcons — asegurarse de que también incluye:

```csharp
SetRefArray(uiSO, "cardIcons", cardIcons);
```

- [ ] **Step 6: Commit**

```bash
git add Assets/Editor/AstralSwarmSetup.cs
git commit -m "feat: AstralSwarmSetup — styled level-up, pause, settings, shop panels"
```

---

## Task 10: SetupMainMenuScene

**Files:**
- Modify: `Assets/Editor/AstralSwarmSetup.cs`

- [ ] **Step 1: Añadir el MenuItem y el método**

Añadir antes del cierre `#endif` final:

```csharp
private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

[MenuItem("Astral Swarm/Setup Main Menu Scene")]
public static void SetupMainMenuScene()
{
    Scene scene = System.IO.File.Exists(MainMenuScenePath)
        ? EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single)
        : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    foreach (GameObject root in scene.GetRootGameObjects())
        Object.DestroyImmediate(root);

    // Camera
    GameObject camGO = new GameObject("Main Camera");
    camGO.tag = "MainCamera";
    Camera cam = camGO.AddComponent<Camera>();
    cam.orthographic = true;
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = new Color(0.039f, 0f, 0.078f, 1f); // #0A0014
    camGO.AddComponent<AudioListener>();

    // Canvas
    GameObject canvasGO = new GameObject("Canvas");
    Canvas canvas = canvasGO.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920f, 1080f);
    scaler.matchWidthOrHeight = 0.5f;
    canvasGO.AddComponent<GraphicRaycaster>();

    // Fondo
    GameObject bgGO = new GameObject("Background", typeof(RectTransform));
    bgGO.transform.SetParent(canvasGO.transform, false);
    Image bgImg = bgGO.AddComponent<Image>();
    bgImg.color = new Color(0.039f, 0f, 0.078f, 1f);
    StretchToFill(bgGO.GetComponent<RectTransform>());

    // Panel principal
    GameObject mainPanel = CreateStyledPanel(canvasGO, "MainPanel");
    RectTransform mpRT = mainPanel.GetComponent<RectTransform>();
    mpRT.anchorMin = new Vector2(0.5f, 0.5f); mpRT.anchorMax = new Vector2(0.5f, 0.5f);
    mpRT.pivot = new Vector2(0.5f, 0.5f);
    mpRT.sizeDelta = new Vector2(500f, 500f); mpRT.anchoredPosition = Vector2.zero;

    // Título
    Text titleText = CreateLegacyText(mainPanel, "TitleText", "ASTRAL SWARM", 48,
        new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleCenter);
    Shadow titleShadow = titleText.gameObject.AddComponent<Shadow>();
    titleShadow.effectColor = new Color(0.5f, 0.2f, 0f, 1f);
    titleShadow.effectDistance = new Vector2(3f, -3f);
    Outline titleOutline = titleText.gameObject.AddComponent<Outline>();
    titleOutline.effectColor = new Color(1f, 0.6f, 0f, 0.5f);
    titleOutline.effectDistance = new Vector2(1f, -1f);
    RectTransform titleRT = titleText.GetComponent<RectTransform>();
    titleRT.anchorMin = new Vector2(0f, 0.75f); titleRT.anchorMax = new Vector2(1f, 1f);
    titleRT.offsetMin = Vector2.zero; titleRT.offsetMax = Vector2.zero;

    // Botones menú
    GameObject managerGO = new GameObject("MainMenuManager");
    MainMenuManager mmManager = managerGO.AddComponent<MainMenuManager>();

    Button playBtn     = CreateButton(mainPanel, "PlayButton",     "JUGAR",    new Vector2(0f,  100f));
    Button settingsBtn = CreateButton(mainPanel, "SettingsButton", "AJUSTES",  new Vector2(0f,    0f));
    Button quitBtn     = CreateButton(mainPanel, "QuitButton",     "SALIR",    new Vector2(0f, -100f));

    UnityEventTools.AddPersistentListener(playBtn.onClick,
        new UnityEngine.Events.UnityAction(mmManager.PlayGame));
    UnityEventTools.AddPersistentListener(settingsBtn.onClick,
        new UnityEngine.Events.UnityAction(mmManager.ShowSettings));
    UnityEventTools.AddPersistentListener(quitBtn.onClick,
        new UnityEngine.Events.UnityAction(mmManager.QuitGame));

    // Panel Ajustes
    GameObject settingsPanel = CreateStyledPanel(canvasGO, "SettingsPanel");
    settingsPanel.SetActive(false);
    RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
    spRT.anchorMin = new Vector2(0.5f, 0.5f); spRT.anchorMax = new Vector2(0.5f, 0.5f);
    spRT.pivot = new Vector2(0.5f, 0.5f);
    spRT.sizeDelta = new Vector2(500f, 380f); spRT.anchoredPosition = Vector2.zero;

    CreateResultText(settingsPanel, "SettingsTitle", "AJUSTES", 28,
        new Color(1f, 0.843f, 0f, 1f), new Vector2(0f, 150f));

    // Slider volumen
    Slider volSlider = CreateSlider(settingsPanel, "VolumeSlider");
    volSlider.minValue = 0f; volSlider.maxValue = 1f; volSlider.value = 1f;
    RectTransform volRT = volSlider.GetComponent<RectTransform>();
    volRT.anchorMin = new Vector2(0.5f, 0.5f); volRT.anchorMax = new Vector2(0.5f, 0.5f);
    volRT.pivot = new Vector2(0.5f, 0.5f);
    volRT.sizeDelta = new Vector2(380f, 30f); volRT.anchoredPosition = new Vector2(0f, 50f);

    CreateLegacyText(settingsPanel, "VolLabel", "VOLUMEN", 14,
        new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleCenter)
        .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 90f);

    // Dropdown modo ventana
    GameObject ddGO = new GameObject("WindowDropdown", typeof(RectTransform));
    ddGO.transform.SetParent(settingsPanel.transform, false);
    Image ddBg = ddGO.AddComponent<Image>();
    ddBg.color = new Color(0.176f, 0.106f, 0.306f, 1f);
    Dropdown windowDD = ddGO.AddComponent<Dropdown>();
    windowDD.targetGraphic = ddBg;
    RectTransform ddRT = ddGO.GetComponent<RectTransform>();
    ddRT.anchorMin = new Vector2(0.5f, 0.5f); ddRT.anchorMax = new Vector2(0.5f, 0.5f);
    ddRT.pivot = new Vector2(0.5f, 0.5f);
    ddRT.sizeDelta = new Vector2(380f, 40f); ddRT.anchoredPosition = new Vector2(0f, -10f);
    Text ddLabel = CreateLegacyText(ddGO, "Label", "Ventana", 14,
        new Color(1f, 0.843f, 0f, 1f), TextAnchor.MiddleLeft);
    StretchToFill(ddLabel.GetComponent<RectTransform>());
    windowDD.captionText = ddLabel;

    Button backBtn = CreateButton(settingsPanel, "BackButton", "ATRÁS", new Vector2(0f, -100f));
    UnityEventTools.AddPersistentListener(backBtn.onClick,
        new UnityEngine.Events.UnityAction(mmManager.HideSettings));

    // Wire MainMenuManager
    SerializedObject mmSO = new SerializedObject(mmManager);
    SetRef(mmSO, "mainPanel",           mainPanel);
    SetRef(mmSO, "settingsPanel",       settingsPanel);
    SetRef(mmSO, "masterVolumeSlider",  volSlider);
    mmSO.ApplyModifiedPropertiesWithoutUndo();

    // Wire dropdown en MainMenuManager
    UnityEventTools.AddPersistentListener(windowDD.onValueChanged, (UnityEngine.Events.UnityAction<int>)((int idx) => {}));

    // EventSystem
    GameObject esGO = new GameObject("EventSystem");
    esGO.AddComponent<EventSystem>();
    esGO.AddComponent<StandaloneInputModule>();

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

    EditorUtility.DisplayDialog("Astral Swarm",
        "¡Menú Principal configurado! Añade MainMenu a Build Settings.", "OK");
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Editor/AstralSwarmSetup.cs
git commit -m "feat: AstralSwarmSetup — SetupMainMenuScene with fantasy style"
```

---

## Task 11: Conectar MainMenuManager con dropdown de ventana

**Files:**
- Modify: `Assets/Scripts/MainMenuManager.cs`

El `MainMenuManager.cs` ya tiene `SetWindowed/SetFullscreen/SetBorderless` pero no conecta el Dropdown automáticamente.

- [ ] **Step 1: Añadir conexión dropdown en Start()**

En `MainMenuManager.Start()`, añadir tras la inicialización de sliders:

```csharp
[Header("Window Mode")]
public Dropdown windowModeDropdown;
```

Y en `Start()`, tras las líneas de sliders existentes:

```csharp
if (windowModeDropdown != null)
{
    windowModeDropdown.ClearOptions();
    windowModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        { "Ventana", "Pantalla Completa", "Sin Bordes" });
    windowModeDropdown.onValueChanged.AddListener(idx => {
        switch (idx) {
            case 0: SetWindowed(); break;
            case 1: SetFullscreen(); break;
            case 2: SetBorderless(); break;
        }
    });
}
```

- [ ] **Step 2: Añadir wire del dropdown en SetupMainMenuScene**

En `SetupMainMenuScene()`, en el bloque Wire MainMenuManager, añadir:

```csharp
SetRef(mmSO, "windowModeDropdown", windowDD);
```

Y eliminar la línea provisional:
```csharp
UnityEventTools.AddPersistentListener(windowDD.onValueChanged, (UnityEngine.Events.UnityAction<int>)((int idx) => {}));
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/MainMenuManager.cs Assets/Editor/AstralSwarmSetup.cs
git commit -m "feat: MainMenuManager — wire window mode dropdown in Start"
```

---

## Task 12: Ejecutar setup en Unity y verificar

- [ ] **Step 1: Ejecutar "Astral Swarm > Setup Game Scene"** en Unity Editor
  - Verificar que la consola no muestra errores
  - Verificar que el Canvas tiene: StatsPanel, XpPanel, TimerText, LevelUpPanel, PausePanel, PauseSettingsPanel, ShopPanel

- [ ] **Step 2: Ejecutar "Astral Swarm > Setup Main Menu Scene"**
  - Verificar que la escena MainMenu tiene: MainPanel (título + 3 botones), SettingsPanel (slider + dropdown)

- [ ] **Step 3: Añadir ambas escenas a Build Settings**
  - File > Build Settings > Add Open Scenes
  - Orden: 0 = MainMenu, 1 = Game

- [ ] **Step 4: Verificar en Play Mode (escena Game)**
  - HUD visible: vida roja, nivel dorado, oro amarillo, barra XP morada, timer
  - ESC abre menú pausa con estilo fantasy y fondo semitransparente
  - ESC en pausa → Ajustes → slider + dropdown funcionan → Atrás vuelve
  - Recibir daño actualiza el contador de vida
  - Subir de nivel muestra las 3 cartas con borde dorado y hover effect

- [ ] **Step 5: Verificar en Play Mode (escena MainMenu)**
  - Título "ASTRAL SWARM" en dorado con sombra
  - JUGAR carga la escena Game
  - AJUSTES muestra panel con slider y dropdown
  - SALIR cierra la aplicación (en Editor: sale del Play Mode)

- [ ] **Step 6: Commit final**

```bash
git add -A
git commit -m "feat: UI redesign completo — HUD, level-up, shop, menú principal y pausa con estética fantasy"
```
