using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectionButton : MonoBehaviour
{
    [SerializeField] private string abilityName;
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
}
