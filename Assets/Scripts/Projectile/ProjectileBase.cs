using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public abstract class ProjectileBase : NetworkBehaviour
{
    [SerializeField] private GameObject projectileGO;
    [SerializeField] protected float projectileSpeed = 20f;
    [SerializeField] protected float projectileDamage = 10f;

    protected ulong shooterClientId;
    protected Vector3 startPosition;
    protected Vector3 moveDirection;

    internal void Initialize(Vector3 direction, ulong shooterClientId)
    {
        moveDirection = direction.normalized;
        this.shooterClientId = shooterClientId;
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
    internal abstract void ProjectileBehaviour();

    internal void DestroyBullet()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public ulong GetShooterClientId() => shooterClientId;
}
