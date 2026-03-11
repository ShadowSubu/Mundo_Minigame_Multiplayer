using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to your item button prefab.
/// The CharacterCustomizationMenu will instantiate and initialize these automatically.
/// 
/// Prefab setup expected:
///   - Button (this component lives here)
///     - IconImage   (Image component)
///     - NameLabel   (TextMeshProUGUI, optional)
///     - SelectedHighlight (GameObject, shown when this item is selected)
/// </summary>
[RequireComponent(typeof(Button))]
public class CharacterCustomizationItemSelectionButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameLabel;          // optional
    [SerializeField] private GameObject selectedHighlight;       // optional

    private CharacterItemData _itemData;
    private System.Action<CharacterItemData> _onSelected;

    /// <summary>
    /// Called by CharacterCustomizationMenu after instantiating this prefab.
    /// </summary>
    public void Initialize(CharacterItemData data, System.Action<CharacterItemData> onSelected)
    {
        _itemData    = data;
        _onSelected  = onSelected;

        // Populate visuals
        if (iconImage != null)
            iconImage.sprite = data.icon;

        if (nameLabel != null)
            nameLabel.text = data.itemName;

        SetSelected(false);

        // Wire up the button click
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _onSelected?.Invoke(_itemData);
    }

    /// <summary>
    /// Toggles the selected highlight visual on this button.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }
}
