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

    protected NetworkObject shooterObject;
    protected Vector3 startPosition;
    protected Vector3 moveDirection;

    internal void Initialize(Vector3 direction, NetworkObject shooter)
    {
        moveDirection = direction.normalized;
        shooterObject = shooter;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!IsServer) return;
        ProjectileBehaviour();
    }

    private void OnTriggerEnter(Collider other)
    {
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

    public ulong GetShooterClientId() => shooterObject.OwnerClientId;
    public ProjectileType ProjectileType => projectileType;
    public float MaxCooldown => maxCooldown;
}
