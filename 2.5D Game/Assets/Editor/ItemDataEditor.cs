using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty itemTypeProp = serializedObject.FindProperty("itemType");
        string enumName = itemTypeProp.enumNames[itemTypeProp.enumValueIndex];
        ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), enumName);

        EditorGUILayout.LabelField("Item Information", EditorStyles.boldLabel); // Item Information
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemID")); // Item ID
        EditorGUILayout.PropertyField(serializedObject.FindProperty("count")); // Count
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxCount")); // Max Count
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemLevel")); // Item Level
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName")); // Item Name
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription")); // Item Description
        
        serializedObject.FindProperty("icon").objectReferenceValue = 
            EditorGUILayout.ObjectField("Icon", serializedObject.FindProperty("icon").objectReferenceValue, typeof(Texture), false);

        EditorGUILayout.Space();

        ItemData itemData = (ItemData)target;
        if (itemType == ItemType.Consumable)
        {
            EditorGUILayout.LabelField("Is Stackable", itemData.isStackable.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("Is Equippable", itemData.isEquippable.ToString());
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Type", EditorStyles.boldLabel); // Item Type
        EditorGUILayout.PropertyField(itemTypeProp, new GUIContent("Item Type")); // Item Type

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Sub Type", EditorStyles.boldLabel); 
        
        switch (itemType)
        {
            case ItemType.Weapon:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponSubType"), new GUIContent("Weapon Type")); // Weapon Sub Type
                break;
            case ItemType.Armor:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("armorSubType"), new GUIContent("Armor Type")); // Armor Sub Type
                break;
            case ItemType.Consumable:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("consumableSubType"), new GUIContent("Consumable Type")); // Consumable Sub Type
                break;
            case ItemType.Quest:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questSubType"), new GUIContent("Quest Type")); // Quest Sub Type
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Rarity", EditorStyles.boldLabel); // Item Rarity
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRarity"), new GUIContent("Item Rarity")); // Item Rarity

        if (itemType == ItemType.Consumable)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Consumable Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRestore"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("manaRestore"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRestore"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("critChanceRestore"));

            // Get the current consumable subtype
            SerializedProperty subTypeProp = serializedObject.FindProperty("consumableSubType");
            ConsumableSubType consumableSubType = (ConsumableSubType)subTypeProp.enumValueIndex;

            if (consumableSubType == ConsumableSubType.Potion)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("potionType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("potionCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("potionMaxCount"));
            }
        }

        if (itemType == ItemType.Weapon) // Weapon Stats
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType")); // Weapon Type
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponHand")); // Weapon Hand
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponWeight")); // Weapon Weight
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damage")); // Damage
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponCritChance")); // Crit Chance
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponCritDamage")); // Crit Damage
        }

        if (itemType == ItemType.Armor) // Armor Stats
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Armor Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("armorWeight")); // Armor Weight
            EditorGUILayout.PropertyField(serializedObject.FindProperty("armor")); // Armor
            EditorGUILayout.PropertyField(serializedObject.FindProperty("health")); // Health
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mana")); // Mana
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stamina")); // Stamina
            EditorGUILayout.PropertyField(serializedObject.FindProperty("armorCritChance")); // Crit Chance
            EditorGUILayout.PropertyField(serializedObject.FindProperty("armorCritDamage")); // Crit Damage
        }

        if (itemType == ItemType.Quest) // Quest Stats
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quest Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questID")); // Quest ID
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questProgress")); // Quest Progress
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questProgressMax")); // Quest Progress Max
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questReward")); // Quest Reward
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questRewardMax")); // Quest Reward Max
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questRewardMin")); // Quest Reward Min
        }

        serializedObject.ApplyModifiedProperties();
    }
}

