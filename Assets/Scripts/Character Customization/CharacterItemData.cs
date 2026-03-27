using UnityEngine;

/// <summary>
/// ScriptableObject representing a single equippable item (head or body).
/// Create individual item assets via:
/// Right-click > Create > Character Customization > Item Data
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Character Customization/Item Data")]
public class CharacterItemData : ScriptableObject
{
    //[Header("Display")]
    //[Tooltip("The icon shown on the selection button in the UI.")]
    //public Sprite icon;

    [Tooltip("Human-readable name shown in the UI.")]
    public string itemName = "New Item";

    [Header("Model")]
    [Tooltip("The 3D model prefab that will be instantiated and attached to the character's rig slot.")]
    public GameObject modelPrefab;
}
