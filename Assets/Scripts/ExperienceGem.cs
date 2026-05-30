using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private int   experienceAmount = 10;
    [SerializeField] private float magnetSpeed  = 8f;
    [SerializeField] private float bobHeight    = 0.12f;
    [SerializeField] private float bobSpeed     = 3f;

    private Transform      playerTarget;
    private bool           isMagnetized;
    private SpriteRenderer sr;
    private float          spawnY;
    private float          bobPhase;
    private GameManager    gameManager;

    private static GameManager cachedGameManager;

    private void Awake()
    {
        sr          = GetComponent<SpriteRenderer>();
        spawnY      = transform.position.y;
        bobPhase    = Random.Range(0f, Mathf.PI * 2f);

        // Cache GameManager to avoid expensive FindObjectOfType on every pickup
        if (cachedGameManager == null)
            cachedGameManager = FindFirstObjectByType<GameManager>();
        gameManager = cachedGameManager;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (isMagnetized)
        {
            if (playerTarget == null) { Destroy(gameObject); return; }

            transform.position = Vector3.MoveTowards(
                transform.position, playerTarget.position, magnetSpeed * Time.deltaTime);

            if ((transform.position - playerTarget.position).sqrMagnitude < 0.04f)
                CollectGem();
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = spawnY + Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobHeight;
            transform.position = pos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isMagnetized)
        {
            playerTarget = collision.transform;
            isMagnetized = true;
        }
    }

    public void SetAmount(int amount)
    {
        experienceAmount = amount;
    }

    private void CollectGem()
    {
        if (gameManager != null) gameManager.AddExperience(experienceAmount);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickupSound();
        Destroy(gameObject);
    }
}
