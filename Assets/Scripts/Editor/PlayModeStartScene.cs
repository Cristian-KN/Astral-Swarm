using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Fuerza que al pulsar Play en el editor el juego arranque SIEMPRE desde el
/// menú principal (MainMenu), sin importar qué escena tengas abierta.
/// En las builds esto ya lo garantiza el orden de Build Settings (MainMenu = índice 0),
/// así que el menú principal aparece nada más abrir el juego en ambos casos.
/// </summary>
[InitializeOnLoad]
public static class PlayModeStartScene
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    static PlayModeStartScene()
    {
        var mainMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
        if (mainMenu != null)
        {
            EditorSceneManager.playModeStartScene = mainMenu;
        }
        else
        {
            Debug.LogWarning($"[PlayModeStartScene] No se encontró la escena en {MainMenuPath}");
        }
    }
}
