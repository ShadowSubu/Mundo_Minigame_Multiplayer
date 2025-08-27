using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is a networked base class for any object that is targetable
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class TargetBase : NetworkBehaviour
{
    [SerializeField] private byte maxHealth = 100;
    public event EventHandler<byte> OnHealthChanged;
    private NetworkVariable<byte> currentHealth = new NetworkVariable<byte>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += HandleHealthChanged;
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(byte previousValue, byte newValue)
    {
        OnHealthChanged?.Invoke(this, newValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterBehaviour(other);
    }

    internal abstract void OnTriggerEnterBehaviour(Collider other);
    internal abstract void OnHitpointsDepletedBehaviour();

    /// <summary>
    /// Change the Health Value of this target
    /// </summary>
    /// <param name="hitpoints">Amount to change (-ve to reduce / +ve to increase)</param>
    /// <param name="invokerClientId"></param>
    [Rpc(SendTo.Server)]
    public void ReceiveHitpointsRpc(byte hitpoints, ulong invokerClientId)
    {
        if (!IsServer) return;
        currentHealth.Value = (byte)Mathf.Max(0, currentHealth.Value - hitpoints);
        if (currentHealth.Value <= 0)
        {
            OnHitpointsDepletedBehaviour();
        }
    }

    public byte GetCurrentHealth() => currentHealth.Value;

    public byte GetMaxHealth() => maxHealth;
}
