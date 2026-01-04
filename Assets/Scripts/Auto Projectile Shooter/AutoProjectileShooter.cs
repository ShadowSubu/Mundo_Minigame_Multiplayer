using NullReferenceDetection;
using UnityEngine;

public class AutoProjectileShooter : Shooter
{
    [Header("Auto Shooter Settings")]
    [Tooltip("Where should the bullet go to"), SerializeField, ValueRequired] public Transform targetTransform;
    [Tooltip("Time in seconds"), SerializeField, ValueRequired] public float fireInterval=2;

    private void Start()
    {
        Debug.Log("Auto Shooter Start");
        InvokeRepeating(nameof(ShootOnTimer), fireInterval, fireInterval);
    }
    
    private void ShootOnTimer()
    {
        // Calculate direction from fire point to mouse click
        Vector3 direction = (targetTransform.position - firePoint.position).normalized;
        direction.y = 0;
        
        Shoot(GetMouseWorldPosition(targetTransform.position), selectedProjectile, direction, OwnerClientId);
    }
    
    protected override void Update()
    {
        // if (DeveloperDashboard.Instance.DashboardEnabled) return;
        //
        // if (IsOwner && Input.GetMouseButton(0))
        // {
        //     if (isShooting) return;
        //     Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //
        //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootingLayer))
        //     {
        //         // Calculate direction from fire point to mouse click
        //         Vector3 direction = (hit.point - firePoint.position).normalized;
        //         direction.y = 0;
        //
        //         Shoot(GetMouseWorldPosition(Input.mousePosition), selectedProjectile, direction, OwnerClientId);
        //     }
        // }
        UpdateCoolDown();
    }
}
