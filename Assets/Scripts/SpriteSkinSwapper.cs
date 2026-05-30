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
    }

    // This method will be called by PlayerClassVisuals with the loaded frames
    public void SetSkin(Sprite[] allSprites)
    {
        skinMap.Clear();
        foreach (var s in allSprites)
        {
            if (s == null) continue;
            // Map by generic name (e.g. Warrior_Idle_0)
            skinMap[s.name] = s;
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer.sprite == null || skinMap.Count == 0) return;
        
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
