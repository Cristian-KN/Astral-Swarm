using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExperienceGem : MonoBehaviour
{
    [Header("Configuración de la Gema")]
    [Tooltip("Cantidad de experiencia que da esta gema al recogerla.")]
    [SerializeField] private int experienceAmount = 10;
    
    [Tooltip("Fuerza para atraer la gema al jugador si la toca de refilón (opcional)")]
    [SerializeField] private float magnetSpeed = 5f;

    private Transform playerTarget;
    private bool isMagnetized = false;

    private void Start()
    {
        // Asegurarnos que el collider actúe como Trigger para no empujar al jugador físicamente
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // Pequeño efecto visual extra opcional: Si el jugador la absorbe, vuela hacia él antes de desaparecer
        if (isMagnetized && playerTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, magnetSpeed * Time.deltaTime);
            
            // Si está muy cerca del jugador, la "comemos" definitivamente
            if (Vector3.Distance(transform.position, playerTarget.position) < 0.2f)
            {
                CollectGem();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMagnetized)
        {
            // Empezar a atraer la gema hacia el centro del jugador visualmente
            playerTarget = collision.transform;
            isMagnetized = true;

            // O si no queremos el efecto imán, simplemente la recogemos aquí directamente:
            // CollectGem();
        }
    }

    private void CollectGem()
    {
        // Buscamos el GameManager para sumarle la EXP (lo crearemos en la Fase 6)
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddExperience(experienceAmount);
        }

        // Reproducir aquí un sonido de "Ding!"
        
        Destroy(gameObject);
    }
}
