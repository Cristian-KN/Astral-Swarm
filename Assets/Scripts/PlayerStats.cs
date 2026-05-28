using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Necesario si usamos UI clásica de Unity

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas de Vida")]
    public float maxHealth = 100;
    private float currentHealth;

    [Header("Feedback")]
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float redFlashDuration = 0.15f;

    [Header("Estadísticas de Combate")]
    public float attackPower = 10f;
    public float attackSpeed = 1f;
    public float luck = 1f;
    public float defense = 0f; // Reducción plana o porcentual
    public float cooldownReduction = 0f; // 0 a 1 (0.2 = 20% menos)
    public float difficulty = 0f; // Aumenta con items de Sacrificio

    [Header("Multiplicadores (Suma de objetos)")]
    public float attackMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float luckMultiplier = 1f;
    public float difficultyMultiplier = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;
    private UIManager uiManager;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIManager>();
        uiManager?.UpdateHealth(currentHealth, maxHealth);
    }

    /// <summary>
    /// Función principal de usabilidad y daño. Se puede llamar desde los enemigos cuando tocan al jugador.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        // Aplicar defensa (Reducción plana, mínimo 1 de daño)
        float finalDamage = Mathf.Max(1, damage - defense);
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        uiManager?.UpdateHealth(currentHealth, maxHealth);

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
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.TriggerGameOver();
        gameObject.SetActive(false);
    }
}
