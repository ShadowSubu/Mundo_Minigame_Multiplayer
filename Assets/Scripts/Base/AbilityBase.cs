using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is a networked base class for any Ability
/// </summary>
public abstract class AbilityBase : NetworkBehaviour
{
    [SerializeField] private AbilityType abilityType;

    [SerializeField] protected float maxCooldown = 25f;

    internal NetworkObject casterObject;

    public event EventHandler<float> OnAbilityCooldownTick;

    /// <summary>
    /// Setup the ability before use
    /// </summary>
    /// <param name="caster">Player</param>
    public void InitializeAbility(NetworkObject caster)
    {
        casterObject = caster;
    }

    internal abstract void OnAbilityUse(Ray ray, GameManager.Team team);

    public NetworkObject CasterObject => casterObject;
    public AbilityType AbilityType => abilityType;
    public float MaxCooldown => maxCooldown;
}

[Serializable]
public enum AbilityType
{
    None,
    Parry,
    Blink,
    FakeShot,
    Invisibility,
    SpeedBoost
}
