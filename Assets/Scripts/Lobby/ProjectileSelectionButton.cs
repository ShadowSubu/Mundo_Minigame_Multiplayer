using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectileSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string projectileName;
    [SerializeField, TextArea] private string description;
    private Button button;

    private void Awake()
    {
        button= GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(SelectProjectile);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(SelectProjectile);
    }

    public event EventHandler<GameObject> OnProjectileSelected;
    public async void SelectProjectile()
    {   
        await LobbyManager.Instance.SavePlayerProjectileSelection(projectileName);
        OnProjectileSelected?.Invoke(this, gameObject);
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
