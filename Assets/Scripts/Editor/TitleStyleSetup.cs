using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using TMPro;

/// <summary>
/// Da al título "ASTRAL SWARM" del menú principal una estética pixel-art medieval:
/// genera el TMP Font Asset SDF a partir de la fuente Jacquard 12 (OFL) y le aplica
/// degradado oro→bronce, contorno oscuro y sombra proyectada.
///
/// Uso: menú  Tools > Astral Swarm > Aplicar Estilo del Título
/// (La generación del Font Asset SDF requiere el editor de Unity, por eso va aquí.)
/// </summary>
public static class TitleStyleSetup
{
    private const string TtfPath = "Assets/Fonts/Jacquard12-Regular.ttf";
    private const string FontAssetPath = "Assets/Fonts/Jacquard12 SDF.asset";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string TitleObjectName = "TitleText";

    [MenuItem("Tools/Astral Swarm/Aplicar Estilo del Título")]
    public static void ApplyTitleStyle()
    {
        TMP_FontAsset fontAsset = GetOrCreateFontAsset();
        if (fontAsset == null) return;

        TextMeshProUGUI title = FindTitle();
        if (title == null)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            title = FindTitle();
        }
        if (title == null)
        {
            Debug.LogError($"[Astral Swarm] No se encontró un objeto '{TitleObjectName}' en la escena {MainMenuScenePath}.");
            return;
        }

        StyleTitle(title, fontAsset);

        EditorUtility.SetDirty(title);
        Scene scene = title.gameObject.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Astral Swarm] Estilo medieval aplicado al título correctamente.");
    }

    private static TMP_FontAsset GetOrCreateFontAsset()
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null) return existing;

        Font ttf = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (ttf == null)
        {
            Debug.LogError($"[Astral Swarm] No se encontró la fuente en {TtfPath}.");
            return null;
        }

        // Atlas SDF 1024x1024, dinámico: los glifos se hornean bajo demanda.
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        if (fontAsset == null)
        {
            Debug.LogError("[Astral Swarm] No se pudo generar el TMP Font Asset.");
            return null;
        }
        fontAsset.name = "Jacquard12 SDF";

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
        {
            fontAsset.atlasTextures[0].name = "Jacquard12 Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        }
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        // Asegura que las letras del título existan en el atlas antes de guardar.
        fontAsset.TryAddCharacters("ASTRAL SWRM");

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);
        Debug.Log($"[Astral Swarm] TMP Font Asset creado en {FontAssetPath}.");
        return fontAsset;
    }

    private static TextMeshProUGUI FindTitle()
    {
        foreach (TextMeshProUGUI t in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
        {
            if (t.gameObject.name == TitleObjectName && t.gameObject.scene.IsValid())
                return t;
        }
        return null;
    }

    private static void StyleTitle(TextMeshProUGUI t, TMP_FontAsset fontAsset)
    {
        // El contenedor del título estaba anclado en Y 0.85 con offset +350, lo que lo
        // empujaba FUERA del borde superior de la pantalla. Lo reanclamos arriba-centro,
        // colgando del borde superior, para que quede visible sobre el pergamino.
        RectTransform container = t.rectTransform.parent as RectTransform;
        if (container != null)
        {
            container.anchorMin = new Vector2(0.5f, 1f);
            container.anchorMax = new Vector2(0.5f, 1f);
            container.pivot = new Vector2(0.5f, 1f);
            container.anchoredPosition = new Vector2(0f, -40f);
            container.sizeDelta = new Vector2(1000f, 220f);
            EditorUtility.SetDirty(container);
        }

        t.font = fontAsset;
        t.text = "ASTRAL SWARM";
        t.fontStyle = FontStyles.Bold;
        t.enableAutoSizing = false;
        t.fontSize = 120;
        t.characterSpacing = 6;
        t.alignment = TextAlignmentOptions.Center;

        // Degradado oro → bronce, muy de cartel medieval.
        var gold = new Color32(255, 224, 120, 255);
        var bronze = new Color32(196, 132, 48, 255);
        t.enableVertexGradient = true;
        t.colorGradient = new VertexGradient(gold, gold, bronze, bronze);

        // Contorno oscuro para que las letras pixeladas resalten sobre el fondo.
        t.outlineColor = new Color32(40, 22, 10, 255);
        t.outlineWidth = 0.22f;

        // Sombra proyectada (underlay) para dar profundidad.
        Material mat = t.fontMaterial;
        if (mat != null && mat.HasProperty(ShaderUtilities.ID_UnderlayColor))
        {
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.75f));
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1.2f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1.2f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.1f);
        }
    }
}
