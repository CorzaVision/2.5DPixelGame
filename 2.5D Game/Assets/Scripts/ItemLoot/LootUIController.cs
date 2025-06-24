using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Manages the loot user interface, displaying items from loot bags and handling item collection.
/// This script provides tooltips, individual item looting, and bulk looting functionality.
/// </summary>
public class LootUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;

    // UI Elements
    private VisualElement lootPanel;
    private ScrollView itemList;
    private Button lootAllButton;
    private Button closeButton;

    // Tooltip Elements
    private VisualElement itemTooltip;
    private Label tooltipName;
    private Label tooltipRarity;
    private Label tooltipDescription;
    private VisualElement tooltipStatsList;

    // Data
    private Bag currentBag;
    private PlayerInventory currentPlayerInventory;
    private InteractableBag currentBagScript;
    private PlayerStats currentPlayerStats;
    #region Unity Lifecycle

    private void Awake()
    {
        InitializeUIElements();
        SetupEventHandlers();
        HideLootPanel();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes all UI element references from the UIDocument.
    /// </summary>
    private void InitializeUIElements()
    {
        var root = uiDocument.rootVisualElement;
        
        // Main UI elements
        lootPanel = root.Q<VisualElement>("loot-panel");
        itemList = root.Q<ScrollView>("item-list");
        lootAllButton = root.Q<Button>("loot-all-button");
        closeButton = root.Q<Button>("close-button");

        // Tooltip elements
        itemTooltip = root.Q<VisualElement>("item-tooltip");
        tooltipName = root.Q<Label>("tooltip-name");
        tooltipRarity = root.Q<Label>("tooltip-rarity");
        tooltipDescription = root.Q<Label>("tooltip-description");
        tooltipStatsList = root.Q<VisualElement>("tooltip-stats-list");
    }

    /// <summary>
    /// Sets up event handlers for UI interactions.
    /// </summary>
    private void SetupEventHandlers()
    {
        if (lootAllButton != null) 
        {
            lootAllButton.clicked += LootAll;
        }
        
        if (closeButton != null) 
        {
            closeButton.clicked += HideLoot;
        }
    }

    /// <summary>
    /// Hides the loot panel initially.
    /// </summary>
    private void HideLootPanel()
    {
        if (lootPanel != null)
        {
            lootPanel.style.display = DisplayStyle.None;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Shows the loot UI with items from the specified bag.
    /// </summary>
    /// <param name="bag">The bag containing loot items.</param>
    /// <param name="playerInventory">The player's inventory to add items to.</param>
    /// <param name="playerStats">The player's stats to update.</param>
    /// <param name="bagScript">The interactable bag script for cleanup.</param>
    public void ShowLoot(Bag bag, PlayerInventory playerInventory, PlayerStats playerStats, InteractableBag bagScript)
    {
        if (lootPanel == null)
        {
            Debug.LogError("Loot Panel is null! Cannot show loot UI.");
            return;
        }
        
        currentBag = bag;
        currentPlayerInventory = playerInventory;
        currentPlayerStats = playerStats;
        currentBagScript = bagScript;
        lootPanel.style.display = DisplayStyle.Flex;
        RefreshLootList();
    }

    /// <summary>
    /// Hides the loot UI and clears current data.
    /// </summary>
    public void HideLoot()
    {
        if (lootPanel != null)
        {
            lootPanel.style.display = DisplayStyle.None;
        }
        
        currentBag = null;
        currentPlayerInventory = null;
        currentBagScript = null;
    }

    #endregion

    #region Loot Display

    /// <summary>
    /// Refreshes the loot list with current bag items.
    /// </summary>
    private void RefreshLootList()
    {
        if (itemList == null || currentBag == null) return;
        
        itemList.Clear();

        foreach (var item in currentBag.items)
        {
            var itemElement = CreateLootItemElement(item);
            itemList.Add(itemElement);
        }
    }

    /// <summary>
    /// Creates a single loot item element with icon, name, and count.
    /// </summary>
    /// <param name="item">The item to create an element for.</param>
    /// <returns>The created VisualElement.</returns>
    private VisualElement CreateLootItemElement(ItemInstance item)
    {
        var itemElement = new VisualElement();
        itemElement.AddToClassList("loot-item");
        itemElement.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");

        // Add icon
        var icon = CreateItemIcon(item);
        itemElement.Add(icon);

        // Build display name with value/effect
        string displayName = item.itemData.itemName;

        // Show value for currency
        if (item.itemData.itemType == ItemType.Currency && item.itemData.currencyValue > 0)
        {
            displayName += $" (+{item.itemData.currencyValue})";
        }
        // Show effect for consumables (e.g., health restore)
        else if (item.itemData.itemType == ItemType.Consumable)
        {
            if (item.itemData.healthRestore > 0)
                displayName += $" (+{item.itemData.healthRestore} HP)";
            else if (item.itemData.manaRestore > 0)
                displayName += $" (+{item.itemData.manaRestore} MP)";
            // Add more as needed for other effects
        }

        // Add name label with rarity color
        var nameLabel = new Label(displayName);
        nameLabel.AddToClassList("item-name");
        nameLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");
        itemElement.Add(nameLabel);

        // Add count label
        var countLabel = new Label($"x{item.count}");
        countLabel.AddToClassList("item-count");
        countLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");
        itemElement.Add(countLabel);

        if (item.itemData.itemType == ItemType.Currency)
        {
            itemElement.AddToClassList("currency-item");
        }

        // Add event handlers
        itemElement.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
        itemElement.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
        itemElement.RegisterCallback<ClickEvent>(evt => LootItem(item));

        return itemElement;
    }

    /// <summary>
    /// Creates an icon element for an item.
    /// </summary>
    /// <param name="item">The item to create an icon for.</param>
    /// <returns>The created icon VisualElement.</returns>
    private VisualElement CreateItemIcon(ItemInstance item)
    {
        var icon = new VisualElement();
        icon.AddToClassList("loot-item-icon");
        
        if (item.itemData.icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(item.itemData.icon as Texture2D);
        }
        
        return icon;
    }

    #endregion

    #region Loot Actions

    /// <summary>
    /// Loots a single item from the bag.
    /// </summary>
    /// <param name="item">The item to loot.</param>
    private void LootItem(ItemInstance item)
    {
        Debug.Log($"Looting item: {item.itemData.itemName}, type: {item.itemData.itemType}");
        if (item.itemData.itemType == ItemType.Currency)
        {
            Debug.Log("Currency item detected, attempting to add currency.");
            int currencyAmount = item.itemData.currencyValue > 0
                ? item.itemData.currencyValue
                : UnityEngine.Random.Range(item.itemData.currencyMinValue, item.itemData.currencyMaxValue + 1);
            int totalCurrency = currencyAmount * item.count;
            if (currentPlayerStats != null)
            {
                Debug.Log("PlayerStats reference is valid, calling AddCurrency.");
                currentPlayerStats.AddCurrency(item.itemData.currencyType, totalCurrency);
            }
            else
            {
                Debug.LogError("PlayerStats reference is missing in LootUIController!");
            }

            // Optionally: Play a sound or show a popup here
            Debug.Log($"Collected {totalCurrency} {item.itemData.currencyType}");

            // Remove the currency item from the loot bag (do not add to inventory)
            currentBag.items.Remove(item);
        }
        else
        {
            // For all other items, add to inventory as usual
            currentPlayerInventory.AddItemInstance(item);
            currentBag.items.Remove(item);
        }

        if (currentBag.items.Count == 0)
        {
            if (currentBagScript != null) 
            {
                currentBagScript.OnAllLooted();
            }
            HideLoot();
        }
        else
        {
            RefreshLootList();
        }
    }

    /// <summary>
    /// Loots all items from the bag at once.
    /// </summary>
    private void LootAll()
    {
        foreach (var item in new List<ItemInstance>(currentBag.items))
        {
            if (item.itemData.itemType == ItemType.Currency)
            {
                int currencyAmount = item.itemData.currencyValue > 0
                    ? item.itemData.currencyValue
                    : UnityEngine.Random.Range(item.itemData.currencyMinValue, item.itemData.currencyMaxValue + 1);
                int totalCurrency = currencyAmount * item.count;
                if (currentPlayerStats != null)
                {
                    currentPlayerStats.AddCurrency(item.itemData.currencyType, totalCurrency);
                }
                else
                {
                    Debug.LogError("PlayerStats reference is missing in LootUIController!");
                }
                Debug.Log($"Collected {totalCurrency} {item.itemData.currencyType}");
            }
            else
            {
                currentPlayerInventory.AddItemInstance(item);
            }
        }
        currentBag.items.Clear();

        if (currentBagScript != null) 
        {
            currentBagScript.OnAllLooted();
        }
        HideLoot();
    }

    #endregion

    #region Tooltip System

    /// <summary>
    /// Shows the item tooltip with detailed information.
    /// </summary>
    /// <param name="evt">The pointer enter event.</param>
    /// <param name="item">The item to show tooltip for.</param>
    private void ShowTooltip(PointerEnterEvent evt, ItemInstance item)
    {
        if (item == null || item.itemData == null || itemTooltip == null) return;

        // Position tooltip
        itemTooltip.style.left = evt.position.x + 15;
        itemTooltip.style.top = evt.position.y;

        // Set basic information
        SetTooltipBasicInfo(item.itemData);
        
        // Set detailed stats
        SetTooltipStats(item.itemData);

        itemTooltip.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Sets the basic information in the tooltip (name, rarity, description).
    /// </summary>
    /// <param name="data">The item data.</param>
    private void SetTooltipBasicInfo(ItemData data)
    {
        // Set name with rarity color
        tooltipName.text = data.itemName;
        tooltipName.ClearClassList();
        tooltipName.AddToClassList("tooltip-title");
        tooltipName.AddToClassList($"rarity-{data.itemRarity.ToString().ToLower()}");
        
        // Set rarity
        tooltipRarity.text = data.itemRarity.ToString();
        tooltipRarity.ClearClassList();
        tooltipRarity.AddToClassList("tooltip-rarity");
        tooltipRarity.AddToClassList($"rarity-{data.itemRarity.ToString().ToLower()}");

        // Set description
        tooltipDescription.text = data.itemDescription;
    }

    /// <summary>
    /// Sets the detailed stats in the tooltip based on item type.
    /// </summary>
    /// <param name="data">The item data.</param>
    private void SetTooltipStats(ItemData data)
    {
        tooltipStatsList.Clear();

        if (data.itemType == ItemType.Weapon)
        {
            AddWeaponStats(data);
        }
        else if (data.itemType == ItemType.Consumable)
        {
            AddConsumableStats(data);
        }
        else if (data.itemType == ItemType.Currency)
        {
            AddCurrencyStats(data);
        }

        // Add item level if applicable
        if (data.itemLevel > 0)
        {
            var levelLabel = new Label($"Item Level: {data.itemLevel}");
            levelLabel.AddToClassList("tooltip-stat-label");
            tooltipStatsList.Add(levelLabel);
        }
    }

    /// <summary>
    /// Adds weapon-specific stats to the tooltip.
    /// </summary>
    /// <param name="data">The weapon item data.</param>
    private void AddWeaponStats(ItemData data)
    {
        var damageLabel = new Label($"Damage: {data.damage}");
        damageLabel.AddToClassList("tooltip-stat-label");
        tooltipStatsList.Add(damageLabel);

        if (data.weaponCritChance > 0)
        {
            var critLabel = new Label($"Crit Chance: {data.weaponCritChance}%");
            critLabel.AddToClassList("tooltip-stat-label");
            tooltipStatsList.Add(critLabel);
        }

        var handLabel = new Label($"Hand: {data.weaponHand}");
        handLabel.AddToClassList("tooltip-stat-label");
        tooltipStatsList.Add(handLabel);

        var weightLabel = new Label($"Weight: {data.weaponWeight}");
        weightLabel.AddToClassList("tooltip-stat-label");
        tooltipStatsList.Add(weightLabel);
    }

    /// <summary>
    /// Adds consumable-specific stats to the tooltip.
    /// </summary>
    /// <param name="data">The consumable item data.</param>
    private void AddConsumableStats(ItemData data)
    {
        var healLabel = new Label($"Heals: {data.healthRestore}");
        healLabel.AddToClassList("tooltip-stat-label");
        tooltipStatsList.Add(healLabel);
    }

    /// <summary>
    /// Adds currency-specific stats to the tooltip.
    /// </summary>
    /// <param name="data">The currency item data.</param>
    private void AddCurrencyStats(ItemData data)
    {
        var currencyLabel = new Label($"Currency: {data.currencyType}");
        currencyLabel.AddToClassList("tooltip-stat-label");
        tooltipStatsList.Add(currencyLabel);
    }

    /// <summary>
    /// Hides the item tooltip.
    /// </summary>
    private void HideTooltip()
    {
        if (itemTooltip != null)
        {
            itemTooltip.style.display = DisplayStyle.None;
        }
    }

    #endregion
}
