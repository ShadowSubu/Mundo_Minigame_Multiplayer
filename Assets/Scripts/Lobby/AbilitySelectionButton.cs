using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilitySelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string abilityName;
    [SerializeField, TextArea] private string description;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        button.onClick.AddListener(SelectAbility);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(SelectAbility);
    }

    public event System.EventHandler<GameObject> OnAbilitySelected;
    public async void SelectAbility()
    {
        await LobbyManager.Instance.SavePlayerAbilitySelection(abilityName);
        OnAbilitySelected?.Invoke(this, gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UITooltip.Instance.Show(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UITooltip.Instance.Hide();
    }
}
