using UnityEngine;

public enum BackgroundStyle
{
    DeepSpace,      // Espacio profundo con estrellas pequeñas
    Nebula,         // Nebulosa colorida con gas
    StarField,      // Campo de estrellas denso
    VoidPurple,     // Vacío morado estilo Vampire Survivors
    BinaryStars     // Sistema estelar binario
}

public class SpaceBackgroundGenerator : MonoBehaviour
{
    [Header("Configuración del Fondo")]
    [SerializeField] private BackgroundStyle style = BackgroundStyle.DeepSpace;
    [SerializeField] private int textureSize = 128;
    [SerializeField] private int seed = 0;

    [Header("Colores Base")]
    [SerializeField] private Color primaryColor = new Color(0.1f, 0.05f, 0.2f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.2f, 0.1f, 0.3f, 1f);
    [SerializeField] private Color accentColor = new Color(0.5f, 0.3f, 0.8f, 1f);

    [Header("Densidad")]
    [Range(0f, 1f)]
    [SerializeField] private float starDensity = 0.02f;
    [Range(0f, 1f)]
    [SerializeField] private float nebulaDensity = 0.3f;

    private Texture2D generatedTexture;
    private Sprite generatedSprite;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        GenerateBackground();
    }

    [ContextMenu("Regenerate Background")]
    public void GenerateBackground()
    {
        if (seed == 0) seed = Random.Range(1, 999999);
        Random.InitState(seed);

        CleanupTextures();

        generatedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        generatedTexture.filterMode = FilterMode.Point;
        generatedTexture.wrapMode = TextureWrapMode.Repeat;

        switch (style)
        {
            case BackgroundStyle.DeepSpace:
                GenerateDeepSpace();
                break;
            case BackgroundStyle.Nebula:
                GenerateNebula();
                break;
            case BackgroundStyle.StarField:
                GenerateStarField();
                break;
            case BackgroundStyle.VoidPurple:
                GenerateVoidPurple();
                break;
            case BackgroundStyle.BinaryStars:
                GenerateBinaryStars();
                break;
        }

        generatedTexture.Apply();

        generatedSprite = Sprite.Create(
            generatedTexture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize / 2f
        );

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = generatedSprite;
    }

    private void GenerateDeepSpace()
    {
        // Fondo base oscuro con gradiente sutil
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                Color baseColor = Color.Lerp(primaryColor, secondaryColor, noise);
                generatedTexture.SetPixel(x, y, baseColor);
            }
        }

        // Añadir estrellas
        AddStars(starDensity);
    }

    private void GenerateNebula()
    {
        // Generar nebulosa con Perlin noise multi-octava
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float noise1 = Mathf.PerlinNoise(x * 0.02f + seed, y * 0.02f + seed);
                float noise2 = Mathf.PerlinNoise(x * 0.05f + seed * 2, y * 0.05f + seed * 2) * 0.5f;
                float noise3 = Mathf.PerlinNoise(x * 0.1f + seed * 3, y * 0.1f + seed * 3) * 0.25f;

                float combined = (noise1 + noise2 + noise3) / 1.75f;
                combined = Mathf.Pow(combined, 2f); // Contraste

                Color nebulaColor = Color.Lerp(primaryColor, accentColor, combined * nebulaDensity);
                generatedTexture.SetPixel(x, y, nebulaColor);
            }
        }

        // Estrellas brillantes sobre la nebulosa
        AddStars(starDensity * 0.5f);
    }

    private void GenerateStarField()
    {
        // Fondo negro puro
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                generatedTexture.SetPixel(x, y, primaryColor);

        // Muchas estrellas de diferentes tamaños
        AddStars(starDensity * 3f);
        AddStars(starDensity * 2f, 2); // Estrellas más grandes
    }

    private void GenerateVoidPurple()
    {
        // Estilo Vampire Survivors: morado oscuro con textura sutil
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                float pattern = Mathf.Sin(x * 0.2f) * Mathf.Sin(y * 0.2f) * 0.1f;

                Color color = Color.Lerp(primaryColor, secondaryColor, noise + pattern);
                generatedTexture.SetPixel(x, y, color);
            }
        }

        // Pocas estrellas pequeñas
        AddStars(starDensity * 0.8f);
    }

    private void GenerateBinaryStars()
    {
        // Fondo con gradiente desde dos puntos (estrellas lejanas)
        Vector2 star1 = new Vector2(textureSize * 0.3f, textureSize * 0.6f);
        Vector2 star2 = new Vector2(textureSize * 0.7f, textureSize * 0.4f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dist1 = Vector2.Distance(new Vector2(x, y), star1) / textureSize;
                float dist2 = Vector2.Distance(new Vector2(x, y), star2) / textureSize;

                float glow1 = Mathf.Clamp01(1f - dist1 * 2f) * 0.3f;
                float glow2 = Mathf.Clamp01(1f - dist2 * 2f) * 0.2f;

                Color baseColor = primaryColor;
                Color glowColor = Color.Lerp(baseColor, accentColor, glow1 + glow2);

                generatedTexture.SetPixel(x, y, glowColor);
            }
        }

        AddStars(starDensity);
    }

    private void AddStars(float density, int size = 1)
    {
        int starCount = Mathf.RoundToInt(textureSize * textureSize * density);
        Color starColor = new Color(1f, 1f, 1f, 1f);

        for (int i = 0; i < starCount; i++)
        {
            int x = Random.Range(0, textureSize);
            int y = Random.Range(0, textureSize);
            float brightness = Random.Range(0.5f, 1f);

            Color star = starColor * brightness;

            if (size == 1)
            {
                generatedTexture.SetPixel(x, y, star);
            }
            else
            {
                // Estrellas más grandes (2x2 o 3x3)
                for (int dy = 0; dy < size; dy++)
                {
                    for (int dx = 0; dx < size; dx++)
                    {
                        int px = (x + dx) % textureSize;
                        int py = (y + dy) % textureSize;
                        generatedTexture.SetPixel(px, py, star);
                    }
                }
            }
        }
    }

    private void CleanupTextures()
    {
        if (generatedSprite != null)
        {
            Destroy(generatedSprite);
            generatedSprite = null;
        }

        if (generatedTexture != null)
        {
            Destroy(generatedTexture);
            generatedTexture = null;
        }
    }

    private void OnDestroy()
    {
        CleanupTextures();
    }

    private void OnValidate()
    {
        // Regenerar en el editor cuando se cambian valores
        if (Application.isPlaying && generatedTexture != null)
        {
            GenerateBackground();
        }
    }
}
