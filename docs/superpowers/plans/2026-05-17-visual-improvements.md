# Visual Improvements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add four visual systems to Astral Swarm: infinite tiled world background, animated experience gems with color tiers, ESC pause menu, and an icon-based minimap showing enemies and player.

**Architecture:** Each system is a self-contained MonoBehaviour or UIManager extension. WorldBackground and MinimapController are new scripts. ExperienceGem.cs is updated in-place. GameManager gets ESC handling. All four systems are wired into the scene via a final AstralSwarmSetup update — re-running "Astral Swarm/Setup Game Scene" rebuilds the scene with everything connected.

**Tech Stack:** Unity 6.0.3, C# MonoBehaviours, Unity UI (UnityEngine.UI), Texture2D procedural sprites, SerializedObject for Editor wiring.

---

## File Map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `Assets/Scripts/WorldBackground.cs` | Infinite tiling floor using a 9×9 pool of SpriteRenderer tiles |
| Modify | `Assets/Scripts/ExperienceGem.cs` | Add circle sprite, bob animation, color tiers |
| Modify | `Assets/Scripts/GameManager.cs` | Add ESC toggle for pause menu |
| Modify | `Assets/Scripts/UIManager.cs` | Add `pausePanel` field and `ShowPauseMenu()` |
| Create | `Assets/Scripts/MinimapController.cs` | Icon-based minimap: player (white) + enemy (red) dots |
| Modify | `Assets/Editor/AstralSwarmSetup.cs` | Wire all four systems into scene + configure newtileset.png |

---

### Task 1: WorldBackground — Infinite Tiling Floor

**Files:**
- Create: `Assets/Scripts/WorldBackground.cs`

- [ ] **Step 1: Create WorldBackground.cs**

```csharp
// Assets/Scripts/WorldBackground.cs
using UnityEngine;

public class WorldBackground : MonoBehaviour
{
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private int tilesX = 9;   // must be odd
    [SerializeField] private int tilesY = 9;   // must be odd
    [SerializeField] private float tileSize = 2f;

    private Transform cameraTransform;
    private Transform[] tiles;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        Sprite sprite = tileSprite != null ? tileSprite : BuildFallbackTile();
        float scale = tileSize / sprite.bounds.size.x;

        tiles = new Transform[tilesX * tilesY];
        for (int i = 0; i < tiles.Length; i++)
        {
            var go = new GameObject("BgTile");
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            tiles[i] = go.transform;
        }
    }

    private void LateUpdate()
    {
        Vector3 cam  = cameraTransform.position;
        float snapX  = Mathf.Round(cam.x / tileSize) * tileSize;
        float snapY  = Mathf.Round(cam.y / tileSize) * tileSize;
        int halfX    = tilesX / 2;
        int halfY    = tilesY / 2;

        int idx = 0;
        for (int y = -halfY; y <= halfY; y++)
            for (int x = -halfX; x <= halfX; x++)
                tiles[idx++].position = new Vector3(snapX + x * tileSize, snapY + y * tileSize, 0f);
    }

    private static Sprite BuildFallbackTile()
    {
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var bg   = new Color(0.10f, 0.10f, 0.18f, 1f);
        var line = new Color(0.16f, 0.16f, 0.28f, 1f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, (x == 0 || y == 0) ? line : bg);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
```

- [ ] **Step 2: Verify compilation**

Open Unity. Check the Console — no errors. WorldBackground should appear in the component-add menu.

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/WorldBackground.cs"
git commit -m "feat: add WorldBackground infinite tiling floor"
```

---

### Task 2: Visual Experience Gems — Circle Sprite + Bob + Color Tiers

**Files:**
- Modify: `Assets/Scripts/ExperienceGem.cs`

Current state: 65-line script with magnet logic, no visual, `sr.color = Color.cyan` from prefab.

- [ ] **Step 1: Replace ExperienceGem.cs**

```csharp
// Assets/Scripts/ExperienceGem.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private int   experienceAmount = 10;
    [SerializeField] private float magnetSpeed  = 8f;
    [SerializeField] private float bobHeight    = 0.12f;
    [SerializeField] private float bobSpeed     = 3f;

    private Transform      playerTarget;
    private bool           isMagnetized;
    private SpriteRenderer sr;
    private float          spawnY;
    private float          bobPhase;

    private static Sprite sharedCircle;

    private void Awake()
    {
        sr        = GetComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        spawnY    = transform.position.y;
        bobPhase  = Random.Range(0f, Mathf.PI * 2f);
        ApplyColorTier();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (isMagnetized && playerTarget != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, playerTarget.position, magnetSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, playerTarget.position) < 0.2f)
                CollectGem();
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = spawnY + Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobHeight;
            transform.position = pos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMagnetized)
        {
            playerTarget = collision.transform;
            isMagnetized = true;
        }
    }

    private void ApplyColorTier()
    {
        if      (experienceAmount <= 10) sr.color = new Color(0.3f,  1f,   0.4f); // green
        else if (experienceAmount <= 30) sr.color = new Color(0.3f,  0.6f, 1f);   // blue
        else                             sr.color = new Color(0.85f, 0.3f, 1f);   // purple
    }

    private static Sprite GetCircleSprite()
    {
        if (sharedCircle != null) return sharedCircle;

        const int size   = 16;
        const float ppu  = 16f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float radius = center - 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - center, dy = y - center;
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        tex.Apply();

        sharedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        return sharedCircle;
    }

    private void CollectGem()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.AddExperience(experienceAmount);
        Destroy(gameObject);
    }
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, check Console — no errors. The `[RequireComponent(typeof(SpriteRenderer))]` attribute is new; the existing ExperienceGem prefab already has a SpriteRenderer so this won't break anything.

- [ ] **Step 3: Enter Play Mode and verify**

Run the scene. Kill an enemy — a colored circle gem should appear, bob up and down slowly, and fly toward the player on contact. Green for default (10 exp).

- [ ] **Step 4: Commit**

```
git add "Assets/Scripts/ExperienceGem.cs"
git commit -m "feat: visual experience gems with bob animation and color tiers"
```

---

### Task 3: Pause Menu — ESC Toggle with Resume / Restart / Quit

**Files:**
- Modify: `Assets/Scripts/UIManager.cs` (add `pausePanel` + `ShowPauseMenu`)
- Modify: `Assets/Scripts/GameManager.cs` (add ESC handling, update `ResumeGame`)

- [ ] **Step 1: Add pausePanel to UIManager.cs**

In `Assets/Scripts/UIManager.cs`, add after the `victoryPanel` field:

```csharp
    [Header("Pause Menu")]
    public GameObject pausePanel;
```

Add after `ShowVictory()`:

```csharp
    public void ShowPauseMenu(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }
```

Also add `pausePanel` to the `Start()` hide block:

```csharp
    private void Start()
    {
        if (levelUpPanel)  levelUpPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel)  victoryPanel.SetActive(false);
        if (pausePanel)    pausePanel.SetActive(false);   // ADD THIS LINE
    }
```

- [ ] **Step 2: Add ESC handling to GameManager.cs**

In `Assets/Scripts/GameManager.cs`, at the **top** of `Update()`, before the early return:

```csharp
    private void Update()
    {
        // ESC toggles pause (not during level-up or game-over)
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

        if (isGameOver || isPaused) return;
        // ... rest unchanged
```

- [ ] **Step 3: Update ResumeGame() to also hide pause panel**

In `Assets/Scripts/GameManager.cs`, update `ResumeGame()`:

```csharp
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (uiManager != null)
        {
            uiManager.ShowLevelUpMenu(false);
            uiManager.ShowPauseMenu(false);
        }
    }
```

- [ ] **Step 4: Verify compilation**

Open Unity, check Console — no errors.

- [ ] **Step 5: Commit**

```
git add "Assets/Scripts/UIManager.cs" "Assets/Scripts/GameManager.cs"
git commit -m "feat: pause menu with ESC toggle"
```

---

### Task 4: MinimapController — Icon-Based Minimap

**Files:**
- Create: `Assets/Scripts/MinimapController.cs`

- [ ] **Step 1: Create MinimapController.cs**

```csharp
// Assets/Scripts/MinimapController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    [Tooltip("How many world units from the player the minimap edge represents.")]
    [SerializeField] private float worldRadius = 25f;

    private RectTransform    panel;
    private Transform        playerTransform;
    private readonly List<Image> dotPool = new List<Image>();
    private int usedDots;

    private static readonly Color ColorPlayer = Color.white;
    private static readonly Color ColorEnemy  = new Color(1f, 0.25f, 0.2f);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p) playerTransform = p.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        usedDots = 0;

        // Player dot at center
        PlaceDot(Vector2.zero, ColorPlayer, 10f);

        // Enemy dots relative to player
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector2 offset = (Vector2)(enemy.transform.position - playerTransform.position);
            Vector2 norm   = Vector2.ClampMagnitude(offset / worldRadius, 1f);
            PlaceDot(norm, ColorEnemy, 6f);
        }

        // Hide unused pooled dots
        for (int i = usedDots; i < dotPool.Count; i++)
            dotPool[i].gameObject.SetActive(false);
    }

    private void PlaceDot(Vector2 normalizedPos, Color color, float size)
    {
        Image dot   = GetDot(usedDots++);
        var   rt    = dot.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(
            normalizedPos.x * panel.rect.width  * 0.5f,
            normalizedPos.y * panel.rect.height * 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        dot.color    = color;
        dot.gameObject.SetActive(true);
    }

    private Image GetDot(int index)
    {
        if (index < dotPool.Count) return dotPool[index];
        var go = new GameObject("MinimapDot", typeof(RectTransform));
        go.transform.SetParent(panel, false);
        var img = go.AddComponent<Image>();
        dotPool.Add(img);
        return img;
    }
}
```

- [ ] **Step 2: Verify compilation**

Open Unity, check Console — no errors.

- [ ] **Step 3: Commit**

```
git add "Assets/Scripts/MinimapController.cs"
git commit -m "feat: icon-based minimap controller"
```

---

### Task 5: Scene Integration — Wire All Four Systems via AstralSwarmSetup

**Files:**
- Modify: `Assets/Editor/AstralSwarmSetup.cs`

This task adds all four systems to the scene rebuild and configures `newtileset.png` as a single sprite for optional use as the floor tile.

- [ ] **Step 1: Add ConfigureSingleSprite helper method**

In `AstralSwarmSetup.cs`, add this method after `ConfigureSpriteSheet()`:

```csharp
    private static void ConfigureSingleSprite(string assetPath, int ppu)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[AstralSwarmSetup] Sprite not found: {assetPath}"); return; }
        importer.textureType        = TextureImporterType.Sprite;
        importer.spriteImportMode   = SpriteImportMode.Single;
        importer.filterMode         = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = ppu;
        importer.SaveAndReimport();
    }
```

- [ ] **Step 2: Call ConfigureSingleSprite for newtileset.png in ConfigureAllSpriteSheets()**

At the end of `ConfigureAllSpriteSheets()`, before `AssetDatabase.Refresh()`:

```csharp
        ConfigureSingleSprite("Assets/Sprites/Player/newtileset.png", 16);
```

- [ ] **Step 3: Add WorldBackground to SetupScene()**

In `SetupScene()`, after the `shopGO` block and before the player instantiation:

```csharp
        // ----- World Background -----
        GameObject bgGO = new GameObject("WorldBackground");
        WorldBackground wb = bgGO.AddComponent<WorldBackground>();
        // Try to wire newtileset.png if it has already been configured as a sprite
        Sprite tileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/newtileset.png");
        if (tileSprite != null)
        {
            SerializedObject wbSO = new SerializedObject(wb);
            SerializedProperty tileProp = wbSO.FindProperty("tileSprite");
            if (tileProp != null) { tileProp.objectReferenceValue = tileSprite; wbSO.ApplyModifiedPropertiesWithoutUndo(); }
        }
        // If tileSprite is null the script uses its procedural fallback tile at runtime.
```

- [ ] **Step 4: Add Pause Panel to the canvas in SetupScene()**

In `SetupScene()`, after the Victory Panel block and before the EventSystem block:

```csharp
        // ----- Pause Panel -----
        GameObject pausePanel = CreatePanel(canvasGO, "PausePanel", new Color(0f, 0f, 0f, 0.8f));
        pausePanel.SetActive(false);
        CreateResultText(pausePanel, "PauseTitle", "PAUSA", 36, Color.white, new Vector2(0f, 120f));
        Button resumeBtn  = CreateButton(pausePanel, "ResumeButton",   "Continuar", new Vector2(0f,   30f));
        Button restartBtn = CreateButton(pausePanel, "RestartButton2", "Reiniciar", new Vector2(0f,  -70f));
        Button quitBtn    = CreateButton(pausePanel, "QuitButton",     "Salir",     new Vector2(0f, -170f));
        UnityEventTools.AddPersistentListener(resumeBtn.onClick,
            new UnityEngine.Events.UnityAction(gameManager.ResumeGame));
        UnityEventTools.AddPersistentListener(restartBtn.onClick,
            new UnityEngine.Events.UnityAction(gameManager.RestartGame));
        UnityEventTools.AddPersistentListener(quitBtn.onClick,
            new UnityEngine.Events.UnityAction(gameManager.GoToMainMenu));
```

- [ ] **Step 5: Add Minimap panel to the canvas in SetupScene()**

In `SetupScene()`, after the Pause Panel block:

```csharp
        // ----- Minimap -----
        GameObject minimapGO = new GameObject("Minimap", typeof(RectTransform));
        minimapGO.transform.SetParent(canvasGO.transform, false);
        Image minimapBg = minimapGO.AddComponent<Image>();
        minimapBg.color = new Color(0f, 0f, 0f, 0.5f);
        minimapGO.AddComponent<MinimapController>();

        RectTransform minimapRT = minimapGO.GetComponent<RectTransform>();
        minimapRT.anchorMin       = new Vector2(1f, 0f);
        minimapRT.anchorMax       = new Vector2(1f, 0f);
        minimapRT.pivot           = new Vector2(1f, 0f);
        minimapRT.sizeDelta       = new Vector2(150f, 150f);
        minimapRT.anchoredPosition = new Vector2(-20f, 20f);
```

- [ ] **Step 6: Wire UIManager.pausePanel in the SetupScene() UIManager wiring block**

Find the existing block:

```csharp
        SetRef(uiSO, "victoryPanel", victoryPanel);
        SetRefArray(uiSO, "levelUpCards",   cards);
```

Add the pause panel wire before `SetRefArray`:

```csharp
        SetRef(uiSO, "pausePanel",   pausePanel);
        SetRefArray(uiSO, "levelUpCards",   cards);
```

- [ ] **Step 7: Verify compilation**

Open Unity, check Console — no errors. Both "Astral Swarm/Setup Game Scene" and "Astral Swarm/Configure Sprites and Animator" menu items should still exist.

- [ ] **Step 8: Run the setup menu items**

In Unity:
1. `Astral Swarm → Setup Game Scene` — rebuilds the scene with all four systems
2. `Astral Swarm → Configure Sprites and Animator` — re-slices sprites and assigns animator

After both complete, the scene hierarchy should contain:
- `WorldBackground` (with 81 BgTile children visible after Play)
- `Canvas/PausePanel` (initially hidden)
- `Canvas/Minimap` (bottom-right corner)

- [ ] **Step 9: Enter Play Mode and do a full smoke test**

| Feature | How to test | Expected result |
|---------|-------------|-----------------|
| World background | Move the player | Dark tiled floor follows camera infinitely |
| Visual gems | Kill an enemy | Colored glowing circle appears, bobs, flies to player |
| Pause menu | Press ESC | Dark overlay appears with "PAUSA", Continuar / Reiniciar / Salir buttons |
| Pause → resume | Press ESC again or click Continuar | Game resumes, panel hides |
| Pause + level-up | Level up, then press ESC | ESC does nothing while level-up cards are visible |
| Minimap | Move player near enemies | White dot (player) at center, red dots (enemies) around it |

- [ ] **Step 10: Commit**

```
git add "Assets/Editor/AstralSwarmSetup.cs"
git commit -m "feat: wire WorldBackground, pause menu, and minimap into scene setup"
```
