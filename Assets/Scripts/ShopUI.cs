using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject shopPanel;
    public Transform itemGrid;

    private ShopManager shopManager;
    private GameManager gameManager;

    private void Awake()
    {
        shopManager = FindObjectOfType<ShopManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        PopulateItems();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void PopulateItems()
    {
        if (itemGrid == null) return;
        foreach (Transform child in itemGrid) Destroy(child.gameObject);

        List<ItemData> items = shopManager.availableItems;
        int totalSlots = Mathf.Min(6, shopManager.baseSlots);

        for (int i = 0; i < totalSlots && i < items.Count; i++)
            CreateItemButton(items[i]);
    }

    private void CreateItemButton(ItemData item)
    {
        GameObject btnGO = new GameObject(item.itemName, typeof(RectTransform));
        btnGO.transform.SetParent(itemGrid, false);

        Image bg = btnGO.AddComponent<Image>();
        bg.color = new Color(0.176f, 0.106f, 0.306f, 1f);
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;

        Outline border = btnGO.AddComponent<Outline>();
        int price = CalculatePrice(item);
        bool canAfford = gameManager.currentGold >= price;
        border.effectColor = canAfford ? GetRarityColor(item.rarity) : new Color(0.957f, 0.263f, 0.212f, 1f);
        border.effectDistance = new Vector2(2f, -2f);

        if (!canAfford) btn.interactable = false;

        if (item.icon != null)
        {
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(btnGO.transform, false);
            Image icon = iconGO.AddComponent<Image>();
            icon.sprite = item.icon;
            icon.preserveAspect = true;
            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.65f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.offsetMin = new Vector2(-24f, -4f);
            iconRT.offsetMax = new Vector2(24f, -4f);
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject nameGO = new GameObject("Name", typeof(RectTransform));
        nameGO.transform.SetParent(btnGO.transform, false);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = item.itemName;
        nameText.fontSize = 13;
        nameText.color = new Color(1f, 0.843f, 0f, 1f);
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.font = font;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.35f);
        nameRT.anchorMax = new Vector2(1f, 0.65f);
        nameRT.offsetMin = new Vector2(4f, 0f);
        nameRT.offsetMax = new Vector2(-4f, 0f);

        GameObject priceGO = new GameObject("Price", typeof(RectTransform));
        priceGO.transform.SetParent(btnGO.transform, false);
        Text priceText = priceGO.AddComponent<Text>();
        priceText.text = "💰 " + price;
        priceText.fontSize = 12;
        priceText.color = canAfford ? new Color(1f, 0.757f, 0.027f, 1f) : new Color(0.957f, 0.263f, 0.212f, 1f);
        priceText.alignment = TextAnchor.MiddleCenter;
        priceText.font = font;
        RectTransform priceRT = priceGO.GetComponent<RectTransform>();
        priceRT.anchorMin = new Vector2(0f, 0f);
        priceRT.anchorMax = new Vector2(1f, 0.35f);
        priceRT.offsetMin = Vector2.zero;
        priceRT.offsetMax = Vector2.zero;

        ItemData captured = item;
        btn.onClick.AddListener(() => { shopManager.BuyItem(captured); CloseShop(); });
    }

    private int CalculatePrice(ItemData item)
    {
        float mult;
        switch (item.rarity)
        {
            case ItemRarity.Common:    mult = 2.5f;   break;
            case ItemRarity.Rare:      mult = 6.25f;  break;
            case ItemRarity.Epic:      mult = 12.5f;  break;
            case ItemRarity.Legendary: mult = 25f;    break;
            case ItemRarity.Mythic:    mult = 150f;   break;
            default:                   mult = 2.5f;   break;
        }
        return Mathf.RoundToInt(mult * gameManager.GetCurrentLevel() * 10f);
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:    return new Color(0.620f, 0.620f, 0.620f, 1f);
            case ItemRarity.Rare:      return new Color(0.129f, 0.588f, 0.953f, 1f);
            case ItemRarity.Epic:      return new Color(0.612f, 0.153f, 0.690f, 1f);
            case ItemRarity.Legendary: return new Color(1f,     0.596f, 0f,     1f);
            case ItemRarity.Mythic:    return new Color(0.957f, 0.263f, 0.212f, 1f);
            default:                   return Color.white;
        }
    }
}
