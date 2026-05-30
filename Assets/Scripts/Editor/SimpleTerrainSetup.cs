using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class SimpleTerrainSetup
{
    [MenuItem("GameObject/Setup Terrain Now", false, 0)]
    public static void SetupTerrainNow()
    {
        Debug.Log("Iniciando creación de terreno...");

        // Crear Grid
        GameObject grid = new GameObject("Grid");
        grid.AddComponent<Grid>();

        // Crear Tilemap
        GameObject tilemapObj = new GameObject("Ground");
        tilemapObj.transform.SetParent(grid.transform);
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10;

        Debug.Log("✅ Grid y Tilemap creados. Ahora pinta manualmente o usa el otro script.");
        EditorUtility.DisplayDialog("Terreno Base Creado",
            "Grid y Tilemap creados.\n\nAhora puedes pintar tiles manualmente con el Tile Palette.",
            "OK");
    }
}