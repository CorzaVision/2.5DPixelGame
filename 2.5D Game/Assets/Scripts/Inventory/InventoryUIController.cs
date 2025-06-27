using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;


/// <summary>
/// Manages the inventory user interface, including item display, equipment slots, tooltips, and stat display.
/// This script handles all UI interactions for the player's inventory system.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerStats playerStats;

    // UI Elements
    private VisualElement inventoryPanel;
    private VisualElement inventoryGrid;
    private VisualElement equipmentGrid;
    private VisualElement statsDisplay;
    private Button closeButton;
    private VisualElement sortControls;
    private Button sortTypeButton;
    private Button sortRarityButton;
    private Button sortLevelButton;
    private Button sortNameButton;
    private Button sortValueButton;
    private Button autoStackButton;

    // Equipment Slots
    private VisualElement slotHead;
    private VisualElement slotChest;
    private VisualElement slotLegs;
    private VisualElement slotFeet;
    private VisualElement slotWeapon;
    private VisualElement slotOffhand;

    
    // Tooltip Elements
    private VisualElement itemTooltip;
    private Label tooltipName;
    private Label tooltipRarity;
    private Label tooltipDescription;
    private Label currencyInline;
    private VisualElement tooltipStatsList;
    
    // Data
    private PlayerInventory currentPlayerInventory;

    private VisualElement[] bagSlots = new VisualElement[5];


    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        InitializeUIElements();
        SetupEventHandlers();
        HideInventory();
        if (playerStats != null)
            playerStats.OnCurrencyChanged += OnCurrencyChanged;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnCurrencyChanged -= OnCurrencyChanged;
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes all UI element references from the UIDocument.
    /// </summary>
    private void InitializeUIElements()
    {
        if (uiDocument == null) return;
        
        var root = uiDocument.rootVisualElement;

        // Main UI elements
        inventoryPanel = root.Q<VisualElement>("inventory-panel");
        inventoryGrid = root.Q<VisualElement>("inventory-grid");
        equipmentGrid = root.Q<VisualElement>("equipment-grid");
        statsDisplay = root.Q<VisualElement>("stats-display");
        closeButton = root.Q<Button>("close-button");

        // Tooltip elements
        itemTooltip = root.Q<VisualElement>("item-tooltip");
        tooltipName = root.Q<Label>("tooltip-name");
        tooltipRarity = root.Q<Label>("tooltip-rarity");
        tooltipDescription = root.Q<Label>("tooltip-description");
        tooltipStatsList = root.Q<VisualElement>("tooltip-stats-list");
        currencyInline = root.Q<Label>("currency-inline");
        // Sort controls
        sortControls = root.Q<VisualElement>("sort-controls");
        sortTypeButton = root.Q<Button>("sort-type");
        sortRarityButton = root.Q<Button>("sort-rarity");
        sortLevelButton = root.Q<Button>("sort-level");
        sortNameButton = root.Q<Button>("sort-name");
        sortValueButton = root.Q<Button>("sort-value");
        autoStackButton = root.Q<Button>("auto-stack");

        // Equipment slots
        slotHead = root.Q<VisualElement>("slot-head");
        slotChest = root.Q<VisualElement>("slot-chest");
        slotLegs = root.Q<VisualElement>("slot-legs");
        slotFeet = root.Q<VisualElement>("slot-feet");
        slotWeapon = root.Q<VisualElement>("slot-weapon");
        slotOffhand = root.Q<VisualElement>("slot-offhand");

        // Bag slots
        for (int i = 0; i < bagSlots.Length; i++)
        {
            bagSlots[i] = root.Q<VisualElement>($"bag-slot-{i}");
        }

        if (currencyInline != null && playerStats != null)
        {
            currencyInline.text = $"Gold: {playerStats.GetCurrency(CurrencyType.Gold)} Silver: {playerStats.GetCurrency(CurrencyType.Silver)} Copper: {playerStats.GetCurrency(CurrencyType.Copper)}"; // Gold, Silver, Copper
        }
    }

    /// <summary>
    /// Sets up event handlers for UI interactions.
    /// </summary>
    private void SetupEventHandlers()
    {
        if (closeButton != null)
        {
            closeButton.clicked += Hide;
        }

        if (sortControls != null)
        {
            sortTypeButton.clicked += () => SortInventory(PlayerInventory.SortType.Type);
            sortRarityButton.clicked += () => SortInventory(PlayerInventory.SortType.Rarity);
            sortLevelButton.clicked += () => SortInventory(PlayerInventory.SortType.Level);
            sortNameButton.clicked += () => SortInventory(PlayerInventory.SortType.Name);
            sortValueButton.clicked += () => SortInventory(PlayerInventory.SortType.Value);
        }

        if (autoStackButton != null)
        {
            autoStackButton.clicked += AutoStackInventory;
        }
    }

    /// <summary>
    /// Hides the inventory panel initially.
    /// </summary>
    private void HideInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.style.display = DisplayStyle.None;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Shows the inventory UI with the specified player's inventory data.
    /// </summary>
    /// <param name="playerInventory">The player's inventory to display.</param>
    public void Show(PlayerInventory playerInventory)
    {
        currentPlayerInventory = playerInventory;
        inventoryPanel.style.display = DisplayStyle.Flex;
        RefreshAll();
    }

    /// <summary>
    /// Hides the inventory UI.
    /// </summary>
    public void Hide()
    {
        inventoryPanel.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Refreshes all UI elements with current data.
    /// </summary>
    public void RefreshAll()
    {
        if (currentPlayerInventory == null || playerStats == null) return;

        RefreshInventorySlots();
        RefreshEquipmentSlots();
        RefreshBagSlots();
        RefreshStats();
    }

    #endregion

    #region Inventory Display

    /// <summary>
    /// Refreshes the inventory grid with current items.
    /// </summary>
    private void RefreshInventorySlots()
    {
        inventoryGrid.Clear();
        var allSlots = currentPlayerInventory.GetAllSlots();
        foreach (var slot in allSlots)
        {
            var slotElement = CreateInventorySlot(slot);
            inventoryGrid.Add(slotElement);
        }
    }

    /// <summary>
    /// Creates a single inventory slot for an item.
    /// </summary>
    /// <param name="item">The item to create a slot for.</param>
    /// <returns>The created VisualElement slot.</returns>
    private VisualElement CreateInventorySlot(ItemInstance item)
    {
        var slot = new VisualElement();
        slot.AddToClassList("inventory-slot");
        
        if (item != null)
        {
            // Add event handlers
            slot.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
            slot.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
            slot.RegisterCallback<ClickEvent>(evt => OnItemClicked(item));
            
            // Add icon
            var icon = CreateItemIcon(item);
            slot.Add(icon);
            
            // Add stack count for stackable items
            if (item.itemData.isStackable && item.count > 1)
            {
                var countLabel = new Label(item.count.ToString());
                countLabel.AddToClassList("stack-count-label");
                slot.Add(countLabel);
            }
        }
        else
        {
            // Show empty slot (e.g., add a border, grey box, or "Empty" label)
            slot.AddToClassList("empty");
            var emptyLabel = new Label("Empty");
            emptyLabel.AddToClassList("empty-slot-label");
            slot.Add(emptyLabel);
        }
        
        return slot;
    }

    /// <summary>
    /// Creates an icon element for an item.
    /// </summary>
    /// <param name="item">The item to create an icon for.</param>
    /// <returns>The created icon VisualElement.</returns>
    private VisualElement CreateItemIcon(ItemInstance item)
    {
        var icon = new VisualElement();
        icon.AddToClassList("slot-icon");

        if (item.itemData.icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(item.itemData.icon as Texture2D);
        }

        return icon;
    }

    private void RefreshBagSlots()
    {
        if (currentPlayerInventory == null) return;
        for (int i = 0; i < bagSlots.Length; i++)
        {
            bagSlots[i].Clear();
            Bag bag = (i < currentPlayerInventory.bagSlots.Count) ? currentPlayerInventory.bagSlots[i] : null;
            if (bag != null)
            {
                // add Icon, tooltip etc to Bagslot

            }
            else
            {
                 // Empty bag slot leaving blank
            }
        }
    }

    private VisualElement CreateBagSlot(Bag bag, int slotIndex)
    {
        var bagSlot = new VisualElement();
        bagSlot.AddToClassList("bag-slot");
        
        if (bag != null && bag.bagData != null)
        {
            // Bag is equipped - show bag icon
            bagSlot.AddToClassList("equipped");
            
            var bagIcon = new VisualElement();
            bagIcon.AddToClassList("bag-icon");
            
            // DEBUG: Add these lines to see what's happening
            Debug.Log($"Bag Slot {slotIndex}: BagData = {bag.bagData.name}");
            Debug.Log($"Bag Slot {slotIndex}: bag.bagItem = {(bag.bagItem != null ? bag.bagItem.itemData.name : "NULL")}");
            
            // Use the stored bag item reference (NEW APPROACH)
            if (bag.bagItem != null && bag.bagItem.itemData.icon != null)
            {
                bagIcon.style.backgroundImage = new StyleBackground(bag.bagItem.itemData.icon as Texture2D);
                Debug.Log($"Bag Slot {slotIndex}: Icon SET successfully");
            }
            else
            {
                bagIcon.style.backgroundImage = null;
                Debug.Log($"Bag Slot {slotIndex}: Icon is NULL - bagItem={(bag.bagItem != null ? "EXISTS" : "NULL")}, icon={(bag.bagItem?.itemData.icon != null ? "EXISTS" : "NULL")}");
            }
            
            bagSlot.Add(bagIcon);
            
            // Keep tooltip functionality
            bagSlot.RegisterCallback<PointerEnterEvent>(evt => 
            {
                if (bag.bagItem != null)
                {
                    tooltipName.text = bag.bagItem.itemData.itemName;
                    tooltipRarity.text = bag.bagItem.itemData.itemRarity.ToString();
                    tooltipDescription.text = bag.bagItem.itemData.itemDescription;
                }
                else
                {
                    tooltipName.text = bag.bagData.bagName;
                    tooltipRarity.text = $"Slots: {bag.slots.Count}";
                    tooltipDescription.text = $"Bag Type: {bag.bagData.bagType}";
                }
                itemTooltip.style.display = DisplayStyle.Flex;
            });
            
            bagSlot.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
        }
        else
        {
            // Empty bag slot
            bagSlot.AddToClassList("empty");
            var emptyLabel = new Label("Empty");
            emptyLabel.AddToClassList("empty-slot-label");
            bagSlot.Add(emptyLabel);
        }
        
        return bagSlot;
    }

    #endregion

    #region Equipment Display

    /// <summary>
    /// Refreshes the equipment slots with current equipped items.
    /// </summary>
    private void RefreshEquipmentSlots()
    {
        equipmentGrid.Clear();

        if (playerStats.CurrentWeapon != null)
        {
            var slot = CreateEquipmentSlot(playerStats.CurrentWeapon);
            equipmentGrid.Add(slot);
        }
    }

    /// <summary>
    /// Creates an equipment slot for an equipped item.
    /// </summary>
    /// <param name="item">The equipped item.</param>
    /// <returns>The created equipment slot VisualElement.</returns>
    private VisualElement CreateEquipmentSlot(ItemInstance item)
    {
        var slot = new VisualElement();
        slot.AddToClassList("equipment-slot");

        // Add event handlers
        slot.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
        slot.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
        slot.RegisterCallback<PointerDownEvent>(evt => HandleEquipmentClick(evt));

        // Create icon container with rarity border
        var iconContainer = new VisualElement();
        iconContainer.AddToClassList("equipment-icon-container");
        iconContainer.AddToClassList($"rarity-border-{item.itemData.itemRarity.ToString().ToLower()}");
        
        var icon = CreateItemIcon(item);
        iconContainer.Add(icon);
        slot.Add(iconContainer);

        // Create name label with rarity color
        var nameLabel = new Label(item.itemData.itemName);
        nameLabel.AddToClassList("equipment-item-name");
        nameLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");
        slot.Add(nameLabel);

        return slot;
    }

    /// <summary>
    /// Handles double-click events on equipment slots for unequipping.
    /// </summary>
    /// <param name="evt">The pointer down event.</param>
    private void HandleEquipmentClick(PointerDownEvent evt)
    {
        if (evt.clickCount == 2)
        {
            playerStats.UnequipWeapon();
            RefreshAll();
        }
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
        if (item == null || item.itemData == null) return;

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
        itemTooltip.style.display = DisplayStyle.None;
    }

    #endregion

    #region Item Interactions

    /// <summary>
    /// Handles item click events for equipping weapons or using consumables.
    /// </summary>
    /// <param name="item">The clicked item.</param>
    private void OnItemClicked(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        switch (item.itemData.itemType)
        {
            case ItemType.Weapon:
                playerStats.EquipWeapon(item);
                Debug.Log($"Equipped {item.itemData.itemName}");
                break;
                
            case ItemType.Consumable:
                playerStats.UseConsumable(item);
                Debug.Log($"Used {item.itemData.itemName}");
                break;
                
            case ItemType.Currency:
                playerStats.UseItem(item);
                Debug.Log($"Collected {item.itemData.itemName}");
                break;

            case ItemType.Bag:
                if (item.itemData.isEquippable)
                {
                    currentPlayerInventory.EquipBagItem(item);
                    RefreshInventorySlots();
                    Debug.Log($"Equipped {item.itemData.itemName}");
                }
                break;
        }

        RefreshAll();
    }

    #endregion

    #region Stats Display

    /// <summary>
    /// Refreshes the stats display with current player statistics.
    /// </summary>
    private void RefreshStats()
    {
        statsDisplay.Clear();

        if (playerStats == null) 
        {
            Debug.LogError("PlayerStats is not assigned in the InventoryUIController!");
            return;
        }

        // Main stats
        statsDisplay.Add(CreateStatLabel($"Health: {playerStats.CurrentHealth}"));
        statsDisplay.Add(CreateStatLabel($"Attack: {playerStats.CurrentAttack}"));
        statsDisplay.Add(CreateStatLabel($"Defense: {playerStats.CurrentDefense}"));
        statsDisplay.Add(CreateStatSeparator());
        statsDisplay.Add(CreateStatLabel($"Level: {playerStats.CurrentLevel}"));
        statsDisplay.Add(CreateStatLabel($"EXP: {(int)playerStats.CurrentExperience} / {(int)playerStats.ExperienceToNextLevel}"));

        // Currency stats with color
        statsDisplay.Add(CreateStatSeparator());
        statsDisplay.Add(CreateColoredStatLabel($"Gold: {playerStats.GetCurrency(CurrencyType.Gold)} Silver: {playerStats.GetCurrency(CurrencyType.Silver)} Copper: {playerStats.GetCurrency(CurrencyType.Copper)}", new Color(1f, 0.84f, 0f))); // Gold, Silver, Copper
    }

    /// <summary>
    /// Creates a stat label with proper styling.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>The created Label element.</returns>
    private Label CreateStatLabel(string text)
    {
        var label = new Label(text);
        label.AddToClassList("stat-label");
        return label;
    }
    
    /// <summary>
    /// Creates a visual separator for the stats display.
    /// </summary>
    /// <returns>The created separator VisualElement.</returns>
    private VisualElement CreateStatSeparator()
    {
        var separator = new VisualElement();
        separator.style.height = 1;
        separator.style.backgroundColor = new StyleColor(new Color(0.4f, 0.4f, 0.4f, 0.4f));
        separator.style.marginTop = 5;
        separator.style.marginBottom = 5;

        return separator;
    }

    private Label CreateColoredStatLabel(string text, Color color)
    {
        var label = new Label(text);
        label.AddToClassList("stat-label");
        label.style.color = new StyleColor(color);
        return label;
    }

    private void OnCurrencyChanged(CurrencyType type, int amount)
    {
        RefreshStats();
    }

    #endregion

    #region Sorting

    private void SortInventory(PlayerInventory.SortType sortType)
    {
        if (currentPlayerInventory != null)
        {
            currentPlayerInventory.SortInventory(sortType);
            RefreshInventorySlots(); // Refresh the UI after sorting
            Debug.Log($"Inventory sorted by: {sortType}");
        }
    }

    private void AutoStackInventory()
    {
        if (currentPlayerInventory != null)
        {
            currentPlayerInventory.AutoStackItems(); // Correct method name
            RefreshInventorySlots(); // Refresh the UI after auto-stacking
            Debug.Log("Inventory auto-stacked");
        }
    }

    #endregion

}