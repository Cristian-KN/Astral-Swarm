using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitSkinLibrary", menuName = "ScriptableObjects/UnitSkinLibrary")]
public class UnitSkinLibrary : ScriptableObject
{
    [System.Serializable]
    public class AnimationSet
    {
        public string animationName; // e.g. Idle, Run, Attack
        public Sprite[] frames;
    }

    [System.Serializable]
    public class ColorSkin
    {
        public string colorName; // Blue, Yellow, Red, Purple, Black
        public List<AnimationSet> animations = new List<AnimationSet>();
        
        public Sprite[] GetAnimation(string name)
        {
            var anim = animations.Find(a => a.animationName.ToLower() == name.ToLower());
            return anim != null ? anim.frames : null;
        }
    }

    [System.Serializable]
    public class UnitClass
    {
        public string className; // Warrior, Archer, Lancer
        public List<ColorSkin> skins = new List<ColorSkin>();
        
        public ColorSkin GetSkin(string color)
        {
            var skin = skins.Find(s => s.colorName.ToLower() == color.ToLower());
            return skin;
        }
    }

    public List<UnitClass> unitClasses = new List<UnitClass>();

    public Sprite[] GetAnimation(string className, string colorName, string animName)
    {
        var unit = unitClasses.Find(u => u.className.ToLower() == className.ToLower());
        if (unit == null) return null;
        
        var skin = unit.GetSkin(colorName);
        if (skin == null) return null;
        
        return skin.GetAnimation(animName);
    }
}
