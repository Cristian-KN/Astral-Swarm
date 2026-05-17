using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    [Tooltip("How many world units from the player the minimap edge represents.")]
    [SerializeField] private float worldRadius = 25f;

    private RectTransform    panel;
    private Transform        playerTransform;
    private readonly List<Image> dotPool = new List<Image>();
    private int usedDots;

    private static readonly Color ColorPlayer = Color.white;
    private static readonly Color ColorEnemy  = new Color(1f, 0.25f, 0.2f);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p) playerTransform = p.transform;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        usedDots = 0;

        // Player dot at center
        PlaceDot(Vector2.zero, ColorPlayer, 10f);

        // Enemy dots relative to player
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector2 offset = (Vector2)(enemy.transform.position - playerTransform.position);
            Vector2 norm   = Vector2.ClampMagnitude(offset / worldRadius, 1f);
            PlaceDot(norm, ColorEnemy, 6f);
        }

        // Hide unused pooled dots
        for (int i = usedDots; i < dotPool.Count; i++)
            dotPool[i].gameObject.SetActive(false);
    }

    private void PlaceDot(Vector2 normalizedPos, Color color, float size)
    {
        Image dot   = GetDot(usedDots++);
        var   rt    = dot.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(
            normalizedPos.x * panel.rect.width  * 0.5f,
            normalizedPos.y * panel.rect.height * 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        dot.color    = color;
        dot.gameObject.SetActive(true);
    }

    private Image GetDot(int index)
    {
        if (index < dotPool.Count) return dotPool[index];
        var go = new GameObject("MinimapDot", typeof(RectTransform));
        go.transform.SetParent(panel, false);
        var img = go.AddComponent<Image>();
        dotPool.Add(img);
        return img;
    }
}
