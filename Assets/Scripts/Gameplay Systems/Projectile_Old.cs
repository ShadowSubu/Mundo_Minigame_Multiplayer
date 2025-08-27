using Unity.Netcode;
using UnityEngine;

public class Projectile_Old : NetworkBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float maxDistance;
    private Vector3 startPosition;
    private ulong shooterClientId;
    [SerializeField] private byte damage = 10;

    internal void Initialize(Vector3 direction, float bulletSpeed, float maxDistance, ulong ownerClientId)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        this.maxDistance = maxDistance;
        startPosition = transform.position;
        shooterClientId = ownerClientId;
    }

    void Update()
    {
        if (!IsServer) return; // Only server moves bullets

        // Move bullet
        transform.position += moveDirection * speed * Time.deltaTime;

        // Check if traveled max distance
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            DestroyBullet();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerController otherPlayer = other.GetComponent<PlayerController>();
        Debug.Log($"Projectile hit: {other.name}, OwnerClientId: {otherPlayer?.OwnerClientId}");
        if (otherPlayer != null && otherPlayer.OwnerClientId != shooterClientId)
        {
            // Hit an opponent - destroy bullet
            otherPlayer.TakeDamageRpc(damage);
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
    }
}
