using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Necesario si usamos UI clásica de Unity

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas de Vida")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Retroalimentación y Usabilidad")]
    [Tooltip("Tiempo de invulnerabilidad tras recibir daño (i-frames).")]
    [SerializeField] private float invulnerabilityTime = 1f;
    [Tooltip("Duración del efecto de color rojo (feedback visual).")]
    [SerializeField] private float redFlashDuration = 0.15f;

    [Header("Referencias Opcionales para UI (para la memoria)")]
    // [SerializeField] private Slider healthSlider; // PENDIENTE: Asignar en Fase 5
    // [SerializeField] private AudioClip hitSound;  // PENDIENTE: Asignar audio

    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Si tenemos UI asiganada, la actualizamos al inicio
        // if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth; }
    }

    /// <summary>
    /// Función principal de usabilidad y daño. Se puede llamar desde los enemigos cuando tocan al jugador.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return; // Ignora el daño si está invulnerable.

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Actualizar UI
        Debug.Log("Vida actual del jugador: " + currentHealth);
        // if (healthSlider != null) healthSlider.value = currentHealth;

        // Feedback Auditivo
        // if (hitSound != null) AudioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Feedback Visual e Invulnerabilidad
            StartCoroutine(DamageFeedbackCoroutine());
        }
    }

    /// <summary>
    /// Corrutina que gestiona la usabilidad visual (poner el sprite rojo) y la invulnerabilidad mecánica.
    /// </summary>
    private IEnumerator DamageFeedbackCoroutine()
    {
        isInvulnerable = true;

        // 1. Feedback visual inmediato (Sprite Rojo)
        spriteRenderer.color = Color.red;

        // 2. Esperamos la duración del parpadeo rojo
        yield return new WaitForSeconds(redFlashDuration);

        // 3. Volvemos al color normal
        spriteRenderer.color = Color.white;

        // 4. Si queremos un efecto de parpadeo (transparencia) durante el resto del tiempo de invulnerabilidad:
        float flashTime = invulnerabilityTime - redFlashDuration;
        float elapsed = 0f;
        while(elapsed < flashTime)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // Semitransparente
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white; // Opaco
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        spriteRenderer.color = Color.white;
        isInvulnerable = false; // El jugador vuelve a ser vulnerable.
    }

    private void Die()
    {
        Debug.Log("¡El Hechicero ha caído! Fin de la partida.");
        // Aquí conectaremos con el GameManager en la Fase 6 para mostrar la pantalla de Game Over.
        gameObject.SetActive(false); // Por ahora, lo ocultamos.
    }
}
