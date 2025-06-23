using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// NO LONGER A SINGLETON!
public class LootUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement lootPanel;
    private ScrollView itemList;
    private Button lootAllButton;
    private Button closeButton;

    // --- Add references for the tooltip ---
    private VisualElement itemTooltip;
    private Label tooltipName;
    private Label tooltipRarity;
    private Label tooltipDescription;
    private VisualElement tooltipStatsList;

    private Bag currentBag;
    private PlayerInventory currentPlayerInventory;
    private InteractableBag currentBagScript;

    // We now do everything in Awake for safety.
    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        lootPanel = root.Q<VisualElement>("loot-panel");
        itemList = root.Q<ScrollView>("item-list");
        lootAllButton = root.Q<Button>("loot-all-button");
        closeButton = root.Q<Button>("close-button");

        // --- Query the tooltip elements ---
        itemTooltip = root.Q<VisualElement>("item-tooltip");
        tooltipName = root.Q<Label>("tooltip-name");
        tooltipRarity = root.Q<Label>("tooltip-rarity");
        tooltipDescription = root.Q<Label>("tooltip-description");
        tooltipStatsList = root.Q<VisualElement>("tooltip-stats-list");

        // Hide the panel by default.
        if (lootPanel != null)
        {
            lootPanel.style.display = DisplayStyle.None;
        }

        // Add null checks before subscribing to events
        if (lootAllButton != null) lootAllButton.clicked += LootAll;
        if (closeButton != null) closeButton.clicked += HideLoot;
    }

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
    
    private void RefreshLootList()
    {
        if (itemList == null || currentBag == null) return;
        
        itemList.Clear();

        foreach (var item in currentBag.items)
        {
            // Create a container for the item row
            var itemElement = new VisualElement();
            itemElement.AddToClassList("loot-item");
            
            // Add a class for rarity-based styling from your USS file
            itemElement.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");

            // --- Create and add the icon for the loot item ---
            var icon = new VisualElement();
            icon.AddToClassList("loot-item-icon");
            
            // --- This is the corrected line ---
            if (item.itemData.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(item.itemData.icon as Texture2D);
            }
            itemElement.Add(icon);

            // Create and add the item name label
            var nameLabel = new Label(item.itemData.itemName);
            nameLabel.AddToClassList("item-name");
            nameLabel.AddToClassList($"rarity-{item.itemData.itemRarity.ToString().ToLower()}");
            itemElement.Add(nameLabel);

            // Create and add the item count label
            var countLabel = new Label($"x{item.count}");
            countLabel.AddToClassList("item-count");
            itemElement.Add(countLabel);

            // --- Add hover events for the tooltip ---
            itemElement.RegisterCallback<PointerEnterEvent>(evt => ShowTooltip(evt, item));
            itemElement.RegisterCallback<PointerLeaveEvent>(evt => HideTooltip());

            // Make the entire element clickable to loot the item
            itemElement.RegisterCallback<ClickEvent>(evt => LootItem(item));

            // Add the fully constructed item row to the list
            itemList.Add(itemElement);
        }
    }
    
    private void LootItem(ItemInstance item)
    {
        currentPlayerInventory.AddItemInstance(item);
        currentBag.items.Remove(item);
        if (currentBag.items.Count == 0)
        {
            if (currentBagScript != null) currentBagScript.OnAllLooted();
            HideLoot();
        }
        else
        {
            RefreshLootList();
        }
    }
    
    private void LootAll()
    {
        foreach (var item in new List<ItemInstance>(currentBag.items))
        {
            currentPlayerInventory.AddItemInstance(item);
        }
        currentBag.items.Clear();
        if (currentBagScript != null) currentBagScript.OnAllLooted();
        HideLoot();
    }

    // --- Add the tooltip handler methods ---
    private void ShowTooltip(PointerEnterEvent evt, ItemInstance item)
    {
        if (item == null || item.itemData == null || itemTooltip == null) return;

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
        if(itemTooltip != null)
        {
            itemTooltip.style.display = DisplayStyle.None;
        }
    }
}
