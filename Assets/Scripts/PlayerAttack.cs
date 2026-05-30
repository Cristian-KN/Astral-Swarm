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
    public IReadOnlyList<WeaponSlot> WeaponSlots => weaponSlots;

    private Animator animator;

    private PlayerStats playerStats;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
    }

    // ---- Stats del jugador aplicadas al combate (pasivas) ----

    /// <summary>Multiplicador de daño según attackPower relativo a su base (PowerCrystal, etc.).</summary>
    private float DamageMultiplier()
    {
        if (playerStats == null) return 1f;
        float baseAtk = playerStats.baseAttackPower > 0f ? playerStats.baseAttackPower : 10f;
        return (playerStats.attackPower / baseAtk) * Mathf.Max(1f, playerStats.attackMultiplier);
    }

    /// <summary>Radio de detección escalado por attackRange relativo a su base.</summary>
    private float ScaledRadius(float baseRadius)
    {
        if (playerStats == null) return baseRadius;
        float baseRange = playerStats.baseAttackRange > 0f ? playerStats.baseAttackRange : 5f;
        return baseRadius * (playerStats.attackRange / baseRange);
    }

    /// <summary>Daño final de un golpe del arma, ya escalado por las stats del jugador.</summary>
    private int ComputeDamage(WeaponSlot slot)
        => Mathf.RoundToInt(slot.weaponData.weaponDamage * slot.level * DamageMultiplier());

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

    private void Update()
    {
        float speedMod = (playerStats != null) ? playerStats.attackSpeed : 1f;

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            WeaponSlot slot = weaponSlots[i];
            if (slot.weaponData == null) continue;
            
            slot.timer += Time.deltaTime;

            // Attack speed formula: lower cooldown as attackSpeed increases
            float effectiveCooldown = slot.weaponData.weaponCooldown / speedMod;

            if (slot.timer >= effectiveCooldown)
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
            // Level up existing weapon instead of ignoring
            existing.level++;
            Debug.Log($"[PlayerAttack] Weapon {weaponData.itemName} leveled up to {existing.level}");
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
        // Play Sound
        if (AudioManager.Instance != null)
        {
            if (slot.weaponData == swordWeapon) AudioManager.Instance.PlaySwordSound();
            else if (slot.weaponData == bowWeapon) AudioManager.Instance.PlayBowSound();
            else if (slot.weaponData == spearWeapon) AudioManager.Instance.PlaySpearSound();
        }

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
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        StartCoroutine(MeleeDamageCoroutine(slot));
    }

    private System.Collections.IEnumerator MeleeDamageCoroutine(WeaponSlot slot)
    {
        float speedMod = (playerStats != null) ? playerStats.attackSpeed : 1f;
        // Base delay is 0.2s for Warrior/Lancer, scaled by attack speed
        yield return new WaitForSeconds(0.2f / speedMod);

        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, ScaledRadius(slot.weaponData.weaponDetectionRadius), enemyLayer);

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
                stats.TakeDamage(ComputeDamage(slot));
            }
        }
    }

    private System.Collections.IEnumerator RangedAttackCoroutine(WeaponSlot slot)
    {
        float radius = ScaledRadius(slot.weaponData.weaponDetectionRadius);
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, radius, enemyLayer);

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

        float speedMod = (playerStats != null) ? playerStats.attackSpeed : 1f;
        // Wait for the release frame (base 0.3s), scaled by attack speed
        yield return new WaitForSeconds(0.3f / speedMod);

        // Re-check nearest target
        nearby = Physics2D.OverlapCircleAll(
            transform.position, radius, enemyLayer);

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
            projScript.SetDamage(ComputeDamage(slot));
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
