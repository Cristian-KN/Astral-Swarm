using UnityEngine;
using System.Collections.Generic;

public class PlayerClassVisuals : MonoBehaviour
{
    [Header("Library")]
    public UnitSkinLibrary skinLibrary;

    [Header("Animators")]
    public RuntimeAnimatorController warriorController;
    public RuntimeAnimatorController archerController;
    public RuntimeAnimatorController lancerController;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplySelection();
    }

    public void ApplySelection()
    {
        string selectedClass = PlayerPrefs.GetString("SelectedClass", "warrior").ToLower();
        string selectedColor = PlayerPrefs.GetString("SelectedColor", "blue").ToLower();
        
        float scale = 1f;

        switch (selectedClass)
        {
            case "warrior":
                animator.runtimeAnimatorController = warriorController;
                scale = 1.1f;
                break;
            case "archer":
                animator.runtimeAnimatorController = archerController;
                scale = 1.15f;
                break;
            case "lancer":
                animator.runtimeAnimatorController = lancerController;
                scale = 1.12f;
                break;
            default:
                animator.runtimeAnimatorController = warriorController;
                scale = 1.1f;
                break;
        }

        transform.localScale = new Vector3(scale, scale, 1f);

        // Add or update SpriteSkinSwapper
        SpriteSkinSwapper swapper = GetComponent<SpriteSkinSwapper>();
        if (swapper == null) swapper = gameObject.AddComponent<SpriteSkinSwapper>();
        
        // Load all sprites for the selected combination from library
        if (skinLibrary != null)
        {
            List<Sprite> allFrames = new List<Sprite>();
            var unit = skinLibrary.unitClasses.Find(u => u.className.ToLower() == selectedClass);
            if (unit != null)
            {
                var skin = unit.skins.Find(s => s.colorName.ToLower() == selectedColor);
                if (skin != null)
                {
                    foreach (var anim in skin.animations)
                    {
                        allFrames.AddRange(anim.frames);
                    }
                }
            }
            swapper.SetSkin(allFrames.ToArray());
        }
    }
}
