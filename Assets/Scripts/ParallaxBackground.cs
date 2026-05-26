using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layerObject;
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f; // 0 = estático, 1 = sigue la cámara completamente
        [HideInInspector]
        public Vector3 startPosition;
    }

    [Header("Configuración Parallax")]
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private bool autoGenerateLayers = true;
    [SerializeField] private int numberOfLayers = 3;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("[ParallaxBackground] No MainCamera found.");
            enabled = false;
            return;
        }

        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        if (autoGenerateLayers && (layers == null || layers.Length == 0))
        {
            GenerateDefaultLayers();
        }

        // Guardar posiciones iniciales
        foreach (var layer in layers)
        {
            if (layer.layerObject != null)
            {
                layer.startPosition = layer.layerObject.transform.position;
            }
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (var layer in layers)
        {
            if (layer.layerObject != null)
            {
                // Mover la capa según su factor de parallax
                Vector3 parallaxOffset = deltaMovement * (1f - layer.parallaxFactor);
                layer.layerObject.transform.position += parallaxOffset;
            }
        }

        lastCameraPosition = cameraTransform.position;
    }

    private void GenerateDefaultLayers()
    {
        layers = new ParallaxLayer[numberOfLayers];

        BackgroundStyle[] styles = new BackgroundStyle[]
        {
            BackgroundStyle.DeepSpace,
            BackgroundStyle.Nebula,
            BackgroundStyle.StarField
        };

        for (int i = 0; i < numberOfLayers; i++)
        {
            GameObject layerObj = new GameObject($"ParallaxLayer_{i}");
            layerObj.transform.SetParent(transform);
            layerObj.transform.localPosition = new Vector3(0, 0, i);

            // Añadir SpriteRenderer
            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -20 - i;

            // Añadir generador de fondo
            SpaceBackgroundGenerator gen = layerObj.AddComponent<SpaceBackgroundGenerator>();

            // Configurar el generador
            if (i < styles.Length)
            {
                // Usar reflexión para setear el enum (o hacerlo manualmente)
                var styleField = gen.GetType().GetField("style",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (styleField != null)
                    styleField.SetValue(gen, styles[i]);
            }

            // Configurar capa de parallax
            layers[i] = new ParallaxLayer
            {
                layerObject = layerObj,
                parallaxFactor = 0.2f + (i * 0.3f) // Capas más lejanas se mueven más lento
            };
        }

        Debug.Log($"[ParallaxBackground] Generadas {numberOfLayers} capas automáticamente.");
    }

    [ContextMenu("Regenerate All Layers")]
    public void RegenerateAllLayers()
    {
        foreach (var layer in layers)
        {
            if (layer.layerObject != null)
            {
                var generator = layer.layerObject.GetComponent<SpaceBackgroundGenerator>();
                if (generator != null)
                {
                    generator.GenerateBackground();
                }
            }
        }
    }
}
