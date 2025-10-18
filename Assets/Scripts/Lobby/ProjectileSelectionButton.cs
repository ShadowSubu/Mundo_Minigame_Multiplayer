using System;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileSelectionButton : MonoBehaviour
{
    [SerializeField] private string projectileName;
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
}
