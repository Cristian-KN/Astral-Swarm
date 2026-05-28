using UnityEngine;
using UnityEditor;
using System.IO;

public class ProjectAutoConfigurator : EditorWindow
{
    [MenuItem("Astral Swarm/Configurar Sprites (Pixel Art)")]
    public static void ConfigureSprites()
    {
        string path = "Assets/Sprites/Downloads";
        if (!Directory.Exists(path))
        {
            Debug.LogError("No se encontró la carpeta de descargas. Ejecuta primero el script de Python.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                // Configuración óptima para Pixel Art de TFG
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.pixelsPerUnit = 32; // Ajustar según el asset

                AssetDatabase.ImportAsset(assetPath);
                count++;
            }
        }

        Debug.Log($"[Astral Swarm] Se han configurado {count} texturas automáticamente.");
    }

    [MenuItem("Astral Swarm/Crear Estructura de Carpetas")]
    public static void CreateFolders()
    {
        string[] folders = { "Scripts", "Sprites", "Prefabs", "Scenes", "Sounds", "Scripts/Editor" };
        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + folder))
                AssetDatabase.CreateFolder("Assets", folder);
        }
        Debug.Log("[Astral Swarm] Estructura de carpetas creada.");
    }
}