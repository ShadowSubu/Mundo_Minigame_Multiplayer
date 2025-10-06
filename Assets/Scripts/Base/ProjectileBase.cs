using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// This is a networked base class for any projectile
/// </summary>
[RequireComponent(typeof(NetworkTransform))]
public abstract class ProjectileBase : NetworkBehaviour
{
    [SerializeField] private ProjectileType projectileType;

    [SerializeField] protected float projectileSpeed = 20f;
    [SerializeField] protected byte projectileDamage = 10;
    [SerializeField] protected float maxCooldown = 4f;

    protected NetworkVariable<NetworkObjectReference> shooterObject = new NetworkVariable<NetworkObjectReference>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    protected Vector3 startPosition;
    protected Vector3 moveDirection;

    internal void Initialize(Vector3 direction, NetworkObject shooterObject)
    {
        this.shooterObject.Value = shooterObject;
        moveDirection = direction.normalized;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!IsServer) return;
        ProjectileBehaviour();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Condition to check if projectile hits another projectile, if so, destroy them and immediately refund cooldown to all shooters
        if (other.TryGetComponent(out ProjectileBase projectile))
        {
            DestroyBullet();
            projectile.ShooterObject.TryGetComponent(out Shooter shooter);
            if (shooter != null)
            {
                shooter.ResetCooldownRpc();
            }
        }

        OnTriggerEnterBehaviour(other);
    }

    internal abstract void OnTriggerEnterBehaviour(Collider other);

    /// <summary>
    /// This method is called in the Update function, Call any logic that you want to run every frame
    /// </summary>
    internal abstract void ProjectileBehaviour();

    internal void DestroyBullet()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public NetworkObject ShooterObject => shooterObject.Value;
    public ProjectileType ProjectileType => projectileType;
    public float MaxCooldown => maxCooldown;

    public NetworkObject GetOwnerNetworkObject()
    {
        return shooterObject.Value;
    }
}

[Serializable]
public enum ProjectileType
{
    Normal,
    Boomerang
}