using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if UNITY_EDITOR
/// <summary>
/// Monta el HUD de UI Toolkit en la escena Game: crea el PanelSettings,
/// elimina el Canvas uGUI antiguo, añade un UIDocument con HUDController,
/// asigna iconos y repuebla el itemPool del GameManager.
/// </summary>
public static class HUDSetupUITK
{
    private const string ScenePath        = "Assets/Scenes/Game.unity";
    private const string UxmlPath         = "Assets/UI/HUD/HUD.uxml";
    private const string UssPath          = "Assets/UI/HUD/HUD.uss";
    private const string PanelSettingsDir = "Assets/UI/HUD/Resources";
    private const string PanelSettingsPath = "Assets/UI/HUD/Resources/HUDPanelSettings.asset";

    private const string TB = "Assets/Sprites/Downloaded/01_TravelBookLite/Sprites/";

    [MenuItem("Astral Swarm/Setup HUD (UI Toolkit)")]
    public static void Setup()
    {
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        if (uxml == null)
        {
            EditorUtility.DisplayDialog("Astral Swarm – HUD UITK",
                "No se encontró HUD.uxml en " + UxmlPath, "OK");
            return;
        }

        PanelSettings ps = EnsurePanelSettings();

        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : SceneManager.GetActiveScene();

        // 1) Eliminar Canvas uGUI antiguo del HUD
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<Canvas>(true) != null && root.name == "Canvas")
                Object.DestroyImmediate(root);
        }

        // 2) Limpiar componentes con script perdido (p.ej. el viejo UIManager)
        foreach (GameObject root in scene.GetRootGameObjects())
            RemoveMissingScriptsRecursive(root);

        // 3) Crear / reutilizar el GameObject del HUD
        GameObject hudGO = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == "HUD-UITK") { hudGO = root; break; }

        if (hudGO == null)
        {
            hudGO = new GameObject("HUD-UITK");
            SceneManager.MoveGameObjectToScene(hudGO, scene);
        }

        var doc = hudGO.GetComponent<UIDocument>();
        if (doc == null) doc = hudGO.AddComponent<UIDocument>();
        // Asignar vía SerializedObject — la asignación directa de m_PanelSettings NO persiste en Unity 6
        var sdoc = new SerializedObject(doc);
        var panelProp = sdoc.FindProperty("m_PanelSettings");
        var sourceProp = sdoc.FindProperty("sourceAsset");
        if (panelProp != null) panelProp.objectReferenceValue = ps;
        if (sourceProp != null) sourceProp.objectReferenceValue = uxml;
        sdoc.ApplyModifiedPropertiesWithoutUndo();

        var controller = hudGO.GetComponent<HUDController>();
        if (controller == null) controller = hudGO.AddComponent<HUDController>();

        // 4) Asignar PanelSettings + iconos al HUDController
        SerializedObject so = new SerializedObject(controller);
        SetRef(so, "panelSettings", ps);
        SetRef(so, "hudStyleSheet", AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath));
        SetRef(so, "heartFull",  LoadSprite(TB + "UI_TravelBook_IconHeart01a.png"));
        SetRef(so, "heartHalf",  LoadSprite(TB + "UI_TravelBook_IconHeart01e.png"));
        SetRef(so, "heartEmpty", LoadSprite(TB + "UI_TravelBook_IconHeart01i.png"));
        SetRef(so, "coinIcon",   LoadSprite(TB + "UI_TravelBook_IconCoin01a.png"));
        SetRef(so, "pauseIcon",  LoadSprite(TB + "UI_TravelBook_IconPause01a.png"));
        SetRef(so, "playIcon",   LoadSprite(TB + "UI_TravelBook_IconPlay01a.png"));
        so.ApplyModifiedPropertiesWithoutUndo();

        // 5) Repoblar itemPool del GameManager con todos los ItemData de Assets/Items
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Items" });
            if (guids.Length > 0)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                SerializedProperty pool = gmSO.FindProperty("itemPool");
                if (pool != null)
                {
                    pool.arraySize = guids.Length;
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        pool.GetArrayElementAtIndex(i).objectReferenceValue =
                            AssetDatabase.LoadAssetAtPath<ItemData>(path);
                    }
                    gmSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Astral Swarm – HUD UITK",
            "HUD de UI Toolkit montado en la escena.\n\n" +
            "• UIDocument 'HUD-UITK' con HUDController.\n" +
            "• Canvas uGUI antiguo eliminado.\n" +
            "• itemPool del GameManager repoblado.\n\n" +
            "Dale a Play.", "OK");
    }

    // ---------------------------------------------------------------------

    private static PanelSettings EnsurePanelSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/UI/HUD"))
            AssetDatabase.CreateFolder("Assets/UI", "HUD");
        if (!AssetDatabase.IsValidFolder(PanelSettingsDir))
            AssetDatabase.CreateFolder("Assets/UI/HUD", "Resources");

        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.name = "HUDPanelSettings";
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1600, 900);
        ps.match = 0.5f;
        ps.sortingOrder = 10; // por encima de cualquier otro panel

        // Reutilizar el ThemeStyleSheet existente (el del menú) para evitar warning
        string[] themeGuids = AssetDatabase.FindAssets("t:ThemeStyleSheet");
        if (themeGuids.Length > 0)
        {
            string themePath = AssetDatabase.GUIDToAssetPath(themeGuids[0]);
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(themePath);
            if (theme != null) ps.themeStyleSheet = theme;
        }

        AssetDatabase.CreateAsset(ps, PanelSettingsPath);
        AssetDatabase.SaveAssets();
        return ps;
    }

    private static void RemoveMissingScriptsRecursive(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
            RemoveMissingScriptsRecursive(child.gameObject);
    }

    private static void SetRef(SerializedObject so, string prop, Object value)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p == null) { Debug.LogWarning("[HUDSetupUITK] Propiedad '" + prop + "' no encontrada."); return; }
        p.objectReferenceValue = value;
    }

    private static Sprite LoadSprite(string path)
    {
        EnsureSpriteImported(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureSpriteImported(string path)
    {
        if (!System.IO.File.Exists(path)) return;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType        = TextureImporterType.Sprite;
            importer.spriteImportMode   = SpriteImportMode.Single;
            importer.filterMode         = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
#endif
