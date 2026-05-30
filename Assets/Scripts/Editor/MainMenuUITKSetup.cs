using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Monta el menú principal de UI Toolkit en la escena MainMenu:
///  - genera texturas de resplandor (glow) y viñeta,
///  - pone el fondo en filtro Point (pixel nítido),
///  - crea el PanelSettings (1408x768, Scale With Screen Size, match 0.5),
///  - crea un GameObject con UIDocument + MainMenuController,
///  - desactiva el menú uGUI antiguo.
///
/// Uso: Tools > Astral Swarm > Montar Menú (UI Toolkit)
/// </summary>
public static class MainMenuUITKSetup
{
    private const string UiDir = "Assets/UI/MainMenu";
    private const string GenDir = "Assets/UI/MainMenu/gen";
    private const string ThemeDir = "Assets/UI Toolkit";
    private const string UxmlPath = "Assets/UI/MainMenu/MainMenu.uxml";
    private const string BgPath = "Assets/UI/MainMenu/menu_background.jpeg";
    private const string GlowPath = "Assets/UI/MainMenu/gen/glow.png";
    private const string VignettePath = "Assets/UI/MainMenu/gen/vignette.png";
    private const string PanelPath = "Assets/UI/MainMenu/Resources/MainMenuPanelSettings.asset";
    private const string ThemePath = "Assets/UI Toolkit/UnityDefaultRuntimeTheme.tss";
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/Astral Swarm/Montar Menú (UI Toolkit)")]
    public static void Setup()
    {
        EnsureFolders();
        GenerateGlowTexture();
        GenerateVignetteTexture();
        AssetDatabase.Refresh();

        GenerateUITKFonts();

        SetPointFilter(BgPath);
        SetPointFilter(GlowPath, point: false);     // glow se ve mejor suavizado
        SetPointFilter(VignettePath, point: false);

        ThemeStyleSheet theme = GetOrCreateRuntimeTheme();
        PanelSettings panel = GetOrCreatePanelSettings(theme);

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        if (uxml == null) { Debug.LogError($"[Astral Swarm] No se encontró {UxmlPath}"); return; }

        var glowTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GlowPath);
        var vigTex = AssetDatabase.LoadAssetAtPath<Texture2D>(VignettePath);

        // ---- build the scene ----
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // disable old uGUI menu if present (kept in scene, just inactive)
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "MainMenuCanvas" || root.name == "MainMenuManager") root.SetActive(false);
        }

        GameObject go = GameObject.Find("MainMenuUI");
        if (go == null) go = new GameObject("MainMenuUI");

        var doc = go.GetComponent<UIDocument>();
        if (doc == null) doc = go.AddComponent<UIDocument>();
        // assign via SerializedObject — direct property assignment did not persist panelSettings
        var sdoc = new SerializedObject(doc);
        var panelProp = sdoc.FindProperty("m_PanelSettings");
        var sourceProp = sdoc.FindProperty("sourceAsset");
        if (panelProp != null) panelProp.objectReferenceValue = panel;
        if (sourceProp != null) sourceProp.objectReferenceValue = uxml;
        sdoc.ApplyModifiedPropertiesWithoutUndo();

        var ctrl = go.GetComponent<MainMenuController>();
        if (ctrl == null) ctrl = go.AddComponent<MainMenuController>();

        // assign refs via SerializedObject (private [SerializeField] fields persist reliably here,
        // unlike UIDocument.m_PanelSettings — the controller applies panelSettings at runtime)
        var so = new SerializedObject(ctrl);
        var panelCtrlProp = so.FindProperty("panelSettings");
        var glowProp = so.FindProperty("glowTexture");
        var vigProp = so.FindProperty("vignetteTexture");
        if (panelCtrlProp != null) panelCtrlProp.objectReferenceValue = panel;
        if (glowProp != null) glowProp.objectReferenceValue = glowTex;
        if (vigProp != null) vigProp.objectReferenceValue = vigTex;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Astral Swarm] Menú UI Toolkit montado en MainMenu. Pulsa Play.");
    }

    // ----------------------------------------------------------------

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(GenDir)) AssetDatabase.CreateFolder(UiDir, "gen");
        if (!AssetDatabase.IsValidFolder(UiDir + "/Resources")) AssetDatabase.CreateFolder(UiDir, "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Fonts/UITK")) AssetDatabase.CreateFolder("Assets/Fonts", "UITK");
        if (!AssetDatabase.IsValidFolder(ThemeDir)) AssetDatabase.CreateFolder("Assets", "UI Toolkit");
    }

    private static void GenerateGlowTexture()
    {
        const int N = 128;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var px = new Color[N * N];
        Vector2 c = new Vector2((N - 1) / 2f, (N - 1) / 2f);
        float maxd = c.x;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxd;
                float a = Mathf.Clamp01(1f - d);
                a = a * a;                              // soft radial falloff
                px[y * N + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(GlowPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateVignetteTexture()
    {
        const int N = 256;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var px = new Color[N * N];
        Vector2 c = new Vector2((N - 1) / 2f, (N - 1) / 2f);
        float maxd = Mathf.Sqrt(c.x * c.x + c.y * c.y);
        Color veil = new Color(8f / 255f, 6f / 255f, 14f / 255f, 1f);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxd;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.45f) / 0.55f)) * 0.85f;
                px[y * N + x] = new Color(veil.r, veil.g, veil.b, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(VignettePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // UI Toolkit (Unity 6) ignora -unity-font (Font legacy); necesita FontAsset (TextCore)
    // referenciado con -unity-font-definition. Aquí generamos esos FontAssets desde los .ttf.
    private static void GenerateUITKFonts()
    {
        CreateUITKFont("Assets/Fonts/UnifrakturCook-Bold.ttf", "Assets/Fonts/UITK/UnifrakturCook.asset");
        CreateUITKFont("Assets/Fonts/PirataOne-Regular.ttf", "Assets/Fonts/UITK/PirataOne.asset");
        CreateUITKFont("Assets/Fonts/PixelifySans-Regular.ttf", "Assets/Fonts/UITK/PixelifySans.asset");
        CreateUITKFont("Assets/Fonts/Jersey25-Regular.ttf", "Assets/Fonts/UITK/Jersey25.asset");
        CreateUITKFont("Assets/Fonts/Silkscreen-Regular.ttf", "Assets/Fonts/UITK/Silkscreen.asset");
        AssetDatabase.SaveAssets();
    }

    // caracteres que se hornean en el atlas (ASCII + español + ◆)
    private const string FontCharset =
        " !\"#%&'()*+,-./0123456789:;?¿¡ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "abcdefghijklmnopqrstuvwxyz·…ÁÉÍÓÚÑáéíóúñ◆";

    private static void CreateUITKFont(string ttfPath, string outPath)
    {
        // regenerar siempre para que el atlas quede horneado con los caracteres
        if (AssetDatabase.LoadAssetAtPath<FontAsset>(outPath) != null) AssetDatabase.DeleteAsset(outPath);

        var ttf = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (ttf == null) { Debug.LogWarning($"[Astral Swarm] No se encontró la fuente {ttfPath}"); return; }

        var fa = FontAsset.CreateFontAsset(ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, true);
        if (fa == null) { Debug.LogError($"[Astral Swarm] No se pudo generar el FontAsset de {ttfPath}"); return; }

        fa.name = Path.GetFileNameWithoutExtension(outPath);
        // hornear los glifos en el atlas (si no, el modo dinámico no dibuja en runtime)
        fa.TryAddCharacters(FontCharset);
        // dejarlo estático para que use el atlas horneado directamente
        fa.atlasPopulationMode = AtlasPopulationMode.Static;

        AssetDatabase.CreateAsset(fa, outPath);
        if (fa.atlasTextures != null && fa.atlasTextures.Length > 0)
        {
            fa.atlasTextures[0].name = fa.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
        }
        if (fa.material != null)
        {
            fa.material.name = fa.name + " Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
        }
        EditorUtility.SetDirty(fa);
    }

    private static void SetPointFilter(string path, bool point = true)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.textureType = TextureImporterType.Default;
        imp.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.maxTextureSize = 2048;
        imp.SaveAndReimport();
    }

    private static ThemeStyleSheet GetOrCreateRuntimeTheme()
    {
        var existing = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
        if (existing != null) return existing;

        File.WriteAllText(ThemePath, "@import url(\"unity-theme://default\");\n");
        AssetDatabase.ImportAsset(ThemePath);
        return AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
    }

    private static PanelSettings GetOrCreatePanelSettings(ThemeStyleSheet theme)
    {
        var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
        if (panel == null)
        {
            panel = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panel, PanelPath);
        }
        panel.themeStyleSheet = theme;
        panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panel.referenceResolution = new Vector2Int(1408, 768);
        panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        panel.match = 0.5f;
        EditorUtility.SetDirty(panel);
        AssetDatabase.SaveAssets();
        return panel;
    }
}
