using UnityEngine;
using UnityEditor;
using System.Linq;

public class ItemDataIDAssigner : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (itemData != null && itemData.itemID == 0) // If the item has no ID, assign a new one
            {
                string[] guids = AssetDatabase.FindAssets("t:ItemData");
                int maxID = guids
                    .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)))
                    .Where(i => i != null)
                    .Select(i => i.itemID)
                    .DefaultIfEmpty(0)
                    .Max();

                itemData.itemID = maxID + 1;
                EditorUtility.SetDirty(itemData);
                AssetDatabase.SaveAssets();
                Debug.Log("Assigned new ID: " + itemData.itemID + " to " + itemData.itemName);
            }
        }
    }
}
