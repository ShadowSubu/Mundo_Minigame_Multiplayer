using System;
using Unity.Netcode;
using UnityEngine;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private ProjectileBoomerang projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] LayerMask shootingLayer;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float maxCooldown = 5f;

    private float cooldownTime = 0f;

    private Camera mainCamera;

    public event EventHandler<float> OnCooldownChanged;

    private void Awake()
    {
        mainCamera = Camera.main;
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
            cooldownTime = maxCooldown;
            InvokeRepeating(nameof(UpdateCooldownUI), 0f, 0.1f);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        ProjectileBoomerang projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        if (projectile != null)
        {
            projectile.Initialize(direction, this.NetworkObject);
        }

        projectile.GetComponent<NetworkObject>().Spawn(true);
    }

    public void ReduceCooldown(float amount)
    {
        cooldownTime -= amount;
        UpdateCooldownUI();
    }

    public float GetMaxCooldown()
    {
        return maxCooldown;
    }
}
