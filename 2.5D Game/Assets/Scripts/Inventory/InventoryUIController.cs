using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerStats playerStats; // Assign this in the Inspector

    private VisualElement inventoryPanel;
    private VisualElement inventoryGrid;
    private VisualElement equipmentGrid;
    private VisualElement statsDisplay;
    private Button closeButton;
    
    // --- Add references for the tooltip ---
    private VisualElement itemTooltip;
    private Label tooltipName;
    private Label tooltipRarity;
    private Label tooltipDescription;
    private VisualElement tooltipStatsList;
    
    private PlayerInventory currentPlayerInventory;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        // Query all the new elements
        inventoryPanel = root.Q<VisualElement>("inventory-panel");
        inventoryGrid = root.Q<VisualElement>("inventory-grid");
        equipmentGrid = root.Q<VisualElement>("equipment-grid");
        statsDisplay = root.Q<VisualElement>("stats-display");
        closeButton = root.Q<Button>("close-button");

        // --- Query the tooltip elements ---
        itemTooltip = root.Q<VisualElement>("item-tooltip");
        tooltipName = root.Q<Label>("tooltip-name");
        tooltipRarity = root.Q<Label>("tooltip-rarity");
        tooltipDescription = root.Q<Label>("tooltip-description");
        tooltipStatsList = root.Q<VisualElement>("tooltip-stats-list");
        
        closeButton.clicked += Hide;
        
        // Start hidden
        inventoryPanel.style.display = DisplayStyle.None;
    }

    public void Show(PlayerInventory playerInventory)
    {
        currentPlayerInventory = playerInventory;
        inventoryPanel.style.display = DisplayStyle.Flex;
        RefreshAll();
    }

    public void Hide()
    {
        inventoryPanel.style.display = DisplayStyle.None;
    }

    public void RefreshAll()
    {
        if (currentPlayerInventory == null || playerStats == null) return;

        RefreshInventorySlots();
        RefreshEquipmentSlots();
        RefreshStats();
    }
    
    private void RefreshInventorySlots()
    {
        inventoryGrid.Clear();
        foreach (var item in currentPlayerInventory.items)
        {
            var slot = new VisualElement();
            slot.AddToClassList("inventory-slot");
            
            slot.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
            slot.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());
            
            slot.RegisterCallback<ClickEvent>(evt => OnItemClicked(item));
            
            var icon = new VisualElement();
            icon.AddToClassList("slot-icon");

            if (item.itemData.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(item.itemData.icon as Texture2D);
            }

            slot.Add(icon);
            
            // Add stack count for stackable items
            if (item.itemData.isStackable && item.count > 1)
            {
                var countLabel = new Label(item.count.ToString());
                countLabel.AddToClassList("stack-count-label"); // Use a specific class for styling
                slot.Add(countLabel);
            }
            
            inventoryGrid.Add(slot);
        }
    }

    private void RefreshEquipmentSlots()
    {
        equipmentGrid.Clear();

        if (playerStats.CurrentWeapon != null)
        {
            var item = playerStats.CurrentWeapon;

            var slot = new VisualElement();
            slot.AddToClassList("equipment-slot"); // The main container for the row

            slot.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
            slot.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());

            // --- Create a container for the icon to apply a border ---
            var iconContainer = new VisualElement();
            iconContainer.AddToClassList("equipment-icon-container");
            iconContainer.AddToClassList($"rarity-border-{item.itemData.itemRarity.ToString().ToLower()}"); // Rarity border class
            
            var icon = new VisualElement();
            icon.AddToClassList("slot-icon");

            if (item.itemData.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(item.itemData.icon as Texture2D);
            }

            iconContainer.Add(icon);
            slot.Add(iconContainer);

            // --- Create the name label ---
            var nameLabel = new Label(item.itemData.itemName);
            nameLabel.AddToClassList("equipment-item-name");
            nameLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}"); // Rarity color for text
            slot.Add(nameLabel);

            // --- Handle Unequipping ---
            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    playerStats.UnequipWeapon();
                    RefreshAll();
                }
            });
            
            equipmentGrid.Add(slot);
        }
    }

    // --- Add new methods for showing and hiding the tooltip ---

    private void ShowTooltip(PointerEnterEvent evt, ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        itemTooltip.style.left = evt.position.x + 15;
        itemTooltip.style.top = evt.position.y;

        var data = item.itemData;
        tooltipName.text = data.itemName;
        tooltipName.ClearClassList();
        tooltipName.AddToClassList("tooltip-title");
        tooltipName.AddToClassList($"rarity-{data.itemRarity.ToString().ToLower()}");

        tooltipRarity.text = data.itemRarity.ToString();
        tooltipRarity.ClearClassList();
        tooltipRarity.AddToClassList("tooltip-rarity");
        tooltipRarity.AddToClassList($"rarity-{data.itemRarity.ToString().ToLower()}");
        
        tooltipDescription.text = data.itemDescription;

        tooltipStatsList.Clear();
        
        if (data.itemType == ItemType.Weapon)
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
        else if (data.itemType == ItemType.Consumable)
        {
            var healLabel = new Label($"Heals: {data.healthRestore}");
            healLabel.AddToClassList("tooltip-stat-label");
            tooltipStatsList.Add(healLabel);
        }
        
        if (data.itemLevel > 0)
        {
            var levelLabel = new Label($"Item Level: {data.itemLevel}");
            levelLabel.AddToClassList("tooltip-stat-label");
            tooltipStatsList.Add(levelLabel);
        }

        itemTooltip.style.display = DisplayStyle.Flex;
    }

    private void HideTooltip()
    {
        itemTooltip.style.display = DisplayStyle.None;
    }

    private void OnItemClicked(ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        // Check if the item is a weapon
        if (item.itemData.itemType == ItemType.Weapon)
        {
            // If it's a weapon, equip it.
            playerStats.EquipWeapon(item);
            Debug.Log($"Equipped {item.itemData.itemName}");
        }
        // Check if the item is a consumable
        else if (item.itemData.itemType == ItemType.Consumable)
        {
            // If it's a consumable, use it.
            playerStats.UseConsumable(item);
            Debug.Log($"Used {item.itemData.itemName}");
        }

        // Refresh the entire UI to show stat changes and consumed items.
        RefreshAll();
    }

    private void RefreshStats()
    {
        statsDisplay.Clear();

        if (playerStats == null) 
        {
            Debug.LogError("PlayerStats is not assigned in the InventoryUIController!");
                return;
            }

        // Create and add labels for each stat
        statsDisplay.Add(CreateStatLabel($"Health: {playerStats.CurrentHealth}"));
        statsDisplay.Add(CreateStatLabel($"Attack: {playerStats.CurrentAttack}"));
        statsDisplay.Add(CreateStatLabel($"Defense: {playerStats.CurrentDefense}"));
        statsDisplay.Add(CreateStatSperator());
        statsDisplay.Add(CreateStatLabel($"Level: {playerStats.CurrentLevel}"));
        statsDisplay.Add(CreateStatLabel($"EXP: {(int)playerStats.CurrentExperience} / {(int)playerStats.ExperienceToNextLevel}"));
    }

    private Label CreateStatLabel(string text)
    {
        var label = new Label(text);
        label.AddToClassList("stat-label");
        return label;
    }
    
    private VisualElement CreateStatSperator()
    {
        var seperator = new VisualElement();
        seperator.style.height = 1;
        seperator.style.backgroundColor = new StyleColor(new Color(0.4f,0.4f,0.4f,0.4f));
        seperator.style.marginTop = 5;
        seperator.style.marginBottom = 5;

        return seperator;
    }
}
