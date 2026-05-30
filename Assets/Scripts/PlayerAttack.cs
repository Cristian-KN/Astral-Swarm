using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public ItemData weaponData;
        public int level = 1;
        public float timer = 0f;
    }

    [Header("Arma inicial")]
    [SerializeField] private ItemData defaultWeapon;
    [SerializeField] private LayerMask enemyLayer;

    private readonly List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    public IReadOnlyList<WeaponSlot> WeaponSlots => weaponSlots;

    private void Start()
    {
        if (defaultWeapon != null)
            EquipWeapon(defaultWeapon);
    }

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            WeaponSlot slot = weaponSlots[i];
            if (slot.weaponData == null) continue;
            slot.timer += Time.deltaTime;
            if (slot.timer >= slot.weaponData.weaponCooldown)
            {
                slot.timer = 0f;
                ExecuteAttack(slot);
            }
        }
    }

    public void EquipWeapon(ItemData weaponData)
    {
        WeaponSlot existing = weaponSlots.Find(s => s.weaponData == weaponData);
        if (existing != null)
        {
            return;
        }

        if (weaponSlots.Count >= 3) return;
        weaponSlots.Add(new WeaponSlot { weaponData = weaponData });
    }

    public void RemoveWeapon(ItemData weaponData)
    {
        weaponSlots.RemoveAll(s => s.weaponData == weaponData);
    }

    private void ExecuteAttack(WeaponSlot slot)
    {
        if (slot.weaponData.projectilePrefab != null)
        {
            FireRanged(slot);
        }
        else
        {
            PerformMelee(slot);
        }
    }

    private void PerformMelee(WeaponSlot slot)
    {
        if (animator != null)
        {
            // Reset trigger to ensure it doesn't queue up
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        // Damage is now handled in a Coroutine to sync with animation frames
        StartCoroutine(MeleeDamageCoroutine(slot));
    }

    private System.Collections.IEnumerator MeleeDamageCoroutine(WeaponSlot slot)
    {
        // Small delay so the damage hits when the sword is actually swinging (middle of animation)
        // TinySwords attack animations are roughly 0.3s long. 
        // 0.1s is usually the "swing" moment.
        yield return new WaitForSeconds(0.1f);

        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, slot.weaponData.weaponDetectionRadius, enemyLayer);

        if (nearby.Length == 0) yield break;

        Transform nearest = null;
        float shortest = Mathf.Infinity;
        foreach (Collider2D col in nearby)
        {
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < shortest) { shortest = d; nearest = col.transform; }
        }

        if (nearest != null)
        {
            EnemyStats stats = nearest.GetComponent<EnemyStats>();
            if (stats != null)
            {
                stats.TakeDamage(slot.weaponData.weaponDamage * slot.level);
            }
        }
    }

    private void FireRanged(WeaponSlot slot)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, slot.weaponData.weaponDetectionRadius, enemyLayer);

        if (nearby.Length == 0) return;

        Transform nearest = null;
        float shortest = Mathf.Infinity;
        foreach (Collider2D col in nearby)
        {
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < shortest) { shortest = d; nearest = col.transform; }
        }

        if (nearest == null) return;

        GameObject proj = Instantiate(slot.weaponData.projectilePrefab, transform.position, Quaternion.identity);
        Vector2 dir = (nearest.position - transform.position).normalized;

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDirection(dir);
            projScript.SetDamage(slot.weaponData.weaponDamage * slot.level);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var slot in weaponSlots)
        {
            if (slot.weaponData != null)
                Gizmos.DrawWireSphere(transform.position, slot.weaponData.weaponDetectionRadius);
        }
    }
}
