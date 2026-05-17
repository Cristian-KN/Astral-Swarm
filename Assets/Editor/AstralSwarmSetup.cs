using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
/// <summary>
/// Editor utility that builds the entire base game scene and prefab set for Astral Swarm.
/// Run from the menu: "Astral Swarm/Setup Game Scene".
/// </summary>
public static class AstralSwarmSetup
{
    // ---------- Paths ----------
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string EditorFolder = "Assets/Editor";
    private const string SpritesFolder = "Assets/Sprites";
    private const string SpritesPlayerFolder = "Assets/Sprites/Player";
    private const string SpritesEnemiesFolder = "Assets/Sprites/Enemies";
    private const string AnimationsFolder = "Assets/Animations";
    private const string ScenesFolder = "Assets/Scenes";
    private const string ScenePath = "Assets/Scenes/Game.unity";

    // ---------- Prefab Paths ----------
    private const string ProjectilePrefabPath = "Assets/Prefabs/Projectile.prefab";
    private const string ExperienceGemPrefabPath = "Assets/Prefabs/ExperienceGem.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";

    // ---------- Layers / Tags ----------
    private const string PlayerTag = "Player";
    private const string EnemyTag = "Enemy";

    [MenuItem("Astral Swarm/Setup Game Scene")]
    public static void SetupGameScene()
    {
        try
        {
            // 1) Folders
            CreateFolders();

            // 2) Prefabs (saved to disk so they can be referenced by other prefabs/scene)
            GameObject projectilePrefab = CreateProjectilePrefab();
            GameObject experienceGemPrefab = CreateExperienceGemPrefab();
            GameObject enemyPrefab = CreateEnemyPrefab(experienceGemPrefab);
            GameObject playerPrefab = CreatePlayerPrefab(projectilePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3) Scene
            SetupScene(playerPrefab, enemyPrefab);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AstralSwarmSetup] Setup failed: " + ex);
            EditorUtility.DisplayDialog(
                "Astral Swarm Setup",
                "El setup falló: " + ex.Message + "\nRevisa la consola para más detalles.",
                "OK");
            return;
        }

        EditorUtility.DisplayDialog(
            "Astral Swarm Setup",
            "¡Setup completo! Falta asignar sprites al Animator del Player y los enemigos.",
            "OK");
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
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string fullPath = parent + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
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

        // Tag & Layer
        TrySetTag(go, EnemyTag);
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        if (enemyLayerIndex == -1)
        {
            Debug.LogWarning("AstralSwarmSetup: Layer 'Enemy' not found. Set it in Project Settings → Tags and Layers.");
            enemyLayerIndex = 6; // fallback
        }
        go.layer = enemyLayerIndex;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.4f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.white;

        go.AddComponent<EnemyAI>();
        EnemyStats stats = go.AddComponent<EnemyStats>();
        go.AddComponent<EnemyColorizer>();

        // Wire experienceGemPrefab (private SerializeField) via SerializedObject
        SerializedObject so = new SerializedObject(stats);
        SerializedProperty gemProp = so.FindProperty("experienceGemPrefab");
        if (gemProp != null)
        {
            gemProp.objectReferenceValue = experienceGemPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("[AstralSwarmSetup] Could not find SerializedProperty 'experienceGemPrefab' on EnemyStats.");
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreatePlayerPrefab(GameObject projectilePrefab)
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
        go.AddComponent<Animator>(); // Controller assigned by user later

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerStats>();
        PlayerAttack playerAttack = go.AddComponent<PlayerAttack>();
        go.AddComponent<InventoryManager>();

        // Wire PlayerAttack private SerializeField fields.
        SerializedObject so = new SerializedObject(playerAttack);

        SerializedProperty projProp = so.FindProperty("magicProjectilePrefab");
        if (projProp != null)
        {
            projProp.objectReferenceValue = projectilePrefab;
        }
        else
        {
            Debug.LogWarning("[AstralSwarmSetup] Could not find SerializedProperty 'magicProjectilePrefab' on PlayerAttack.");
        }

        SerializedProperty layerProp = so.FindProperty("enemyLayer");
        if (layerProp != null)
        {
            // LayerMask is serialized as an int bitmask.
            int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
            if (enemyLayerIndex == -1)
            {
                Debug.LogWarning("AstralSwarmSetup: Layer 'Enemy' not found. Set it in Project Settings → Tags and Layers.");
                enemyLayerIndex = 6; // fallback
            }
            layerProp.intValue = 1 << enemyLayerIndex;
        }
        else
        {
            Debug.LogWarning("[AstralSwarmSetup] Could not find SerializedProperty 'enemyLayer' on PlayerAttack.");
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PlayerPrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void TrySetTag(GameObject go, string tag)
    {
        try
        {
            go.tag = tag;
        }
        catch (UnityException)
        {
            Debug.LogWarning("[AstralSwarmSetup] Tag '" + tag + "' is not defined. Falling back to 'Untagged'. Configura el tag en Edit > Project Settings > Tags and Layers.");
        }
    }

    // =====================================================================
    // SCENE
    // =====================================================================

    private static void SetupScene(GameObject playerPrefab, GameObject enemyPrefab)
    {
        Scene scene;
        if (System.IO.File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // Clear scene
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Object.DestroyImmediate(root);
        }

        // ----- Managers -----
        GameObject gameManagerGO = new GameObject("GameManager");
        GameManager gameManager = gameManagerGO.AddComponent<GameManager>();
        UIManager uiManager = gameManagerGO.AddComponent<UIManager>();

        GameObject enemySpawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = enemySpawnerGO.AddComponent<EnemySpawner>();

        // Wire enemyPrefabs list on the EnemySpawner
        SerializedObject spawnerSO = new SerializedObject(spawner);
        SerializedProperty enemiesProp = spawnerSO.FindProperty("enemyPrefabs");
        if (enemiesProp != null)
        {
            enemiesProp.arraySize = 1;
            enemiesProp.GetArrayElementAtIndex(0).objectReferenceValue = enemyPrefab;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("[AstralSwarmSetup] Could not find 'enemyPrefabs' on EnemySpawner.");
        }

        GameObject shopGO = new GameObject("ShopManager");
        shopGO.AddComponent<ShopManager>();

        // ----- World: Player -----
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance.transform.position = Vector3.zero;

        // ----- Camera -----
        GameObject cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        Camera cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        // background color #0a0a1a
        cam.backgroundColor = new Color(0x0a / 255f, 0x0a / 255f, 0x1a / 255f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraGO.AddComponent<AudioListener>();
        cameraGO.AddComponent<CameraFollow>();
        cameraGO.transform.position = new Vector3(0f, 0f, -10f);

        // ----- UI -----
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // HUD parent
        GameObject hud = CreateUIChild(canvasGO, "HUD");
        RectTransform hudRT = hud.GetComponent<RectTransform>();
        hudRT.anchorMin = Vector2.zero;
        hudRT.anchorMax = Vector2.one;
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;

        // XP Slider (top-center, y = -30 from top)
        Slider xpSlider = CreateSlider(hud, "XpSlider");
        xpSlider.minValue = 0f;
        xpSlider.maxValue = 100f;
        xpSlider.value = 0f;
        xpSlider.interactable = false;
        RectTransform xpRT = xpSlider.GetComponent<RectTransform>();
        xpRT.anchorMin = new Vector2(0.5f, 1f);
        xpRT.anchorMax = new Vector2(0.5f, 1f);
        xpRT.pivot = new Vector2(0.5f, 1f);
        xpRT.sizeDelta = new Vector2(600f, 20f);
        xpRT.anchoredPosition = new Vector2(0f, -30f);

        // Level Text (top-left)
        Text levelText = CreateLegacyText(hud, "LevelText", "LVL 1", 20, Color.white, TextAnchor.UpperLeft);
        RectTransform levelRT = levelText.GetComponent<RectTransform>();
        levelRT.anchorMin = new Vector2(0f, 1f);
        levelRT.anchorMax = new Vector2(0f, 1f);
        levelRT.pivot = new Vector2(0f, 1f);
        levelRT.sizeDelta = new Vector2(200f, 40f);
        levelRT.anchoredPosition = new Vector2(20f, -20f);

        // Timer Text (top-center)
        Text timerText = CreateLegacyText(hud, "TimerText", "03:00", 24, Color.white, TextAnchor.UpperCenter);
        RectTransform timerRT = timerText.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.5f, 1f);
        timerRT.anchorMax = new Vector2(0.5f, 1f);
        timerRT.pivot = new Vector2(0.5f, 1f);
        timerRT.sizeDelta = new Vector2(200f, 40f);
        timerRT.anchoredPosition = new Vector2(0f, -70f);

        // ----- Level Up Panel -----
        GameObject levelUpPanel = CreatePanel(canvasGO, "LevelUpPanel", new Color(0f, 0f, 0f, 0.7f));
        levelUpPanel.SetActive(false);
        CreateUpgradeButton(levelUpPanel, "Option1Button", "Mejora 1", new Vector2(0f, 120f), gameManager);
        CreateUpgradeButton(levelUpPanel, "Option2Button", "Mejora 2", new Vector2(0f, 0f), gameManager);
        CreateUpgradeButton(levelUpPanel, "Option3Button", "Mejora 3", new Vector2(0f, -120f), gameManager);

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

        // ----- EventSystem -----
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // ----- Wire UIManager references -----
        SerializedObject uiSO = new SerializedObject(uiManager);
        SetObjectReference(uiSO, "xpSlider", xpSlider);
        SetObjectReference(uiSO, "levelText", levelText);
        SetObjectReference(uiSO, "timerText", timerText);
        SetObjectReference(uiSO, "levelUpPanel", levelUpPanel);
        SetObjectReference(uiSO, "gameOverPanel", gameOverPanel);
        SetObjectReference(uiSO, "victoryPanel", victoryPanel);
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        // ----- Save scene -----
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        if (!saved)
        {
            Debug.LogWarning("[AstralSwarmSetup] EditorSceneManager.SaveScene returned false. Verifica que la escena exista en " + ScenePath);
        }

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

    private static Text CreateLegacyText(GameObject parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = alignment;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null)
        {
            // Unity 6 may use Arial.ttf builtin fallback
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private static Slider CreateSlider(GameObject parent, string name)
    {
        GameObject sliderGO = new GameObject(name, typeof(RectTransform));
        sliderGO.transform.SetParent(parent.transform, false);

        Slider slider = sliderGO.AddComponent<Slider>();

        // Background
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(sliderGO.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        RectTransform bgRT = background.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        slider.fillRect = fillRT;
        slider.targetGraphic = bgImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform));
        btnGO.transform.SetParent(parent.transform, false);
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.35f, 1f);
        Button button = btnGO.AddComponent<Button>();
        button.targetGraphic = img;

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 80f);
        rt.anchoredPosition = anchoredPos;

        // Label
        Text label_t = CreateLegacyText(btnGO, "Text", label, 22, Color.white, TextAnchor.MiddleCenter);
        RectTransform labelRT = label_t.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        return button;
    }

    private static void CreateUpgradeButton(GameObject parent, string name, string label, Vector2 anchoredPos, GameManager gameManager)
    {
        Button button = CreateButton(parent, name, label, anchoredPos);
        UnityEventTools.AddPersistentListener(button.onClick,
            new UnityEngine.Events.UnityAction(gameManager.ResumeGame));
    }

    private static void CreateRestartButton(GameObject parent, string name, string label, Vector2 anchoredPos, GameManager gameManager)
    {
        Button button = CreateButton(parent, name, label, anchoredPos);
        UnityEventTools.AddPersistentListener(button.onClick,
            new UnityEngine.Events.UnityAction(gameManager.RestartGame));
    }

    private static void CreateResultText(GameObject parent, string name, string content, int fontSize, Color color, Vector2 anchoredPos)
    {
        Text t = CreateLegacyText(parent, name, content, fontSize, color, TextAnchor.MiddleCenter);
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600f, 80f);
        rt.anchoredPosition = anchoredPos;
    }

    private static void SetObjectReference(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("[AstralSwarmSetup] Could not find SerializedProperty '" + propertyName + "' on " + so.targetObject.GetType().Name);
            return;
        }
        prop.objectReferenceValue = value;
    }
}
#endif
