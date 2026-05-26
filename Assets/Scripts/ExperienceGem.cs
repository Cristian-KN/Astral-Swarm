using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private int   experienceAmount = 10;
    [SerializeField] private float magnetSpeed  = 8f;
    [SerializeField] private float bobHeight    = 0.12f;
    [SerializeField] private float bobSpeed     = 3f;

    private Transform      playerTarget;
    private bool           isMagnetized;
    private SpriteRenderer sr;
    private float          spawnY;
    private float          bobPhase;
    private GameManager    gameManager;

    private static Sprite sharedCircle;

    private void Awake()
    {
        sr          = GetComponent<SpriteRenderer>();
        sr.sprite   = GetCircleSprite();
        spawnY      = transform.position.y;
        bobPhase    = Random.Range(0f, Mathf.PI * 2f);
        gameManager = FindObjectOfType<GameManager>();
        ApplyColorTier();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (isMagnetized)
        {
            if (playerTarget == null) { Destroy(gameObject); return; }

            transform.position = Vector3.MoveTowards(
                transform.position, playerTarget.position, magnetSpeed * Time.deltaTime);

            if ((transform.position - playerTarget.position).sqrMagnitude < 0.04f)
                CollectGem();
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = spawnY + Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobHeight;
            transform.position = pos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMagnetized)
        {
            playerTarget = collision.transform;
            isMagnetized = true;
        }
    }

    private void ApplyColorTier()
    {
        if      (experienceAmount <= 10) sr.color = new Color(0.3f,  1f,   0.4f);
        else if (experienceAmount <= 30) sr.color = new Color(0.3f,  0.6f, 1f);
        else                             sr.color = new Color(0.85f, 0.3f, 1f);
    }

    private static Sprite GetCircleSprite()
    {
        if (sharedCircle != null) return sharedCircle;

        const int size   = 16;
        const float ppu  = 16f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float radius = center - 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - center, dy = y - center;
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        tex.Apply();

        sharedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        return sharedCircle;
    }

    private void CollectGem()
    {
        if (gameManager != null)
        {
            // Aplicar multiplicador de bioma
            float finalExp = experienceAmount;
            BiomeManager biomeManager = BiomeManager.Instance;
            if (biomeManager != null)
            {
                finalExp *= biomeManager.GetExpMultiplier();
            }

            gameManager.AddExperience(Mathf.RoundToInt(finalExp));
        }
        Destroy(gameObject);
    }
}
