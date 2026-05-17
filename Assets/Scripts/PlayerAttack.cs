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

    private void Start()
    {
        if (defaultWeapon != null)
            EquipWeapon(defaultWeapon);
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
                FireWeapon(slot);
            }
        }
    }

    /// <summary>
    /// Equipa un arma nueva o sube de nivel si ya la tenemos. Máximo 3 slots.
    /// </summary>
    public void EquipWeapon(ItemData weaponData)
    {
        WeaponSlot existing = weaponSlots.Find(s => s.weaponData == weaponData);
        if (existing != null)
        {
            existing.level = Mathf.Min(existing.level + 1, weaponData.weaponMaxLevel);
            return;
        }

        if (weaponSlots.Count >= 3) return;
        weaponSlots.Add(new WeaponSlot { weaponData = weaponData });
    }

    private void FireWeapon(WeaponSlot slot)
    {
        if (slot.weaponData.projectilePrefab == null) return;

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
