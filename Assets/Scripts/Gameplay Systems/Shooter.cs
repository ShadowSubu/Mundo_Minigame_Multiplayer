using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private ProjectileType selectedProjectile;
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

    [SerializeField] private Transform firePoint;
    [SerializeField] LayerMask shootingLayer;
    [SerializeField, Tooltip("in ms")] private int channelDuration;

    private float cooldownTime = 0f;
    private bool isShooting = false;

    private Camera mainCamera;

    public event EventHandler<float> OnCooldownChanged;
    public event EventHandler OnShoot;

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeProjectileDictionary();
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

    private void Update()
    {
        if (IsOwner && Input.GetMouseButton(0))
        {
            if (isShooting) return;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootingLayer))
            {
                // Calculate direction from fire point to mouse click
                Vector3 direction = (hit.point - firePoint.position).normalized;
                direction.y = 0;

                Shoot(GetMouseWorldPosition(Input.mousePosition), selectedProjectile, direction, OwnerClientId);
            }
        }
        UpdateCoolDown();
    }

    private void UpdateCoolDown()
    {
        if (cooldownTime > 0f)
        {
            cooldownTime -= Time.deltaTime;
            if (cooldownTime < 0f)
            {
                cooldownTime = 0f;
                CancelInvoke(nameof(UpdateCooldownUI));
                UpdateCooldownUI();
            }
        }
    }

    private void UpdateCooldownUI()
    {
        OnCooldownChanged?.Invoke(this, cooldownTime);
    }

    public async void Shoot(Ray ray, ProjectileType projectileType, Vector3 direction, ulong ownerClientId)
    {
        if (cooldownTime > 0f)
        {
            return; 
        }
        isShooting = true;

        GetComponent<PlayerController>().RotateToDirectionRpc(direction);
        await Task.Delay(channelDuration);

        // Send to server to spawn bullet
        SpawnBulletServerRpc(ray, projectileType, firePoint.position, direction, ownerClientId);

        if (DeveloperDashboard.Instance.OverrideValues)
        {
            cooldownTime = GetProjectileCooldownDev(selectedProjectile);
        }
        else
        {
            cooldownTime = projectilesDictionary[selectedProjectile].MaxCooldown;
        }

        InvokeRepeating(nameof(UpdateCooldownUI), 0f, 0.1f);
        OnShoot?.Invoke(this, EventArgs.Empty);
        isShooting = false;
    }

    [Rpc(SendTo.Server)]
    void SpawnBulletServerRpc(Ray ray, ProjectileType projectileType, Vector3 spawnPos, Vector3 direction, ulong ownerClientId)
    {
        Debug.Log("Selected projectile : " + selectedProjectile);
        ProjectileBase projectile = Instantiate(projectilesDictionary[projectileType], spawnPos, Quaternion.LookRotation(direction));

        if (projectile != null)
        {
            projectile.Initialize(ray, direction, this.NetworkObject);
        }

        projectile.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientId, true);
    }

    [Rpc(SendTo.Owner)]
    public void ReduceCooldownRpc(float amount)
    {
        cooldownTime -= amount;
        UpdateCooldownUI();
    }

    [Rpc(SendTo.Owner)]
    public void ResetCooldownRpc()
    {
        cooldownTime = 0f;
        UpdateCooldownUI();
    }

    public float GetMaxCooldown()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            return GetProjectileCooldownDev(selectedProjectile);
        }
        else
        {
            return projectilesDictionary[selectedProjectile].MaxCooldown;
        }
    }

    Ray GetMouseWorldPosition(Vector3 mousePosition)
    {
        Debug.Log("Mouse Position: " + mousePosition);
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 2f);
        return ray;
    }

    public LayerMask ShootingLayer => shootingLayer;
    public Transform FirePoint => firePoint;
    public ProjectileBase SelectedProjectile => projectilesDictionary[selectedProjectile];
    public ProjectileType SelectedProjectileType => selectedProjectile;
    public bool IsShooting => isShooting;

    #region Testing

    public void SelectProjectile(ProjectileType type)
    {
        selectedProjectile = type;
    }

    public void SelectProjectile(string type)
    {
        selectedProjectile = projectileTypeMapping[type];
    }

    public void SetChannelDuration(int value)
    {
        channelDuration = value;
    }

    #endregion

    #region Development 

    private float GetProjectileCooldownDev(ProjectileType type)
    {
        return type switch
        {
            ProjectileType.Normal => DeveloperDashboard.Instance.GetBulletCooldown(),
            ProjectileType.Boomerang => DeveloperDashboard.Instance.GetBoomerangMaxCooldown(),
            _ => projectilesDictionary[selectedProjectile].MaxCooldown,
        };
    }

    #endregion
}