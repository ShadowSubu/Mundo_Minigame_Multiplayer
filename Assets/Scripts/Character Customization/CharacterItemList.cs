using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that holds a list of equippable character items.
/// Create separate assets for Head items and Body items via:
/// Right-click > Create > Character Customization > Item List
/// </summary>
[CreateAssetMenu(fileName = "NewItemList", menuName = "Character Customization/Item List")]
public class CharacterItemList : ScriptableObject
{
    [Tooltip("All equippable items available in this slot category.")]
    public List<CharacterItemData> items = new List<CharacterItemData>();
}
