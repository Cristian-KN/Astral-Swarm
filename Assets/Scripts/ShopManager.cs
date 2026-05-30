using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Configuración de Tienda")]
    public List<ItemData> availableItems;
    public int baseSlots = 3;
    private int extraSlots = 0;
    
    [Header("Referencias")]
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OpenShop()
    {
        int totalSlots = Mathf.Min(6, baseSlots + extraSlots);
        Debug.Log("Abriendo tienda con " + totalSlots + " slots.");
        // Aquí iría la lógica para instanciar botones de compra en la UI
    }

    public void BuyItem(ItemData item)
    {
        int price = CalculatePrice(item);
        
        if (gameManager.currentGold >= price)
        {
            gameManager.currentGold -= price;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                InventoryManager inv = player.GetComponent<InventoryManager>();
                if (inv != null) inv.AddItem(item);
            }
            
            // Si el objeto es el de expandir tienda
            if (item.itemName == "Expansión de Tienda")
            {
                extraSlots = Mathf.Min(3, extraSlots + 1);
            }
        }
    }

    private int CalculatePrice(ItemData item)
    {
        // Precio = Rareza * Nivel * Factor
        float rarityMult = GetRarityMultiplier(item.rarity);
        return Mathf.RoundToInt(rarityMult * gameManager.GetCurrentLevel());
    }

    private float GetRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 2.5f;
            case ItemRarity.Rare: return 6.25f;
            case ItemRarity.Epic: return 12.5f;
            case ItemRarity.Legendary: return 25f;
            case ItemRarity.Mythic: return 150f;
            default: return 2.5f;
        }
    }
}
