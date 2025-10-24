using Unity.Netcode;
using UnityEngine;

public class AbilityFakeShot : AbilityBase
{
    internal override void OnAbilityUse(Ray ray)
    {
        Shoot(ray);
    }

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            maxCooldown = DeveloperDashboard.Instance.GetFakeshotCooldown();
        }
    }

    private void Shoot(Ray ray)
    {
        Shooter shooter = casterObject.GetComponent<Shooter>();
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, shooter.ShootingLayer))
        {
            // Calculate direction from fire point to mouse click
            Vector3 direction = (hit.point - shooter.FirePoint.position).normalized;
            direction.y = 0;

            SpawnBulletServerRpc(shooter.FirePoint.position, direction);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        Debug.Log("Spawning Fake bullet.");
        Shooter shooter = casterObject.GetComponent<Shooter>();
        ProjectileBase projectile = Instantiate(shooter.SelectedProjectile, spawnPos, Quaternion.LookRotation(direction));
        Destroy(projectile.GetComponent<Collider>());
        Destroy(projectile.GetComponent<Rigidbody>());

        if (projectile != null)
        {
            projectile.Initialize(direction, casterObject);
        }

        projectile.GetComponent<NetworkObject>().Spawn(true);
    }
}
