using UnityEngine;

/// <summary>
/// Aplica automáticamente SpriteOutline a objetos específicos del juego.
/// Útil para configurar outlines en prefabs de enemigos, jugador, etc.
/// </summary>
public class AutoApplyOutline : MonoBehaviour
{
    [Header("Aplicar Outline Automáticamente")]
    [Tooltip("Aplicar outline a todos los enemigos al spawn")]
    [SerializeField] private bool applyToEnemies = true;

    [Tooltip("Aplicar outline al jugador")]
    [SerializeField] private bool applyToPlayer = true;

    [Tooltip("Aplicar outline a proyectiles")]
    [SerializeField] private bool applyToProjectiles = false;

    [Header("Configuración por Defecto")]
    [SerializeField] private Color defaultOutlineColor = Color.black;
    [SerializeField] private int defaultOutlineSize = 1;
    [SerializeField] private bool adaptiveByDefault = true;

    private void Start()
    {
        if (applyToPlayer)
        {
            ApplyToPlayer();
        }

        if (applyToEnemies)
        {
            // Los enemigos se configuran cuando se instancian
            // Subscribirse al spawn de enemigos si hay un sistema
        }

        if (applyToProjectiles)
        {
            // Similar para proyectiles
        }
    }

    private void ApplyToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            AddOutlineIfMissing(player, Color.white, 2, true);
        }
    }

    /// <summary>
    /// Añade SpriteOutline a un GameObject si no lo tiene ya
    /// </summary>
    public static void AddOutlineIfMissing(GameObject obj, Color color, int size, bool adaptive)
    {
        if (obj == null) return;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        SpriteOutline outline = obj.GetComponent<SpriteOutline>();
        if (outline == null)
        {
            outline = obj.AddComponent<SpriteOutline>();
            outline.SetOutlineColor(color);
            outline.SetOutlineSize(size);
            outline.SetAdaptive(adaptive);
        }
    }

    /// <summary>
    /// Llamar esto cuando se instancia un enemigo
    /// </summary>
    public void OnEnemySpawned(GameObject enemy)
    {
        if (applyToEnemies)
        {
            AddOutlineIfMissing(enemy, defaultOutlineColor, defaultOutlineSize, adaptiveByDefault);
        }
    }

    /// <summary>
    /// Llamar esto cuando se instancia un proyectil
    /// </summary>
    public void OnProjectileSpawned(GameObject projectile)
    {
        if (applyToProjectiles)
        {
            AddOutlineIfMissing(projectile, defaultOutlineColor, 1, adaptiveByDefault);
        }
    }
}
