using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("El Transform del jugador al que la cámara debe seguir.")]
    [SerializeField] private Transform targetInfo; 

    [Tooltip("Tiempo de suavizado. Un valor menor hará que la cámara lo siga más rápido.")]
    [SerializeField] private float smoothTime = 0.15f;
    
    [Tooltip("Desplazamiento base de la cámara respecto al jugador en el eje Z (debe ser negativo).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    // Variable de referencia para el algoritmo de suavizado de Unity (SmoothDamp)
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        // Si no hemos asignado un objetivo en el inspector, intentamos buscar al jugador por su Tag.
        if (targetInfo == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetInfo = player.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: No se ha encontrado el tag 'Player' en la escena.");
            }
        }
    }

    // Usamos LateUpdate para calcular la cámara justo después de que el jugador se haya movido.
    private void LateUpdate()
    {
        if (targetInfo == null) return;

        // Calculamos la posición deseada de la cámara sumando el offset a la posición del jugador
        Vector3 targetPosition = targetInfo.position + offset;

        // SmoothDamp transiciona gradualmente un vector hacia el objetivo deseado a lo largo del tiempo
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
