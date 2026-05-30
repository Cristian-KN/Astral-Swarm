using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// Genera un build Standalone Windows 64-bit del juego.
/// Uso en editor: Astral Swarm > Build Windows (.exe)
/// Uso en batch:  Unity.exe -batchmode -projectPath ... -executeMethod BuildWindows.Build -quit
/// La salida queda en  Builds/AstralSwarm-Win64/Astral Swarm.exe  (raíz del proyecto).
/// </summary>
public static class BuildWindows
{
    private const string OutDir = "Builds/AstralSwarm-Win64";
    private const string ExeName = "Astral Swarm.exe";

    [MenuItem("Astral Swarm/Build Windows (.exe)")]
    public static void Build()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[Build] No hay escenas habilitadas en Build Settings.");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutDir + "/" + ExeName,
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None,
        };

        Debug.Log("[Build] Escenas: " + string.Join(", ", scenes));
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"[Build] Resultado={summary.result}  bytes={summary.totalSize}  salida={summary.outputPath}");

        if (Application.isBatchMode)
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
#endif
