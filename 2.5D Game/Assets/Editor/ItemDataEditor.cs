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
        EditorGUILayout.Space();

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

        if (itemType == ItemType.Consumable) // Consumable Stats
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Consumable Stats", EditorStyles.boldLabel); // Consumable Stats
            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRestore")); // Health Restore
            EditorGUILayout.PropertyField(serializedObject.FindProperty("manaRestore")); // Mana Restore
            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRestore")); // Stamina Restore
            EditorGUILayout.PropertyField(serializedObject.FindProperty("critChanceRestore")); // Crit Chance Restore
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

