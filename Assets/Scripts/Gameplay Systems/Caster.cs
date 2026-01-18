using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Caster : NetworkBehaviour
{
    [SerializeField] private AbilityType selectedAbilityType;
    private AbilityType SelectedAbilityType
    {
        get => selectedAbilityType;
        set
        {
            if (selectedAbilityType != value)
            {
                selectedAbilityType = value;
                RequestSetupSelectedAbilityRpc();
            }
        }
    }

    [SerializeField] private List<AbilityBase> abilityDatabase;
    private Dictionary<AbilityType, AbilityBase> abilityDictionary;

    // TODO : Make this configurable from outside the script
    private Dictionary<string, AbilityType> abilityTypeMapping = new()
    {
        { "Blink", AbilityType.Blink },
        { "FakeShot", AbilityType.FakeShot },
        { "Parry", AbilityType.Parry },
        { "Invisibility", AbilityType.Invisibility },
        { "SpeedBoost", AbilityType.SpeedBoost }
    };

    private AbilityBase activeAbility;

    private float cooldownTime = 0f;

    private Camera mainCamera;

    public event EventHandler<float> OnCooldownChanged;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeAbilityDictionary();
    }

    private void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        RequestSetupSelectedAbilityRpc();
    }

    private void InitializeAbilityDictionary()
    {
        abilityDictionary = new();
        foreach (AbilityBase ability in abilityDatabase)
        {
            if (!abilityDictionary.ContainsKey(ability.AbilityType))
            {
                abilityDictionary.Add(ability.AbilityType, ability);
                //Debug.Log($"Added ability: {ability.AbilityType} to dictionary");
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestSetupSelectedAbilityRpc()
    {
        SetupAbilityRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetupAbilityRpc()
    {
        // Make sure all abilities are inactive
        foreach (var ability in abilityDictionary.Values)
        {
            ability.gameObject.SetActive(false);
        }

        // Activate the selected ability
        activeAbility = abilityDictionary[SelectedAbilityType];
        activeAbility.gameObject.SetActive(true);
        activeAbility.InitializeAbility(this.NetworkObject);
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Debug.Log("Casting ability...");
            Cast();
        }
        UpdateCoolDown();
    }

    private void UpdateCoolDown()
    {
        if (cooldownTime > 0f)
        {
            cooldownTime -= Time.deltaTime;
            if (cooldownTime < 0f)
            {
                cooldownTime = 0f;
                CancelInvoke(nameof(UpdateCooldownUI));
                UpdateCooldownUI();
            }
        }
    }

    private void UpdateCooldownUI()
    {
        OnCooldownChanged?.Invoke(this, cooldownTime);
    }

    private void Cast()
    {
        if (cooldownTime > 0f)
        {
            return;
        }

        Debug.Log("Requesting ability use...");
        RequestAbilityUseRpc(GetMouseWorldPosition(Input.mousePosition), GameManager.Instance.GetLocalPlayerTeam());

        if (DeveloperDashboard.Instance.OverrideValues)
        {
            cooldownTime = GetAbilityCooldownDev(SelectedAbilityType);
        }
        else
        {
            cooldownTime = abilityDictionary[SelectedAbilityType].MaxCooldown;
        }
        InvokeRepeating(nameof(UpdateCooldownUI), 0f, 0.1f);
    }

    /// <summary>
    /// Returns a ray from the main camera through the mouse position into the world from where it was clicked.
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <returns></returns>
    Ray GetMouseWorldPosition(Vector3 mousePosition)
    {
        Debug.Log("Mouse Position: " + mousePosition);
        Ray ray = MainCamera.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 2f);
        return ray;
    }

    [Rpc(SendTo.Server)]
    private void RequestAbilityUseRpc(Ray ray, GameManager.Team team)
    {
        if (activeAbility != null)
        {
            Debug.Log("Ability used on server.");
            activeAbility.OnAbilityUse(ray, team);
        }
    }

    public void DestroyAbility()
    {
        if (activeAbility != null && IsServer)
        {
            activeAbility.GetComponent<NetworkObject>().Despawn();
        }
    }

    public float GetMaxCooldown()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            return GetAbilityCooldownDev(SelectedAbilityType);
        }
        else
        {
            return abilityDictionary[SelectedAbilityType].MaxCooldown;
        }
    }

    internal void SelectAbility(string abilityType)
    {
        SelectedAbilityType = abilityTypeMapping[abilityType];
    }

    public Camera MainCamera => mainCamera;
    public AbilityBase SelectedAbility => abilityDictionary[SelectedAbilityType];

    #region Development 

    private float GetAbilityCooldownDev(AbilityType type)
    {
        return type switch
        {
            AbilityType.Blink => DeveloperDashboard.Instance.GetBulletCooldown(),
            AbilityType.FakeShot => DeveloperDashboard.Instance.GetFakeshotCooldown(),
            AbilityType.Parry =>DeveloperDashboard.Instance.GetParryCooldown(),
            _ => abilityDictionary[SelectedAbilityType].MaxCooldown,
        };
    }

    #endregion
}
