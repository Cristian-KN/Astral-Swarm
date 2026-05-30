using UnityEngine;
using System.Collections.Generic;

public class SpriteSkinSwapper : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, Sprite> skinMap = new Dictionary<string, Sprite>();
    private string currentClass;
    private string currentColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateSkin()
    {
        currentClass = PlayerPrefs.GetString("SelectedClass", "warrior").ToLower();
        currentColor = PlayerPrefs.GetString("SelectedColor", "blue").ToLower();

        LoadSkinMap();
    }

    private void LoadSkinMap()
    {
        skinMap.Clear();
        
        // Find all folders for this class across all colors to map them
        // Actually, we only need to load the target color's sprites
        string colorFolder = char.ToUpper(currentColor[0]) + currentColor.Substring(1) + " Units";
        string classFolder = char.ToUpper(currentClass[0]) + currentClass.Substring(1);
        
        // This is a bit slow at runtime if we use FindAssets, but we only do it once at Start/Skin Change.
        // In a real project, we'd use the ScriptableObject Library.
        // I will implement a basic version that tries to load from the project structure.
        
        // Since I don't have access to AssetDatabase in runtime, I'll rely on the 
        // UnitSkinLibrary being assigned to PlayerClassVisuals.
    }

    // This method will be called by PlayerClassVisuals with the loaded frames
    public void SetSkin(Sprite[] allSprites)
    {
        skinMap.Clear();
        foreach (var s in allSprites)
        {
            if (s == null) continue;
            // Map by generic name (e.g. Warrior_Idle_0)
            // TinySwords names are already generic enough
            skinMap[s.name] = s;
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer.sprite == null) return;
        
        string spriteName = spriteRenderer.sprite.name;
        
        if (skinMap.TryGetValue(spriteName, out Sprite newSprite))
        {
            if (spriteRenderer.sprite != newSprite)
            {
                spriteRenderer.sprite = newSprite;
            }
        }
    }
}
