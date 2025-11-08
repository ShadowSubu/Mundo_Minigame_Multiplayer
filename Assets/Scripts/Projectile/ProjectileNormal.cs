using UnityEngine;

public class ProjectileNormal : ProjectileBase
{
    [SerializeField] private float maxDistance = 20f;

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            projectileDamage = DeveloperDashboard.Instance.GetBulletDamage();
            projectileSpeed = DeveloperDashboard.Instance.GetBulletProjectileSpeed();
            maxDistance = DeveloperDashboard.Instance.GetBulletMaxDistance();
        }
    }

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        TargetBase hit = other.GetComponent<TargetBase>();
        Debug.Log($"Projectile hit: {other.name}, OwnerClientId: {hit?.OwnerClientId}");
        if (hit != null && hit.GetComponent<PlayerController>().PlayerTeam != ShooterObject.GetComponent<PlayerController>().PlayerTeam)
        {
            // Hit an opponent - destroy bullet
            hit.ReceiveDamageRpc(projectileDamage);
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

    public float MaxDistance => maxDistance;
}
