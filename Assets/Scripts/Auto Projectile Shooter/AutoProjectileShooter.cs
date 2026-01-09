using NullReferenceDetection;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VInspector;

public class AutoProjectileShooter : NetworkBehaviour
{
    [SerializeField] protected ProjectileType selectedProjectile;
    [SerializeField] private List<ProjectileBase> projectilesDatabase;
    private Dictionary<ProjectileType, ProjectileBase> projectilesDictionary;

    private Dictionary<string, ProjectileType> projectileTypeMapping = new()
    {
        { "Bullet", ProjectileType.Normal },
        { "Boomerang", ProjectileType.Boomerang },
        { "Mortar", ProjectileType.Mortar},
        { "Homing", ProjectileType.Homing },
        { "Curved", ProjectileType.Curved }
    };

    [SerializeField, ValueRequired] private Transform firePoint;
    [SerializeField, ValueRequired] private Transform targetPoint;

    private void Awake()
    {
        InitializeProjectileDictionary();
    }

    [Button]
    public void StartShooting()
    {
        InvokeRepeating(nameof(ShootProjectile), 0f, 2f);
    }

    [Button]
    public void StopShooting()
    {
        CancelInvoke(nameof(ShootProjectile));
    }

    private void ShootProjectile()
    {
        //SpawnBullet();
    }

    void SpawnBullet(Ray ray, ProjectileType projectileType, Vector3 spawnPos, Vector3 direction, ulong ownerClientId)
    {
        Debug.Log("Selected projectile : " + selectedProjectile);
        ProjectileBase projectile = Instantiate(projectilesDictionary[projectileType], spawnPos, Quaternion.LookRotation(direction));

        if (projectile != null)
        {
            projectile.Initialize(ray, direction, this.NetworkObject);
        }

        projectile.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientId, true);
    }

    private void InitializeProjectileDictionary()
    {
        projectilesDictionary = new();
        foreach (ProjectileBase projectile in projectilesDatabase)
        {
            if (!projectilesDictionary.ContainsKey(projectile.ProjectileType))
            {
                projectilesDictionary.Add(projectile.ProjectileType, projectile);
            }
        }
    }
    public ProjectileBase SelectedProjectile => projectilesDictionary[selectedProjectile];
    public ProjectileType SelectedProjectileType => selectedProjectile;
}
