using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class MoneyPickup : MonoBehaviour
{
    [SerializeField] private int   moneyAmount = 1;
    [SerializeField] private float magnetSpeed  = 8f;
    [SerializeField] private float bobHeight    = 0.12f;
    [SerializeField] private float bobSpeed     = 3f;

    private Transform      playerTarget;
    private bool           isMagnetized;
    private float          spawnY;
    private float          bobPhase;
    private GameManager    gameManager;

    private static GameManager cachedGameManager;

    public void SetAmount(int amount) => moneyAmount = amount;

    private void Awake()
    {
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
                Collect();
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

    private void Collect()
    {
        if (gameManager != null) gameManager.AddGold(moneyAmount);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPickupSound();
        Destroy(gameObject);
    }
}
