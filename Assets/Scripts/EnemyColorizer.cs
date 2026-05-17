using UnityEngine;

public class EnemyColorizer : MonoBehaviour
{
    private SpriteRenderer sr;

    public void ApplyColor(EnemyVariantType variant)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        // Aplicamos los colores definidos en EnemyVariants.md
        switch (variant)
        {
            case EnemyVariantType.Verde:    sr.color = new Color(0.4f, 1f, 0.4f); break; // Esmeralda
            case EnemyVariantType.Amarilla: sr.color = new Color(1f, 1f, 0.4f); break; // Dorado
            case EnemyVariantType.Azul:     sr.color = new Color(0.4f, 0.8f, 1f); break; // Cian
            case EnemyVariantType.Morada:   sr.color = new Color(0.8f, 0.4f, 1f); break; // Púrpura
            case EnemyVariantType.Negra:    sr.color = new Color(0.2f, 0.2f, 0.2f); break; // Oscuro
            case EnemyVariantType.Roja:     sr.color = new Color(1f, 0.2f, 0.2f); break; // Carmesí
            default:                        sr.color = Color.white; break;
        }
    }
}