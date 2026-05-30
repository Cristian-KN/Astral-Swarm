using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// Asigna Assets/Icon/AppIcon.png como icono por defecto del juego (se hornea en el .exe).
/// Menú: Astral Swarm > Set App Icon.  Batch: SetAppIcon.ApplyAndBuild (icono + build).
/// </summary>
public static class SetAppIcon
{
    const string IconPath = "Assets/Icon/AppIcon.png";

    [MenuItem("Astral Swarm/Set App Icon")]
    public static void Apply()
    {
        var tex = LoadIcon();
        if (tex == null) return;
        SetDefault(tex);
        AssetDatabase.SaveAssets();
        Debug.Log("[Icon] Icono aplicado.");
    }

    // Para el build headless: pone el icono y luego compila (BuildWindows.Build sale del editor).
    public static void ApplyAndBuild()
    {
        var tex = LoadIcon();
        if (tex != null) { SetDefault(tex); AssetDatabase.SaveAssets(); }
        else Debug.LogError("[Icon] Sin icono; compilo sin él.");
        BuildWindows.Build();
    }

    static Texture2D LoadIcon()
    {
        AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        if (imp != null)
        {
            imp.textureType = TextureImporterType.Default;
            imp.isReadable = true;
            imp.mipmapEnabled = false;
            imp.maxTextureSize = 1024;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (tex == null) Debug.LogError("[Icon] No se encontró " + IconPath);
        return tex;
    }

    static void SetDefault(Texture2D tex)
    {
        // Default Icon (el que usa el .exe si no hay iconos específicos de plataforma)
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { tex });

        // Y Standalone explícito: misma textura en todos los tamaños
        var nbt = NamedBuildTarget.Standalone;
        int[] sizes = PlayerSettings.GetIconSizes(nbt, IconKind.Application);
        if (sizes != null && sizes.Length > 0)
        {
            var icons = new Texture2D[sizes.Length];
            for (int i = 0; i < icons.Length; i++) icons[i] = tex;
            PlayerSettings.SetIcons(nbt, icons, IconKind.Application);
        }
    }
}
#endif
