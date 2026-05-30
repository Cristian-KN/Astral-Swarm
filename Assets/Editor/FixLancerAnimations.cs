using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// Arregla las animaciones del lancero:
///  - El Lancer_Run venía sliceado 10×192 cuando la textura es 6×320, lo que
///    partía a los personajes por la mitad (frames vacíos / slivers) → andar roto.
///    Aquí se re-slicea a 6 celdas de 320×320 (rejilla uniforme) en los 5 colores.
///  - Reconstruye los clips Lancer_Idle (12 frames) y Lancer_Run (6 frames) a
///    partir de los sprites azules, EN SU SITIO (conserva el .anim y su GUID, así
///    los AnimatorController siguen apuntando a ellos).
///
/// El Lancer_Idle ya está sliceado a 12×320 (no se re-slicea para no romper las
/// referencias del selector); solo se reconstruye su clip por si referenciaba
/// frames inexistentes (_12/_13).
///
/// Menú: Astral Swarm/Arreglar Animaciones del Lancero
/// </summary>
public static class FixLancerAnimations
{
    private const string UnitsDir = "Assets/Sprites/Downloaded/TinySwords/Units";
    private static readonly string[] Colors = { "Blue", "Yellow", "Red", "Purple", "Black" };

    private const string BlueIdlePath   = UnitsDir + "/Blue Units/Lancer/Lancer_Idle.png";
    private const string BlueRunPath    = UnitsDir + "/Blue Units/Lancer/Lancer_Run.png";
    private const string BlueAttackPath = UnitsDir + "/Blue Units/Lancer/Lancer_Right_Attack.png";
    private const string IdleClipPath   = "Assets/Animations/Units/Lancer_Idle.anim";
    private const string RunClipPath    = "Assets/Animations/Units/Lancer_Run.anim";
    private const string AttackClipPath = "Assets/Animations/Units/Lancer_Attack.anim";

    private const int Cell = 320; // celda nativa del lancero (idle, run y ataque)

    [MenuItem("Astral Swarm/Arreglar Animaciones del Lancero")]
    public static void Fix()
    {
        // 1) Re-slicear las hojas mal cortadas (xN×192) a celdas de 320 en los 5 colores.
        //    Lancer_Run: 1920×320 -> 6 frames ; Lancer_Right_Attack: 960×320 -> 3 frames.
        foreach (string color in Colors)
        {
            ResliceUniform($"{UnitsDir}/{color} Units/Lancer/Lancer_Run.png",          "Lancer_Run");
            ResliceUniform($"{UnitsDir}/{color} Units/Lancer/Lancer_Right_Attack.png", "Lancer_Right_Attack");
        }
        AssetDatabase.Refresh();

        // 2) Reconstruir los clips desde los sprites azules (en su sitio, conservando GUID).
        RebuildClip(IdleClipPath,   BlueIdlePath,   fps: 10f, loop: true);
        RebuildClip(RunClipPath,    BlueRunPath,    fps: 12f, loop: true);
        RebuildClip(AttackClipPath, BlueAttackPath, fps: 12f, loop: false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Astral Swarm",
            "Animaciones del lancero arregladas:\n\n" +
            "• Lancer_Run re-sliceado a 6×320 (andar fluido).\n" +
            "• Lancer_Right_Attack re-sliceado a 3×320 (ataque fluido).\n" +
            "• Clips Idle (12), Run (6) y Attack (3) reconstruidos.\n\n" +
            "Selecciona lancero y dale a Play.", "OK");
    }

    /// <summary>Re-slicea una hoja horizontal en celdas cuadradas de 320×320.</summary>
    private static void ResliceUniform(string assetPath, string namePrefix)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[FixLancer] No encontrado: {assetPath}"); return; }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null) { Debug.LogWarning($"[FixLancer] Textura no cargable: {assetPath}"); return; }

        importer.textureType        = TextureImporterType.Sprite;
        importer.spriteImportMode   = SpriteImportMode.Multiple;
        importer.filterMode         = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 64;
        importer.isReadable          = true;

        int cols = tex.width / Cell;
        var meta = new SpriteMetaData[cols];
        for (int i = 0; i < cols; i++)
        {
            meta[i] = new SpriteMetaData
            {
                name      = $"{namePrefix}_{i}",
                rect      = new Rect(i * Cell, 0, Cell, Cell),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };
        }
        importer.spritesheet = meta;
        importer.SaveAndReimport();
    }

    /// <summary>
    /// Reescribe la curva de sprites de un clip existente (conserva su GUID y
    /// loop) con todos los sprites de la hoja indicada, ordenados por su índice.
    /// </summary>
    private static void RebuildClip(string clipPath, string sheetPath, float fps, bool loop)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) { Debug.LogWarning($"[FixLancer] Clip no encontrado: {clipPath}"); return; }

        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(TrailingIndex)
            .ToArray();

        if (sprites.Length == 0) { Debug.LogWarning($"[FixLancer] Sin sprites en: {sheetPath}"); return; }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        clip.frameRate = fps;

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);
    }

    /// <summary>Extrae el número final del nombre (Lancer_Run_3 → 3) para ordenar.</summary>
    private static int TrailingIndex(Sprite s)
    {
        int us = s.name.LastIndexOf('_');
        if (us >= 0 && int.TryParse(s.name.Substring(us + 1), out int n)) return n;
        return 0;
    }
}
#endif
