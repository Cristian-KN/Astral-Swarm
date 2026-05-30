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

    [Header("Armas por Clase")]
    [SerializeField] private ItemData swordWeapon;
    [SerializeField] private ItemData bowWeapon;
    [SerializeField] private ItemData spearWeapon;
    [SerializeField] private LayerMask enemyLayer;

    private readonly List<WeaponSlot> weaponSlots = new List<WeaponSlot>();

    private void Start()
    {
        string selectedClass = PlayerPrefs.GetString("SelectedClass", "warrior").ToLower();
        ItemData weaponToEquip = null;
        
        switch (selectedClass)
        {
            case "warrior": weaponToEquip = swordWeapon; break;
            case "archer": weaponToEquip = bowWeapon; break;
            case "lancer": weaponToEquip = spearWeapon; break;
        }

        if (weaponToEquip != null)
            EquipWeapon(weaponToEquip);
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
            StartCoroutine(RangedAttackCoroutine(slot));
        }
        else
        {
            PerformMelee(slot);
        }
    }

    private void PerformMelee(WeaponSlot slot)
    {
        Debug.Log($"[PlayerAttack] Performing Melee attack with {slot.weaponData.itemName}");
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        StartCoroutine(MeleeDamageCoroutine(slot));
    }

    private System.Collections.IEnumerator MeleeDamageCoroutine(WeaponSlot slot)
    {
        // For Lancer (Spear), the hit should be slightly delayed to match the thrust
        // TinySwords melee animations usually hit around 0.2s - 0.3s
        yield return new WaitForSeconds(0.2f);

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

    private System.Collections.IEnumerator RangedAttackCoroutine(WeaponSlot slot)
    {
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

        if (nearest == null) yield break;

        // Trigger animation
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        // Wait for the specific release frame (Archer_Shoot frame 3/4 is usually the release)
        // TinySwords animations are roughly 0.1s per frame.
        yield return new WaitForSeconds(0.3f);

        // Re-check nearest target in case it moved or died
        nearby = Physics2D.OverlapCircleAll(
            transform.position, slot.weaponData.weaponDetectionRadius, enemyLayer);
        
        nearest = null;
        shortest = Mathf.Infinity;
        foreach (Collider2D col in nearby)
        {
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < shortest) { shortest = d; nearest = col.transform; }
        }

        if (nearest == null) yield break;

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
