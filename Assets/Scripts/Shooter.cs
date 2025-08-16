using System;
using Unity.Netcode;
using UnityEngine;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Calculate direction from fire point to mouse click
            Vector3 direction = (hit.point - firePoint.position).normalized;
            direction.y = 0;

            // Send to server to spawn bullet
            SpawnBulletServerRpc(firePoint.position, direction);
        }

        cooldownTime = maxCooldown;
        InvokeRepeating(nameof(UpdateCooldownUI), 0f, 0.1f);
    }

    [Rpc(SendTo.Server)]
    void SpawnBulletServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        Projectile projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        if (projectile != null)
        {
            projectile.Initialize(direction, bulletSpeed, maxDistance, OwnerClientId);
        }

        projectile.GetComponent<NetworkObject>().Spawn(true);
    }

    public float GetMaxCooldown()
    {
        return maxCooldown;
    }
}
