using UnityEngine;

public class ProjectileNormal : ProjectileBase
{
    [SerializeField] private float maxDistance = 20f;

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        if (!IsServer) return;

        TargetBase hit = other.GetComponent<TargetBase>();
        Debug.Log($"Projectile hit: {other.name}, OwnerClientId: {hit?.OwnerClientId}");
        if (hit != null && hit.OwnerClientId != shooterObject.OwnerClientId)
        {
            // Hit an opponent - destroy bullet
            hit.ReceiveHitpointsRpc(projectileDamage, OwnerClientId);
            DestroyBullet();
        }
    }

    internal override void ProjectileBehaviour()
    {
        // Move bullet
        transform.position += moveDirection * projectileSpeed * Time.deltaTime;

        // Check if traveled max distance
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            DestroyBullet();
        }
    }
}
