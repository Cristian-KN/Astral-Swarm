using UnityEngine;

/// <summary>
/// Añade un borde/outline a sprites para que resalten sobre fondos complejos.
/// Usa un shader custom para eficiencia o fallback a método de instancias.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    [Header("Configuración del Outline")]
    [Tooltip("Color del borde (negro para fondos claros, blanco para oscuros)")]
    [SerializeField] private Color outlineColor = Color.black;

    [Tooltip("Grosor del outline en píxeles")]
    [Range(1, 5)]
    [SerializeField] private int outlineSize = 1;

    [Tooltip("Cambiar automáticamente entre blanco/negro según el bioma")]
    [SerializeField] private bool adaptiveColor = true;

    [Header("Método de Renderizado")]
    [Tooltip("Usar shader (más rápido) o instancias (fallback)")]
    [SerializeField] private OutlineMethod method = OutlineMethod.Shader;

    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;
    private Material originalMaterial;

    // Para método de instancias
    private GameObject[] outlineInstances;

    private static readonly string OUTLINE_SHADER = "Sprites/Outline";
    private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineSizeProperty = Shader.PropertyToID("_OutlineSize");

    public enum OutlineMethod
    {
        Shader,     // Usa shader custom (recomendado)
        Instances   // Crea múltiples copias del sprite (fallback)
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.sharedMaterial;

        InitializeOutline();
    }

    private void Start()
    {
        if (adaptiveColor)
        {
            UpdateAdaptiveColor();
        }
    }

    private void InitializeOutline()
    {
        if (method == OutlineMethod.Shader)
        {
            InitializeShaderOutline();
        }
        else
        {
            InitializeInstanceOutline();
        }
    }

    private void InitializeShaderOutline()
    {
        Shader outlineShader = Shader.Find(OUTLINE_SHADER);

        if (outlineShader != null)
        {
            // Crear material instanciado con el shader de outline
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.SetColor(OutlineColorProperty, outlineColor);
            outlineMaterial.SetFloat(OutlineSizeProperty, outlineSize);

            spriteRenderer.material = outlineMaterial;
        }
        else
        {
            Debug.LogWarning($"[SpriteOutline] Shader '{OUTLINE_SHADER}' no encontrado. Usando método de instancias.");
            method = OutlineMethod.Instances;
            InitializeInstanceOutline();
        }
    }

    private void InitializeInstanceOutline()
    {
        // Crear 8 copias del sprite desplazadas (N, NE, E, SE, S, SW, W, NW)
        outlineInstances = new GameObject[8];

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * outlineSize * 0.01f,
                Mathf.Sin(angle) * outlineSize * 0.01f,
                0.001f // Ligeramente detrás
            );

            GameObject instance = new GameObject($"Outline_{i}");
            instance.transform.SetParent(transform);
            instance.transform.localPosition = offset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
            sr.sprite = spriteRenderer.sprite;
            sr.color = outlineColor;
            sr.sortingLayerName = spriteRenderer.sortingLayerName;
            sr.sortingOrder = spriteRenderer.sortingOrder - 1;

            outlineInstances[i] = instance;
        }
    }

    private void Update()
    {
        if (adaptiveColor)
        {
            UpdateAdaptiveColor();
        }

        // Sincronizar sprite si usa instancias
        if (method == OutlineMethod.Instances && outlineInstances != null)
        {
            SyncSpriteInstances();
        }
    }

    private void UpdateAdaptiveColor()
    {
        BiomeManager biomeManager = BiomeManager.Instance;
        if (biomeManager == null) return;

        BiomeData currentBiome = biomeManager.GetCurrentBiome();
        if (currentBiome == null) return;

        // Calcular luminosidad del fondo
        float brightness = (currentBiome.primaryColor.r +
                           currentBiome.primaryColor.g +
                           currentBiome.primaryColor.b) / 3f;

        // Decidir color del outline
        Color targetColor = brightness < 0.25f ? Color.white : Color.black;

        // Solo actualizar si cambió
        if (Vector4.Distance(outlineColor, targetColor) > 0.01f)
        {
            outlineColor = targetColor;
            ApplyOutlineColor();
        }
    }

    private void ApplyOutlineColor()
    {
        if (method == OutlineMethod.Shader && outlineMaterial != null)
        {
            outlineMaterial.SetColor(OutlineColorProperty, outlineColor);
        }
        else if (method == OutlineMethod.Instances && outlineInstances != null)
        {
            foreach (GameObject instance in outlineInstances)
            {
                if (instance != null)
                {
                    SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = outlineColor;
                }
            }
        }
    }

    private void SyncSpriteInstances()
    {
        foreach (GameObject instance in outlineInstances)
        {
            if (instance != null)
            {
                SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != spriteRenderer.sprite)
                {
                    sr.sprite = spriteRenderer.sprite;
                }
            }
        }
    }

    // API pública
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        ApplyOutlineColor();
    }

    public void SetOutlineSize(int size)
    {
        outlineSize = Mathf.Clamp(size, 1, 5);

        if (method == OutlineMethod.Shader && outlineMaterial != null)
        {
            outlineMaterial.SetFloat(OutlineSizeProperty, outlineSize);
        }
        else if (method == OutlineMethod.Instances)
        {
            // Recrear instancias con nuevo tamaño
            CleanupInstances();
            InitializeInstanceOutline();
        }
    }

    public void SetAdaptive(bool adaptive)
    {
        adaptiveColor = adaptive;
        if (adaptive)
        {
            UpdateAdaptiveColor();
        }
    }

    private void CleanupInstances()
    {
        if (outlineInstances != null)
        {
            foreach (GameObject instance in outlineInstances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            outlineInstances = null;
        }
    }

    private void OnDestroy()
    {
        CleanupInstances();

        if (outlineMaterial != null)
        {
            Destroy(outlineMaterial);
        }
    }

    private void OnValidate()
    {
        // Aplicar cambios en el editor
        if (Application.isPlaying && spriteRenderer != null)
        {
            ApplyOutlineColor();

            if (method == OutlineMethod.Shader && outlineMaterial != null)
            {
                outlineMaterial.SetFloat(OutlineSizeProperty, outlineSize);
            }
        }
    }
}
