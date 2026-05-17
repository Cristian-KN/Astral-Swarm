using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
public static class AstralSwarmSetup
{
    // ---------- Paths ----------
    private const string PrefabsFolder   = "Assets/Prefabs";
    private const string SpritesFolder   = "Assets/Sprites";
    private const string AnimationsFolder = "Assets/Animations";
    private const string ScenesFolder    = "Assets/Scenes";
    private const string ItemsFolder     = "Assets/Items";
    private const string ScenePath       = "Assets/Scenes/Game.unity";

    private const string ProjectilePrefabPath   = "Assets/Prefabs/Projectile.prefab";
    private const string ExperienceGemPrefabPath = "Assets/Prefabs/ExperienceGem.prefab";
    private const string EnemyPrefabPath         = "Assets/Prefabs/Enemy.prefab";
    private const string PlayerPrefabPath        = "Assets/Prefabs/Player.prefab";

    private const string PlayerTag = "Player";
    private const string EnemyTag  = "Enemy";

    // =====================================================================
    // ENTRY POINT
    // =====================================================================

    [MenuItem("Astral Swarm/Setup Game Scene")]
    public static void SetupGameScene()
    {
        try
        {
            CreateFolders();

            GameObject projectilePrefab    = CreateProjectilePrefab();
            GameObject experienceGemPrefab = CreateExperienceGemPrefab();

            List<ItemData> pool = CreateItemDataAssets(projectilePrefab);
            ItemData defaultWeapon = pool[0]; // "Magic Orb" is always first

            GameObject enemyPrefab  = CreateEnemyPrefab(experienceGemPrefab);
            GameObject playerPrefab = CreatePlayerPrefab(defaultWeapon);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetupScene(playerPrefab, enemyPrefab, pool);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AstralSwarmSetup] Setup failed: " + ex);
            EditorUtility.DisplayDialog("Astral Swarm Setup",
                "El setup falló: " + ex.Message + "\nRevisa la consola.", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Astral Swarm Setup",
            "¡Setup completo! Ahora ejecuta 'Configure Sprites and Animator' y luego dale a Play.", "OK");
    }

    // =====================================================================
    // FOLDERS
    // =====================================================================

    private static void CreateFolders()
    {
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Editor");
        EnsureFolder("Assets", "Sprites");
        EnsureFolder("Assets/Sprites", "Player");
        EnsureFolder("Assets/Sprites", "Enemies");
        EnsureFolder("Assets", "Animations");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Items");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }

    // =====================================================================
    // ITEM DATA ASSETS
    // =====================================================================

    private static List<ItemData> CreateItemDataAssets(GameObject projectilePrefab)
    {
        var pool = new List<ItemData>();

        pool.Add(MakeWeapon("MagicOrb",     "Orbe Mágico",   "Dispara proyectiles a enemigos cercanos.",
            projectilePrefab, cooldown: 1.0f, damage: 25, radius: 5f));

        pool.Add(MakeWeapon("ShadowDart",   "Dardo Sombrío", "Disparo rápido con bajo daño.",
            projectilePrefab, cooldown: 0.5f, damage: 12, radius: 4f));

        pool.Add(MakeStat("SpeedBoots",  "Botas de Velocidad",   "+20% velocidad de movimiento.",
            speedBoost: 0.2f));

        pool.Add(MakeStat("IronShield",  "Escudo de Hierro",     "+5 defensa plana.",
            defense: 5f));

        pool.Add(MakeStat("PowerCrystal","Cristal de Poder",     "+10 poder de ataque.",
            attack: 10f));

        pool.Add(MakeGrowth("GrowthSeed","Semilla de Crecimiento",
            "Gana fuerza con cada enemigo eliminado.", growthPerKill: 0.05f));

        AssetDatabase.SaveAssets();
        return pool;
    }

    private static ItemData MakeWeapon(string file, string name, string desc,
        GameObject prefab, float cooldown, int damage, float radius)
    {
        string path = $"{ItemsFolder}/{file}.asset";
        ItemData existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null) return existing;

        ItemData d = ScriptableObject.CreateInstance<ItemData>();
        d.itemName              = name;
        d.description           = desc;
        d.type                  = ItemType.Weapon;
        d.projectilePrefab      = prefab;
        d.weaponCooldown        = cooldown;
        d.weaponDamage          = damage;
        d.weaponDetectionRadius = radius;
        d.weaponMaxLevel        = 5;
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    private static ItemData MakeStat(string file, string name, string desc,
        float speedBoost = 0f, float attack = 0f, float defense = 0f)
    {
        string path = $"{ItemsFolder}/{file}.asset";
        ItemData existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null) return existing;

        ItemData d = ScriptableObject.CreateInstance<ItemData>();
        d.itemName    = name;
        d.description = desc;
        d.type        = ItemType.Stat;
        d.speedBoost   = speedBoost;
        d.attackBoost  = attack;
        d.defenseBoost = defense;
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    private static ItemData MakeGrowth(string file, string name, string desc, float growthPerKill)
    {
        string path = $"{ItemsFolder}/{file}.asset";
        ItemData existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null) return existing;

        ItemData d = ScriptableObject.CreateInstance<ItemData>();
        d.itemName      = name;
        d.description   = desc;
        d.type          = ItemType.Growth;
        d.isGrowthItem  = true;
        d.growthPerKill = growthPerKill;
        AssetDatabase.CreateAsset(d, path);
        return d;
    }

    // =====================================================================
    // PREFABS
    // =====================================================================

    private static GameObject CreateProjectilePrefab()
    {
        GameObject go = new GameObject("Projectile");

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Projectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateExperienceGemPrefab()
    {
        GameObject go = new GameObject("ExperienceGem");
        go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;

        go.AddComponent<ExperienceGem>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, ExperienceGemPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab(GameObject experienceGemPrefab)
    {
        GameObject go = new GameObject("Enemy");
        TrySetTag(go, EnemyTag);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1) { Debug.LogWarning("Layer 'Enemy' not found, using 6."); enemyLayer = 6; }
        go.layer = enemyLayer;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.4f;

        go.AddComponent<SpriteRenderer>();
        go.AddComponent<EnemyAI>();
        EnemyStats stats = go.AddComponent<EnemyStats>();
        go.AddComponent<EnemyColorizer>();

        SerializedObject so = new SerializedObject(stats);
        SerializedProperty gemProp = so.FindProperty("experienceGemPrefab");
        if (gemProp != null) { gemProp.objectReferenceValue = experienceGemPrefab; so.ApplyModifiedPropertiesWithoutUndo(); }
        else Debug.LogWarning("[AstralSwarmSetup] 'experienceGemPrefab' not found on EnemyStats.");

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreatePlayerPrefab(ItemData defaultWeapon)
    {
        GameObject go = new GameObject("Player");
        TrySetTag(go, PlayerTag);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.6f, 0.9f);

        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Animator>();
        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerStats>();
        PlayerAttack playerAttack = go.AddComponent<PlayerAttack>();
        go.AddComponent<InventoryManager>();

        SerializedObject so = new SerializedObject(playerAttack);

        SerializedProperty weaponProp = so.FindProperty("defaultWeapon");
        if (weaponProp != null) weaponProp.objectReferenceValue = defaultWeapon;
        else Debug.LogWarning("[AstralSwarmSetup] 'defaultWeapon' not found on PlayerAttack.");

        SerializedProperty layerProp = so.FindProperty("enemyLayer");
        if (layerProp != null)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1) enemyLayer = 6;
            layerProp.intValue = 1 << enemyLayer;
        }
        else Debug.LogWarning("[AstralSwarmSetup] 'enemyLayer' not found on PlayerAttack.");

        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PlayerPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void TrySetTag(GameObject go, string tag)
    {
        try { go.tag = tag; }
        catch (UnityException) { Debug.LogWarning("[AstralSwarmSetup] Tag '" + tag + "' not defined."); }
    }

    // =====================================================================
    // SCENE
    // =====================================================================

    private static void SetupScene(GameObject playerPrefab, GameObject enemyPrefab, List<ItemData> itemPool)
    {
        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        foreach (GameObject root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        // ----- Managers -----
        GameObject gmGO = new GameObject("GameManager");
        GameManager gameManager = gmGO.AddComponent<GameManager>();
        UIManager uiManager = gmGO.AddComponent<UIManager>();

        // Wire GameManager itemPool
        SerializedObject gmSO = new SerializedObject(gameManager);
        SerializedProperty poolProp = gmSO.FindProperty("itemPool");
        if (poolProp != null)
        {
            poolProp.arraySize = itemPool.Count;
            for (int i = 0; i < itemPool.Count; i++)
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = itemPool[i];
            gmSO.ApplyModifiedPropertiesWithoutUndo();
        }

        // EnemySpawner
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        SerializedObject spawnerSO = new SerializedObject(spawner);
        SerializedProperty enemiesProp = spawnerSO.FindProperty("enemyPrefabs");
        if (enemiesProp != null)
        {
            enemiesProp.arraySize = 1;
            enemiesProp.GetArrayElementAtIndex(0).objectReferenceValue = enemyPrefab;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject shopGO = new GameObject("ShopManager");
        shopGO.AddComponent<ShopManager>();

        // ----- World Background -----
        GameObject bgGO = new GameObject("WorldBackground");
        WorldBackground wb = bgGO.AddComponent<WorldBackground>();
        Sprite tileSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/newtileset.png");
        if (tileSprite != null)
        {
            SerializedObject wbSO = new SerializedObject(wb);
            SerializedProperty tileProp = wbSO.FindProperty("tileSprite");
            if (tileProp != null) { tileProp.objectReferenceValue = tileSprite; wbSO.ApplyModifiedPropertiesWithoutUndo(); }
        }

        // ----- Player -----
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance.transform.position = Vector3.zero;

        // ----- Camera -----
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0x0a / 255f, 0x0a / 255f, 0x1a / 255f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<CameraFollow>();
        camGO.transform.position = new Vector3(0f, 0f, -10f);

        // ----- Canvas -----
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // HUD
        GameObject hud = CreateUIChild(canvasGO, "HUD");
        StretchToFill(hud.GetComponent<RectTransform>());

        Slider xpSlider = CreateSlider(hud, "XpSlider");
        xpSlider.minValue = 0f; xpSlider.maxValue = 100f; xpSlider.interactable = false;
        RectTransform xpRT = xpSlider.GetComponent<RectTransform>();
        xpRT.anchorMin = new Vector2(0.5f, 1f); xpRT.anchorMax = new Vector2(0.5f, 1f);
        xpRT.pivot = new Vector2(0.5f, 1f);
        xpRT.sizeDelta = new Vector2(600f, 20f);
        xpRT.anchoredPosition = new Vector2(0f, -30f);

        Text levelText = CreateLegacyText(hud, "LevelText", "LVL 1", 20, Color.white, TextAnchor.UpperLeft);
        RectTransform lvlRT = levelText.GetComponent<RectTransform>();
        lvlRT.anchorMin = new Vector2(0f, 1f); lvlRT.anchorMax = new Vector2(0f, 1f);
        lvlRT.pivot = new Vector2(0f, 1f);
        lvlRT.sizeDelta = new Vector2(200f, 40f);
        lvlRT.anchoredPosition = new Vector2(20f, -20f);

        Text timerText = CreateLegacyText(hud, "TimerText", "03:00", 24, Color.white, TextAnchor.UpperCenter);
        RectTransform timerRT = timerText.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.5f, 1f); timerRT.anchorMax = new Vector2(0.5f, 1f);
        timerRT.pivot = new Vector2(0.5f, 1f);
        timerRT.sizeDelta = new Vector2(200f, 40f);
        timerRT.anchoredPosition = new Vector2(0f, -70f);

        // ----- Level Up Panel (3 cards side by side) -----
        GameObject levelUpPanel = CreatePanel(canvasGO, "LevelUpPanel", new Color(0f, 0f, 0f, 0.75f));
        levelUpPanel.SetActive(false);

        CreateLegacyText(levelUpPanel, "TitleText", "¡SUBE DE NIVEL!", 28, Color.yellow, TextAnchor.MiddleCenter)
            .GetComponent<RectTransform>()
            .anchoredPosition = new Vector2(0f, 120f);

        var cards = new Button[3];
        var cardNames = new Text[3];
        var cardDescs = new Text[3];
        Vector2[] cardPositions = { new Vector2(-320f, -30f), new Vector2(0f, -30f), new Vector2(320f, -30f) };
        string[] defaultNames = { "Opción 1", "Opción 2", "Opción 3" };

        for (int i = 0; i < 3; i++)
        {
            (cards[i], cardNames[i], cardDescs[i]) = CreateLevelUpCard(
                levelUpPanel, "Card" + (i + 1), defaultNames[i], "...", cardPositions[i]);
        }

        // ----- Game Over Panel -----
        GameObject gameOverPanel = CreatePanel(canvasGO, "GameOverPanel", new Color(0f, 0f, 0f, 0.8f));
        gameOverPanel.SetActive(false);
        CreateResultText(gameOverPanel, "ResultText", "GAME OVER", 36, Color.red, new Vector2(0f, 80f));
        CreateRestartButton(gameOverPanel, "RestartButton", "Reintentar", new Vector2(0f, -60f), gameManager);

        // ----- Victory Panel -----
        GameObject victoryPanel = CreatePanel(canvasGO, "VictoryPanel", new Color(0f, 0f, 0f, 0.8f));
        victoryPanel.SetActive(false);
        CreateResultText(victoryPanel, "ResultText", "¡VICTORIA!", 36, Color.yellow, new Vector2(0f, 80f));
        CreateRestartButton(victoryPanel, "RestartButton", "Reintentar", new Vector2(0f, -60f), gameManager);

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

        // ----- Minimap -----
        GameObject minimapGO = new GameObject("Minimap", typeof(RectTransform));
        minimapGO.transform.SetParent(canvasGO.transform, false);
        Image minimapBg = minimapGO.AddComponent<Image>();
        minimapBg.color = new Color(0f, 0f, 0f, 0.5f);
        minimapGO.AddComponent<MinimapController>();
        RectTransform minimapRT = minimapGO.GetComponent<RectTransform>();
        minimapRT.anchorMin        = new Vector2(1f, 0f);
        minimapRT.anchorMax        = new Vector2(1f, 0f);
        minimapRT.pivot            = new Vector2(1f, 0f);
        minimapRT.sizeDelta        = new Vector2(150f, 150f);
        minimapRT.anchoredPosition = new Vector2(-20f, 20f);

        // ----- EventSystem -----
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ----- Wire UIManager -----
        SerializedObject uiSO = new SerializedObject(uiManager);
        SetRef(uiSO, "xpSlider",     xpSlider);
        SetRef(uiSO, "levelText",    levelText);
        SetRef(uiSO, "timerText",    timerText);
        SetRef(uiSO, "levelUpPanel", levelUpPanel);
        SetRef(uiSO, "gameOverPanel", gameOverPanel);
        SetRef(uiSO, "victoryPanel", victoryPanel);
        SetRef(uiSO, "pausePanel",   pausePanel);
        SetRefArray(uiSO, "levelUpCards",   cards);
        SetRefArray(uiSO, "cardNameTexts",  cardNames);
        SetRefArray(uiSO, "cardDescTexts",  cardDescs);
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        // ----- Save scene -----
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            Debug.LogWarning("[AstralSwarmSetup] SaveScene returned false.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // =====================================================================
    // UI HELPERS
    // =====================================================================

    private static GameObject CreateUIChild(GameObject parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Text CreateLegacyText(GameObject parent, string name, string content,
        int fontSize, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = alignment;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
              ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private static Slider CreateSlider(GameObject parent, string name)
    {
        GameObject sliderGO = new GameObject(name, typeof(RectTransform));
        sliderGO.transform.SetParent(parent.transform, false);
        Slider slider = sliderGO.AddComponent<Slider>();

        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(sliderGO.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        StretchToFill(bg.GetComponent<RectTransform>());

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        StretchToFill(fillArea.GetComponent<RectTransform>());

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 1f, 1f);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        StretchToFill(fillRT);

        slider.fillRect = fillRT;
        slider.targetGraphic = bgImg;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        StretchToFill(panel.GetComponent<RectTransform>());
        return panel;
    }

    /// <summary>
    /// Creates a level-up card button with a Name text and a Description text.
    /// No persistent onClick — UIManager wires it dynamically at runtime.
    /// </summary>
    private static (Button btn, Text nameText, Text descText) CreateLevelUpCard(
        GameObject parent, string name, string itemName, string desc, Vector2 anchoredPos)
    {
        GameObject cardGO = new GameObject(name, typeof(RectTransform));
        cardGO.transform.SetParent(parent.transform, false);
        Image img = cardGO.AddComponent<Image>();
        img.color = new Color(0.12f, 0.18f, 0.32f, 1f);
        Button btn = cardGO.AddComponent<Button>();
        btn.targetGraphic = img;

        // Highlight color on hover
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.2f, 0.35f, 0.6f, 1f);
        cb.pressedColor = new Color(0.08f, 0.12f, 0.22f, 1f);
        btn.colors = cb;

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(280f, 180f);
        rt.anchoredPosition = anchoredPos;

        // Name (top ~40%)
        Text nameT = CreateLegacyText(cardGO, "CardName", itemName, 18, Color.yellow, TextAnchor.UpperCenter);
        RectTransform nameRT = nameT.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.6f);
        nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.offsetMin = new Vector2(8f, 0f);
        nameRT.offsetMax = new Vector2(-8f, -6f);
        nameT.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameT.verticalOverflow = VerticalWrapMode.Truncate;

        // Description (bottom ~60%)
        Text descT = CreateLegacyText(cardGO, "CardDesc", desc, 13, Color.white, TextAnchor.UpperLeft);
        RectTransform descRT = descT.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0f);
        descRT.anchorMax = new Vector2(1f, 0.6f);
        descRT.offsetMin = new Vector2(8f, 6f);
        descRT.offsetMax = new Vector2(-8f, 0f);
        descT.horizontalOverflow = HorizontalWrapMode.Wrap;
        descT.verticalOverflow = VerticalWrapMode.Truncate;

        return (btn, nameT, descT);
    }

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

    private static void CreateRestartButton(GameObject parent, string name, string label,
        Vector2 anchoredPos, GameManager gm)
    {
        Button btn = CreateButton(parent, name, label, anchoredPos);
        UnityEventTools.AddPersistentListener(btn.onClick,
            new UnityEngine.Events.UnityAction(gm.RestartGame));
    }

    private static void CreateResultText(GameObject parent, string name, string content,
        int fontSize, Color color, Vector2 anchoredPos)
    {
        Text t = CreateLegacyText(parent, name, content, fontSize, color, TextAnchor.MiddleCenter);
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 80f);
        rt.anchoredPosition = anchoredPos;
    }

    private static void SetRef(SerializedObject so, string prop, Object value)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p == null) { Debug.LogWarning("[AstralSwarmSetup] Property '" + prop + "' not found on " + so.targetObject.GetType().Name); return; }
        p.objectReferenceValue = value;
    }

    private static void SetRefArray<T>(SerializedObject so, string prop, T[] values) where T : Object
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p == null) { Debug.LogWarning("[AstralSwarmSetup] Property '" + prop + "' not found."); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    // =====================================================================
    // SPRITE & ANIMATOR CONFIGURATION
    // =====================================================================

    [MenuItem("Astral Swarm/Configure Sprites and Animator")]
    public static void ConfigureSpritesAndAnimator()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Astral Swarm", "Configurando sprites...", 0.2f);
            ConfigureAllSpriteSheets();

            EditorUtility.DisplayProgressBar("Astral Swarm", "Creando Animator...", 0.5f);
            CreatePlayerAnimator();

            EditorUtility.DisplayProgressBar("Astral Swarm", "Asignando sprites a prefabs...", 0.8f);
            AssignPrefabSprites();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Astral Swarm", "¡Sprites y Animator configurados! Ya puedes hacer Play.", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"AstralSwarmSetup error: {e}");
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
        }
    }

    private static void ConfigureAllSpriteSheets()
    {
        if (!AssetDatabase.IsValidFolder(AnimationsFolder))
            AssetDatabase.CreateFolder("Assets", "Animations");

        ConfigureSpriteSheet("Assets/Sprites/Player/Hero.png",          32, 32, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/slime.png",        64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/bat.png",          64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/ghost.png",        64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/eyeball.png",      64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/big_worm.png",     64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/small_worm.png",   64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/Enemies/bee.png",          64, 64, 32);
        ConfigureSpriteSheet("Assets/Sprites/slime-projectile.png",     16, 16, 16);
        ConfigureSingleSprite("Assets/Sprites/Player/newtileset.png", 16);

        AssetDatabase.Refresh();
    }

    private static void ConfigureSpriteSheet(string assetPath, int cellW, int cellH, int ppu)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"Sprite not found: {assetPath}"); return; }

        importer.textureType        = TextureImporterType.Sprite;
        importer.spriteImportMode   = SpriteImportMode.Multiple;
        importer.filterMode         = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = ppu;
        importer.isReadable         = true;
        importer.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null) return;

        int cols = tex.width / cellW;
        int rows = tex.height / cellH;
        var meta = new List<SpriteMetaData>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float x = col * cellW;
                float y = tex.height - (row + 1) * cellH;
                meta.Add(new SpriteMetaData
                {
                    name      = $"frame_r{row}_c{col}",
                    rect      = new Rect(x, y, cellW, cellH),
                    pivot     = new Vector2(0.5f, 0.5f),
                    alignment = (int)SpriteAlignment.Center
                });
            }
        }

        importer.spritesheet = meta.ToArray();
        importer.SaveAndReimport();
    }

    private static void ConfigureSingleSprite(string assetPath, int ppu)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[AstralSwarmSetup] Sprite not found: {assetPath}"); return; }
        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = ppu;
        importer.SaveAndReimport();
    }

    private static void CreatePlayerAnimator()
    {
        if (!AssetDatabase.IsValidFolder(AnimationsFolder))
            AssetDatabase.CreateFolder("Assets", "Animations");

        string controllerPath = "Assets/Animations/PlayerAnimator.controller";

        var allSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Sprites/Player/Hero.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        if (allSprites.Length == 0) { Debug.LogWarning("No Hero sprites found."); return; }

        var idleClip = CreateSpriteAnimClip("PlayerIdle", new[] { allSprites[0] }, 4f, true);
        AssetDatabase.CreateAsset(idleClip, "Assets/Animations/PlayerIdle.anim");

        var runClip = CreateSpriteAnimClip("PlayerRun", allSprites, 8f, true);
        AssetDatabase.CreateAsset(runClip, "Assets/Animations/PlayerRun.anim");

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);

        var rootSM = controller.layers[0].stateMachine;
        var idleState = rootSM.AddState("Idle");
        idleState.motion = idleClip;
        rootSM.defaultState = idleState;

        var runState = rootSM.AddState("Run");
        runState.motion = runClip;

        var t1 = idleState.AddTransition(runState);
        t1.hasExitTime = false; t1.duration = 0f;
        t1.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");

        var t2 = runState.AddTransition(idleState);
        t2.hasExitTime = false; t2.duration = 0f;
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");

        AssetDatabase.SaveAssets();

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/Player.prefab"))
            {
                var anim = scope.prefabContentsRoot.GetComponent<Animator>();
                if (anim != null) anim.runtimeAnimatorController = controller;
                var sr = scope.prefabContentsRoot.GetComponent<SpriteRenderer>();
                if (sr != null && allSprites.Length > 0) sr.sprite = allSprites[0];
            }
        }
    }

    private static AnimationClip CreateSpriteAnimClip(string clipName, Sprite[] sprites, float fps, bool loop)
    {
        var clip = new AnimationClip { name = clipName, frameRate = fps };

        if (loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        return clip;
    }

    private static void AssignPrefabSprites()
    {
        var slimeSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Sprites/Enemies/slime.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (slimeSprites.Length > 0)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/Enemy.prefab"))
            {
                var sr = scope.prefabContentsRoot.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = slimeSprites[0];
            }
        }

        var projSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/Sprites/slime-projectile.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (projSprites.Length > 0)
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/Projectile.prefab"))
            {
                var sr = scope.prefabContentsRoot.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = projSprites[0];
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
