using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterCustomizationTab { Head, Body, Color }

public class CharacterCustomizationMenu : MonoBehaviour
{
    [Header("Character Customization Setup Reference")]
    [Tooltip("The character GameObject whose rig slots will be modified.")]
    [SerializeField] private GameObject characterObject;
    [SerializeField] private CinemachineCamera previewCamera;
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Light previewLight;

    /// TODO: When implementing equip functionality, retrieve the head and body
    /// rig transforms from characterObject here (e.g. via a component or
    /// by searching child transforms by name/tag).

    private CharacterCustomizationTab _activeTab;

    [Header("Tab Buttons")]
    [SerializeField] private Button headTabButton;
    [SerializeField] private Button bodyTabButton;
    [SerializeField] private Button colorTabButton;

    [Header("Tab Panels")]
    [SerializeField] private GameObject headPanel;
    [SerializeField] private GameObject bodyPanel;
    [SerializeField] private GameObject colorPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button closeMenuButton;

    // Optional: visuals that distinguish which tab is active
    // e.g. swap sprites, change colours, animate underline, etc.
    [Header("Tab Active Visuals (optional)")]
    [SerializeField] private Color tabActiveColor   = Color.white;
    [SerializeField] private Color tabInactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Head Slot")]
    [Tooltip("ScriptableObject listing all available head items.")]
    [SerializeField] private CharacterItemList headItemList;

    [Tooltip("Parent transform (with a Layout Group) where head buttons are spawned.")]
    [SerializeField] private Transform headItemGrid;

    [Tooltip("Your item button prefab (must have ItemSelectionButton component).")]
    [SerializeField] private CharacterCustomizationItemSelectionButton itemButtonPrefab;

    private readonly List<CharacterCustomizationItemSelectionButton> _headButtons = new();
    private CharacterItemData _selectedHeadItem;

    [Header("Body Slot")]
    [Tooltip("ScriptableObject listing all available body items.")]
    [SerializeField] private CharacterItemList bodyItemList;

    [Tooltip("Parent transform (with a Layout Group) where body buttons are spawned.")]
    [SerializeField] private Transform bodyItemGrid;

    private readonly List<CharacterCustomizationItemSelectionButton> _bodyButtons = new();
    private CharacterItemData _selectedBodyItem;

    [Header("Body Color — HSV Wheel")]
    [Tooltip("The RawImage or Image that displays the HSV color wheel texture.")]
    [SerializeField] private RawImage colorWheelImage;

    [Tooltip("Cursor RectTransform dragged around the wheel to pick color.")]
    [SerializeField] private RectTransform colorCursor;

    /// Internal HSV state
    private float _hue        = 0f;   // 0–1
    private float _saturation = 1f;   // 0–1
    private float _value      = 1f;   // 0–1

    /// Whether the player is currently dragging on the color wheel
    private bool _isDraggingWheel = false;

    private void Awake()
    {
        InitializeHeadTab();
        InitializeBodyTab();
        InitializeColorTab();

        // Wire tab buttons
        headTabButton.onClick.AddListener(() => SelectTab(CharacterCustomizationTab.Head));
        bodyTabButton.onClick.AddListener(() => SelectTab(CharacterCustomizationTab.Body));
        colorTabButton.onClick.AddListener(() => SelectTab(CharacterCustomizationTab.Color));

        ToggleMenuElements(false);
    }

    private void OnEnable()
    {
        saveButton.onClick.AddListener(SaveCustomization);
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveListener(SaveCustomization);
    }

    public void OpenMenu(LobbyUI invoker)
    {
        closeMenuButton.onClick.AddListener(() => CloseMenu(invoker));
        ToggleMenuElements(true);
        SelectTab(CharacterCustomizationTab.Head);
    }

    public void CloseMenu(LobbyUI invoker)
    {
        ToggleMenuElements(false);
        invoker.CloseCharacterCustomization();
        closeMenuButton.onClick.RemoveListener(() => CloseMenu(invoker));
    }

    private void ToggleMenuElements(bool value)
    {
        characterObject.SetActive(value);
        previewLight.gameObject.SetActive(value);
        previewCamera.gameObject.SetActive(value);
        menuCanvas.gameObject.SetActive(value);
    }

    public void SaveCustomization()
    {
        // TODO : Implement saving logic here. Netcode
    }

    private void SelectTab(CharacterCustomizationTab tab)
    {
        _activeTab = tab;

        headPanel.SetActive(tab == CharacterCustomizationTab.Head);
        bodyPanel.SetActive(tab == CharacterCustomizationTab.Body);
        colorPanel.SetActive(tab == CharacterCustomizationTab.Color);

        RefreshTabButtonVisuals();
    }

    /// <summary>
    /// Updates tab button colours (or swap sprites) to reflect the active tab.
    /// Extend this to animate underlines, scale, etc.
    /// </summary>
    private void RefreshTabButtonVisuals()
    {
        SetTabButtonVisual(headTabButton,  _activeTab == CharacterCustomizationTab.Head);
        SetTabButtonVisual(bodyTabButton,  _activeTab == CharacterCustomizationTab.Body);
        SetTabButtonVisual(colorTabButton, _activeTab == CharacterCustomizationTab.Color);
    }

    private void SetTabButtonVisual(Button btn, bool isActive)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isActive ? tabActiveColor : tabInactiveColor;
    }

    private void InitializeHeadTab()
    {
        if (headItemList == null || itemButtonPrefab == null) return;

        foreach (var item in headItemList.items)
        {
            var btn = Instantiate(itemButtonPrefab, headItemGrid);
            btn.Initialize(item, OnHeadItemSelected);
            _headButtons.Add(btn);
        }
    }

    private void OnHeadItemSelected(CharacterItemData item)
    {
        _selectedHeadItem = item;

        // Update button highlight states
        foreach (var btn in _headButtons)
            btn.SetSelected(btn.name == item.name); // buttons are named after item by default

        // TODO: Instantiate item.modelPrefab and attach it to the character's
        //       head rig slot transform, replacing any previously equipped model.
        Debug.Log($"[CharacterMenu] Head item selected: {item.itemName}");
    }

    private void InitializeBodyTab()
    {
        if (bodyItemList == null || itemButtonPrefab == null) return;

        foreach (var item in bodyItemList.items)
        {
            var btn = Instantiate(itemButtonPrefab, bodyItemGrid);
            btn.Initialize(item, OnBodyItemSelected);
            _bodyButtons.Add(btn);
        }
    }

    private void OnBodyItemSelected(CharacterItemData item)
    {
        _selectedBodyItem = item;

        foreach (var btn in _bodyButtons)
            btn.SetSelected(btn.name == item.name);

        // TODO: Instantiate item.modelPrefab and attach it to the character's
        //       body rig slot transform, replacing any previously equipped model.
        Debug.Log($"[CharacterMenu] Body item selected: {item.itemName}");
    }

    private void InitializeColorTab()
    {
        // TODO: Generate the HSV wheel texture and assign it to colorWheelImage.
        //       A typical approach is to create a Texture2D, iterate its pixels,
        //       convert each pixel position to (H, S) polar coordinates with V=1,
        //       then call Color.HSVToRGB and apply via SetPixels / Apply().

        // TODO: Subscribe to pointer events on colorWheelImage so that
        //       OnColorWheelPointerDown / OnColorWheelDrag can update _hue and
        //       _saturation from the cursor position.
        //       Use IPointerDownHandler, IDragHandler, IPointerUpHandler on
        //       a helper component attached to colorWheelImage's GameObject.

        // TODO: Initialize colorCursor position to match default (_hue, _saturation).

        Debug.Log("[CharacterMenu] Color tab initialized (wheel generation pending).");
    }

    /// <summary>
    /// Call this from a pointer/drag handler on the color wheel image.
    /// Pass the local position of the pointer within the wheel RectTransform.
    /// </summary>
    public void OnColorWheelInput(Vector2 localPoint)
    {
        // TODO: Convert localPoint (relative to wheel center) to polar (angle → hue,
        //       radius → saturation). Clamp radius to wheel bounds.
        //       Update _hue and _saturation, then call ApplyColor().

        // TODO: Move colorCursor to localPoint (clamped to circle boundary).
    }

    /// <summary>
    /// Called when the Value (brightness) slider changes.
    /// Hook this to a Slider's onValueChanged event in the Inspector.
    /// </summary>
    public void OnValueSliderChanged(float value)
    {
        _value = Mathf.Clamp01(value);

        // TODO: ApplyColor();
    }

    /// <summary>
    /// Applies the current HSV values to the character's body material(s).
    /// </summary>
    private void ApplyColor()
    {
        Color selectedColor = Color.HSVToRGB(_hue, _saturation, _value);

        // TODO: Apply selectedColor to the character's body renderer material
        //       (e.g. characterObject.GetComponentInChildren<SkinnedMeshRenderer>()
        //       .material.color = selectedColor;  or via a MaterialPropertyBlock).

        Debug.Log($"[CharacterMenu] Color applied: H={_hue:F2} S={_saturation:F2} V={_value:F2}  →  {selectedColor}");
    }
}
