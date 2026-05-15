using UnityEngine;

/// <summary>
/// Aplica el color correspondiente a la variante del enemigo en tiempo de ejecución.
/// Esto evita tener que crear 35+ sprites diferentes manualmente.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyColorizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ApplyColor(EnemyVariantType variant)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (variant)
        {
            case EnemyVariantType.Normal:
                spriteRenderer.color = Color.white;
                break;
            case EnemyVariantType.Verde:
                spriteRenderer.color = new Color(0.2f, 1f, 0.2f); // Verde Esmeralda
                break;
            case EnemyVariantType.Amarilla:
                spriteRenderer.color = new Color(1f, 0.9f, 0.2f); // Dorado/Amarillo
                break;
            case EnemyVariantType.Azul:
                spriteRenderer.color = new Color(0.2f, 0.8f, 1f); // Cian/Azul Eléctrico
                break;
            case EnemyVariantType.Morada:
                spriteRenderer.color = new Color(0.7f, 0.2f, 1f); // Púrpura
                break;
            case EnemyVariantType.Negra:
                spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f); // Gris Oscuro/Negro
                break;
            case EnemyVariantType.Roja:
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f); // Carmesí Intenso
                break;
        }
    }
}
