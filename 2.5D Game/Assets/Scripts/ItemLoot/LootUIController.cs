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
    /// <param name="bagScript">The interactable bag script for cleanup.</param>
    public void ShowLoot(Bag bag, PlayerInventory playerInventory, InteractableBag bagScript)
    {
        if (lootPanel == null)
        {
            Debug.LogError("Loot Panel is null! Cannot show loot UI.");
            return;
        }
        
        currentBag = bag;
        currentPlayerInventory = playerInventory;
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

        // Add name label with rarity color
        var nameLabel = new Label(item.itemData.itemName);
        nameLabel.AddToClassList("item-name");
        nameLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");
        itemElement.Add(nameLabel);

        // Add count label
        var countLabel = new Label($"x{item.count}");
        countLabel.AddToClassList("item-count");
        itemElement.Add(countLabel);

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
        currentPlayerInventory.AddItemInstance(item);
        currentBag.items.Remove(item);
        
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
            currentPlayerInventory.AddItemInstance(item);
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
