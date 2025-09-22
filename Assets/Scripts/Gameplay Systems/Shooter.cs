using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private ProjectileType selectedProjectile;
    [SerializeField] private List<ProjectileBase> projectilesDatabase;
    private Dictionary<ProjectileType, ProjectileBase> projectilesDictionary;

    [SerializeField] private Transform firePoint;
    [SerializeField] LayerMask shootingLayer;

    private float cooldownTime = 0f;

    private Camera mainCamera;

    public event EventHandler<float> OnCooldownChanged;

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
            Shoot();
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

    private void Shoot()
    {
        if (cooldownTime > 0f)
        {
            return; // Exit if still in cooldown
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootingLayer))
        {
            // Calculate direction from fire point to mouse click
            Vector3 direction = (hit.point - firePoint.position).normalized;
            direction.y = 0;

            // Send to server to spawn bullet
            SpawnBulletServerRpc(firePoint.position, direction);
            cooldownTime = projectilesDictionary[selectedProjectile].MaxCooldown;
            InvokeRepeating(nameof(UpdateCooldownUI), 0f, 0.1f);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        ProjectileBase projectile = Instantiate(projectilesDictionary[selectedProjectile], spawnPos, Quaternion.LookRotation(direction));

        if (projectile != null)
        {
            projectile.Initialize(direction, this.NetworkObject);
        }

        projectile.GetComponent<NetworkObject>().Spawn(true);
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
        return projectilesDictionary[selectedProjectile].MaxCooldown;
    }

    public LayerMask ShootingLayer => shootingLayer;
    public Transform FirePoint => firePoint;
    public ProjectileBase SelectedProjectile => projectilesDictionary[selectedProjectile];

    #region Testing

    public void SelectProjectile(ProjectileType type)
    {
        selectedProjectile = type;
    }

    #endregion
}