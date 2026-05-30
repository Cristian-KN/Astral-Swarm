using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
/// <summary>
/// Reconstruye la escena Game: césped TinySwords (GrassRuleTile) INFINITO que
/// se extiende alrededor del jugador (InfiniteGrass), árboles y rocas TinySwords,
/// Player warrior con espada melee, managers, cámara y drops de gemas/oro.
/// PRESERVA el HUD de UI Toolkit.
/// </summary>
public static class WorldRebuildSetup
{
    private const string ScenePath        = "Assets/Scenes/Game.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string EnemyPrefabPath  = "Assets/Prefabs/Enemy.prefab";

    private const string GrassRuleTilePath = "Assets/Sprites/Downloaded/TinySwords/Terrain/Tileset/GrassRuleTile.asset";
    private const string TreePath    = "Assets/Sprites/Downloaded/TinySwords/Terrain/Resources/Wood/Trees/Tree3.png";
    private const string RockPathFmt = "Assets/Sprites/Downloaded/TinySwords/Terrain/Decorations/Rocks/Rock{0}.png";
    private const string BushPathFmt = "Assets/Sprites/Downloaded/TinySwords/Terrain/Decorations/Bushes/Bushe{0}.png";

    private const string WarriorControllerPath = "Assets/Animations/Units/WarriorController.controller";
    private const string ArcherControllerPath  = "Assets/Animations/Units/ArcherController.controller";
    private const string LancerControllerPath  = "Assets/Animations/Units/LancerController.controller";
    private const string WarriorIdlePath = "Assets/Sprites/Downloaded/TinySwords/Units/Blue Units/Warrior/Warrior_Idle.png";
    private const string SwordItemPath   = "Assets/Items/Espada.asset";
    private const string IconAtlasPath   = "Assets/Sprites/Downloaded/Icons_Shikashi/#1 - Transparent Icons.png";

    [MenuItem("Astral Swarm/Reconstruir Mundo (terreno + warrior)")]
    public static void Rebuild()
    {
        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : SceneManager.GetActiveScene();

        // Borrar gameplay anterior, PRESERVANDO el HUD de UI Toolkit
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name != "HUD-UITK")
                Object.DestroyImmediate(root);

        // Configurar prefabs (warrior + items) antes de instanciar
        ConfigurePlayerAsWarrior();
        ConfigureItems();

        // ----- Terreno: césped TinySwords infinito + árboles + rocas -----
        CreateTerrain();

        // ----- Player (warrior) -----
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = Vector3.zero;
        }
        else Debug.LogWarning("[WorldRebuild] No se encontró Player.prefab.");

        // ----- Managers -----
        GameObject gmGO = new GameObject("GameManager");
        GameManager gameManager = gmGO.AddComponent<GameManager>();

        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Items" });
        if (itemGuids.Length > 0)
        {
            SerializedObject gmSO = new SerializedObject(gameManager);
            SerializedProperty pool = gmSO.FindProperty("itemPool");
            if (pool != null)
            {
                pool.arraySize = itemGuids.Length;
                for (int i = 0; i < itemGuids.Length; i++)
                    pool.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(itemGuids[i]));
                gmSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Crear tipos de enemigos variados y asignarlos al spawner
        List<GameObject> enemyPrefabs = CreateEnemyVariants();
        GameObject spawnerGO = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerGO.AddComponent<EnemySpawner>();
        {
            SerializedObject sSO = new SerializedObject(spawner);
            SerializedProperty enemiesProp = sSO.FindProperty("enemyPrefabs");
            if (enemiesProp != null)
            {
                enemiesProp.arraySize = enemyPrefabs.Count;
                for (int i = 0; i < enemyPrefabs.Count; i++)
                    enemiesProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabs[i];
            }
            var interval = sSO.FindProperty("spawnInterval");
            if (interval != null) interval.floatValue = 0.8f;
            sSO.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject shopGO = new GameObject("ShopManager");
        shopGO.AddComponent<ShopManager>();

        // ----- Cámara -----
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.12f, 0.30f, 0.14f); // verde de relleno por si acaso
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<CameraFollow>();
        camGO.transform.position = new Vector3(0f, 0f, -10f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Astral Swarm – Reconstruir Mundo",
            "Mundo reconstruido:\n\n" +
            "• Césped TinySwords INFINITO (se extiende con el jugador)\n" +
            "• Árboles y rocas TinySwords\n" +
            "• Warrior con espada (ataque melee)\n" +
            "• Enemigos sueltan gemas y oro\n" +
            "• HUD de UI Toolkit preservado\n\n" +
            "Dale a Play.", "OK");
    }

    // ===================== TERRENO =====================

    private static void CreateTerrain()
    {
        // Grid + Tilemap de césped
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        GameObject tilemapObj = new GameObject("Tilemap_Ground");
        tilemapObj.transform.SetParent(gridObj.transform);
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer tmRenderer = tilemapObj.AddComponent<TilemapRenderer>();
        tmRenderer.sortingOrder = -10;

        // Césped TinySwords (GrassRuleTile) + crecimiento infinito alrededor del jugador
        TileBase grass = AssetDatabase.LoadAssetAtPath<TileBase>(GrassRuleTilePath);
        if (grass == null)
            Debug.LogWarning("[WorldRebuild] No se encontró GrassRuleTile. El césped no se pintará.");

        InfiniteGrass inf = tilemapObj.AddComponent<InfiniteGrass>();
        inf.grassTile = grass;
        inf.radius = 24;

        // Decoración infinita: árboles + rocas (con colisión), arbustos (sin colisión)
        var obstacleSprites = new List<Sprite>();
        Sprite tree = LoadFirstSprite(TreePath);
        if (tree != null) { obstacleSprites.Add(tree); obstacleSprites.Add(tree); } // más probable que salgan árboles
        for (int i = 1; i <= 4; i++)
        {
            var rock = LoadFirstSprite(string.Format(RockPathFmt, i));
            if (rock != null) obstacleSprites.Add(rock);
        }
        var decorSprites = new List<Sprite>();
        for (int i = 1; i <= 4; i++)
        {
            var bush = LoadFirstSprite(string.Format(BushPathFmt, i));
            if (bush != null) decorSprites.Add(bush);
        }
        inf.obstacleSprites = obstacleSprites.ToArray();
        inf.decorSprites = decorSprites.ToArray();
        inf.obstacleChance = 0.05f;
        inf.decorChance = 0.06f;

        // Parche inicial de césped para el primer frame
        if (grass != null)
        {
            for (int x = -26; x <= 26; x++)
                for (int y = -26; y <= 26; y++)
                    tilemap.SetTile(new Vector3Int(x, y, 0), grass);
            tilemap.RefreshAllTiles();
        }
    }

    /// <summary>Carga el primer sprite de un PNG, ya sea Single o Multiple (sliced).</summary>
    private static Sprite LoadFirstSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    // ===================== PLAYER WARRIOR =====================

    private static void ConfigurePlayerAsWarrior()
    {
        if (!System.IO.File.Exists(PlayerPrefabPath))
        {
            Debug.LogWarning("[WorldRebuild] No existe Player.prefab para configurar.");
            return;
        }

        var warriorCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(WarriorControllerPath);
        var archerCtrl  = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ArcherControllerPath);
        var lancerCtrl  = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(LancerControllerPath);
        var sword       = AssetDatabase.LoadAssetAtPath<ItemData>(SwordItemPath);

        Sprite warriorIdle = AssetDatabase.LoadAllAssetRepresentationsAtPath(WarriorIdlePath)
            .OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();

        using (var scope = new PrefabUtility.EditPrefabContentsScope(PlayerPrefabPath))
        {
            var root = scope.prefabContentsRoot;

            var anim = root.GetComponent<Animator>();
            if (anim != null && warriorCtrl != null) anim.runtimeAnimatorController = warriorCtrl;

            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null && warriorIdle != null) sr.sprite = warriorIdle;

            var inv = root.GetComponent<InventoryManager>();
            if (inv != null) inv.playerClass = PlayerClass.Warrior;

            var pcv = root.GetComponent<PlayerClassVisuals>();
            if (pcv == null) pcv = root.AddComponent<PlayerClassVisuals>();
            pcv.warriorController = warriorCtrl;
            pcv.archerController  = archerCtrl;
            pcv.lancerController  = lancerCtrl;

            var pa = root.GetComponent<PlayerAttack>();
            if (pa != null && sword != null)
            {
                var paSO = new SerializedObject(pa);
                var dw = paSO.FindProperty("defaultWeapon");
                if (dw != null) { dw.objectReferenceValue = sword; paSO.ApplyModifiedPropertiesWithoutUndo(); }
            }
        }

        if (warriorCtrl == null) Debug.LogWarning("[WorldRebuild] WarriorController no encontrado.");
        if (warriorIdle == null) Debug.LogWarning("[WorldRebuild] Sprite Warrior_Idle no encontrado (¿Multiple?).");
        if (sword == null) Debug.LogWarning("[WorldRebuild] Espada.asset no encontrada.");
    }

    // ===================== ENEMIGOS VARIADOS =====================

    private struct EnemyDef
    {
        public string name, spritePath;
        public EnemyAI.Behavior behavior;
        public float speed, scale;
        public int contactDamage, projectileDamage, health;
        public EnemyDef(string n, string sp, EnemyAI.Behavior b, float spd, float sc, int cd, int pd, int hp)
        { name = n; spritePath = sp; behavior = b; speed = spd; scale = sc; contactDamage = cd; projectileDamage = pd; health = hp; }
    }

    private const string EnemyDir = "Assets/Prefabs/Enemies";
    private const string EnemyProjectilePath = "Assets/Prefabs/Enemies/EnemyProjectile.prefab";
    private const string EnemySpriteDir = "Assets/Sprites/Enemies/";
    private const string SlimeProjectileSprite = "Assets/Sprites/slime-projectile.png";

    private static List<GameObject> CreateEnemyVariants()
    {
        if (!AssetDatabase.IsValidFolder(EnemyDir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");

        // Drops compartidos
        GameObject[] gems = new GameObject[5];
        GameObject[] money = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            gems[i]  = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Pickups/Gem_Tier{i}_Prefab.prefab");
            money[i] = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Pickups/Money_Tier{i}_Prefab.prefab");
        }

        GameObject enemyProjectile = CreateEnemyProjectilePrefab();

        var defs = new List<EnemyDef>
        {
            //          nombre          sprite                       comportamiento          vel  escala cDmg pDmg  vida
            new EnemyDef("Enemy_Slime",   EnemySpriteDir+"slime.png",   EnemyAI.Behavior.Chaser, 2.5f, 1.0f, 15, 0,   50),
            new EnemyDef("Enemy_Bat",     EnemySpriteDir+"bat.png",     EnemyAI.Behavior.Chaser, 4.5f, 0.8f,  8, 0,   25),
            new EnemyDef("Enemy_Bee",     EnemySpriteDir+"bee.png",     EnemyAI.Behavior.Chaser, 5.5f, 0.7f,  6, 0,   18),
            new EnemyDef("Enemy_Ghost",   EnemySpriteDir+"ghost.png",   EnemyAI.Behavior.Chaser, 3.5f, 1.0f, 18, 0,   40),
            new EnemyDef("Enemy_BigWorm", EnemySpriteDir+"big_worm.png",EnemyAI.Behavior.Chaser, 1.3f, 1.4f, 30, 0,  200),
            new EnemyDef("Enemy_Eyeball", EnemySpriteDir+"eyeball.png", EnemyAI.Behavior.Shooter,2.0f, 1.0f,  5, 12,  60),
        };

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1) enemyLayer = 6;

        var result = new List<GameObject>();
        foreach (var d in defs)
        {
            GameObject prefab = CreateOneEnemyPrefab(d, enemyLayer, enemyProjectile, gems, money);
            if (prefab != null) result.Add(prefab);
        }
        return result;
    }

    private static GameObject CreateOneEnemyPrefab(EnemyDef d, int enemyLayer,
        GameObject enemyProjectile, GameObject[] gems, GameObject[] money)
    {
        Sprite sprite = LoadFirstSprite(d.spritePath);

        GameObject go = new GameObject(d.name);
        go.layer = enemyLayer;
        try { go.tag = "Enemy"; } catch (UnityException) { }
        go.transform.localScale = Vector3.one * d.scale;

        var sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f * Mathf.Max(0.5f, d.scale);

        var ai = go.AddComponent<EnemyAI>();
        var aiSO = new SerializedObject(ai);
        SetEnum(aiSO, "behavior", (int)d.behavior);
        SetFloat(aiSO, "moveSpeed", d.speed);
        SetInt(aiSO, "collisionDamage", d.contactDamage);
        if (d.behavior == EnemyAI.Behavior.Shooter)
        {
            SetObj(aiSO, "projectilePrefab", enemyProjectile);
            SetInt(aiSO, "projectileDamage", d.projectileDamage);
            SetFloat(aiSO, "attackRange", 7f);
            SetFloat(aiSO, "fireCooldown", 1.8f);
        }
        aiSO.ApplyModifiedPropertiesWithoutUndo();

        var stats = go.AddComponent<EnemyStats>();
        var stSO = new SerializedObject(stats);
        SetFloat(stSO, "baseMaxHealth", d.health);
        AssignArray(stSO, "gemPrefabs", gems);
        AssignArray(stSO, "moneyPrefabs", money);
        stSO.ApplyModifiedPropertiesWithoutUndo();

        go.AddComponent<EnemyColorizer>();

        string path = $"{EnemyDir}/{d.name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateEnemyProjectilePrefab()
    {
        GameObject go = new GameObject("EnemyProjectile");
        go.transform.localScale = Vector3.one * 0.5f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        var sr = go.AddComponent<SpriteRenderer>();
        Sprite projSprite = LoadFirstSprite(SlimeProjectileSprite);
        if (projSprite != null) sr.sprite = projSprite;
        sr.color = new Color(0.8f, 0.3f, 1f); // tinte mágico
        sr.sortingOrder = 2;

        go.AddComponent<EnemyProjectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyProjectilePath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void SetFloat(SerializedObject so, string p, float v)
    { var sp = so.FindProperty(p); if (sp != null) sp.floatValue = v; }
    private static void SetInt(SerializedObject so, string p, int v)
    { var sp = so.FindProperty(p); if (sp != null) sp.intValue = v; }
    private static void SetEnum(SerializedObject so, string p, int v)
    { var sp = so.FindProperty(p); if (sp != null) sp.enumValueIndex = v; }
    private static void SetObj(SerializedObject so, string p, Object v)
    { var sp = so.FindProperty(p); if (sp != null) sp.objectReferenceValue = v; }

    // ===================== ITEMS (nombre + descripción + icono) =====================

    private struct ItemDef
    {
        public string name, desc;
        public int icon; // índice del sprite "_N" en el atlas Shikashi
        public ItemDef(string n, string d, int ic) { name = n; desc = d; icon = ic; }
    }

    // Mapeo por nombre de archivo del .asset → datos coherentes.
    // Índices de icono: estimación temática del atlas Shikashi (filas de armas/escudos/etc.).
    private static readonly Dictionary<string, ItemDef> ItemDefs = new Dictionary<string, ItemDef>
    {
        { "Espada",           new ItemDef("Espada de Hierro",     "Tajo cuerpo a cuerpo al enemigo más cercano.",        74) },
        { "EspadaEpica",      new ItemDef("Espada Rúnica",        "Espada encantada: más daño y alcance.",               76) },
        { "Lanza",            new ItemDef("Lanza Arrojadiza",     "Jabalina que atraviesa a los enemigos.",              95) },
        { "Arco",             new ItemDef("Arco Élfico",          "Dispara flechas al enemigo más cercano.",             92) },
        { "Baston",           new ItemDef("Bastón Arcano",        "Proyectiles mágicos teledirigidos.",                  99) },
        { "NovadeFuego",      new ItemDef("Nova de Fuego",        "Estallido de fuego a tu alrededor.",                  44) },
        { "MagicOrb",         new ItemDef("Orbe Mágico",          "Orbe que persigue y golpea enemigos.",               280) },
        { "ShadowDart",       new ItemDef("Dardo Sombrío",        "Disparo veloz de baja potencia.",                     73) },
        { "BotasdeVelocidad", new ItemDef("Botas de Velocidad",   "+20% de velocidad de movimiento.",                   118) },
        { "SpeedBoots",       new ItemDef("Botas Veloces",        "+15% de velocidad de movimiento.",                   119) },
        { "IronShield",       new ItemDef("Escudo de Hierro",     "+5 de defensa.",                                      89) },
        { "PowerCrystal",     new ItemDef("Cristal de Poder",     "+10 de poder de ataque.",                            287) },
        { "GrowthSeed",       new ItemDef("Semilla de Crecimiento","Te fortaleces con cada enemigo eliminado.",          234) },
    };

    /// <summary>
    /// Asigna nombre, descripción e icono coherentes a cada ItemData.
    /// </summary>
    private static void ConfigureItems()
    {
        // Mapa índice → sprite ("_N") del atlas
        var byIndex = new Dictionary<int, Sprite>();
        foreach (var s in AssetDatabase.LoadAllAssetRepresentationsAtPath(IconAtlasPath).OfType<Sprite>())
        {
            int us = s.name.LastIndexOf('_');
            if (us >= 0 && int.TryParse(s.name.Substring(us + 1), out int n))
                byIndex[n] = s;
        }
        if (byIndex.Count == 0)
            Debug.LogWarning("[WorldRebuild] Atlas de iconos sin sprites (¿Multiple?). Items sin icono.");

        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Items" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null || !ItemDefs.TryGetValue(file, out ItemDef def)) continue;

            item.itemName = def.name;
            item.description = def.desc;
            if (byIndex.TryGetValue(def.icon, out Sprite sp)) item.icon = sp;
            EditorUtility.SetDirty(item);
        }
        AssetDatabase.SaveAssets();
    }

    private static void AssignArray(SerializedObject so, string prop, GameObject[] values)
    {
        var p = so.FindProperty(prop);
        if (p == null) { Debug.LogWarning($"[WorldRebuild] Propiedad '{prop}' no encontrada en EnemyStats."); return; }
        int count = values.Count(v => v != null);
        p.arraySize = count;
        int idx = 0;
        foreach (var v in values)
            if (v != null) p.GetArrayElementAtIndex(idx++).objectReferenceValue = v;
    }
}
#endif
