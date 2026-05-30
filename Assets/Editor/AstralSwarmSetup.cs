using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public static class AstralSwarmSetup
{
    // ---------- Paths ----------
    private const string AnimationsFolder = "Assets/Animations";
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
            ItemData defaultWeapon = pool[0];

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
            "¡Setup de gameplay completo!\n\n" +
            "1. Ejecuta 'Configure Sprites and Animator'.\n" +
            "2. Ejecuta 'Setup HUD (UI Toolkit)' para montar el HUD.\n" +
            "3. Dale a Play.", "OK");
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
        // No sobrescribir un Player.prefab ya configurado (p.ej. el warrior del usuario)
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (existing != null) return existing;

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
    // SCENE (solo gameplay — el HUD lo monta 'Setup HUD (UI Toolkit)')
    // =====================================================================

    private static void SetupScene(GameObject playerPrefab, GameObject enemyPrefab, List<ItemData> itemPool)
    {
        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Borrar todo el gameplay anterior, pero PRESERVAR el HUD de UI Toolkit si existe
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name != "HUD-UITK")
                Object.DestroyImmediate(root);

        // ----- Managers -----
        GameObject gmGO = new GameObject("GameManager");
        GameManager gameManager = gmGO.AddComponent<GameManager>();

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

        // ----- Save scene -----
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            Debug.LogWarning("[AstralSwarmSetup] SaveScene returned false.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
