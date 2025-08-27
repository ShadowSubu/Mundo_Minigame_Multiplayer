using System;
using UnityEngine;

public class ProjectileBoomerang : ProjectileBase
{
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float returnMaxDistance = 30f;
    [SerializeField] private float cooldownReduction = 2f;

    private Vector3 returnDirection;
    private Vector3 returnStartPosition;
    private bool isReturning = false;

    private bool hasHitTargetForward = false;
    private bool hasHitTargetReturn = false;

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        if (!IsServer) return;

        if (!isReturning && !hasHitTargetForward)
        {
            other.TryGetComponent(out TargetBase hit);
            if (hit != null && hit.OwnerClientId != shooterObject.OwnerClientId)
            {
                // Hit an opponent - destroy bullet
                hit.ReceiveHitpointsRpc(projectileDamage, OwnerClientId);
            }
        }
        else if (isReturning)
        {
            other.TryGetComponent(out Shooter hit);
            if (hit != null && hit.OwnerClientId == shooterObject.OwnerClientId)
            {
                // Hit an opponent - destroy bullet
                hit.ReduceCooldown(cooldownReduction);
                DestroyBullet();
            }
        }
        else if (isReturning && !hasHitTargetReturn)
        {
            other.TryGetComponent(out TargetBase hit);
            if (hit != null && hit.OwnerClientId != shooterObject.OwnerClientId)
            {
                // Hit an opponent - destroy bullet
                hit.ReceiveHitpointsRpc(projectileDamage, OwnerClientId);
            }
        }
    }

    internal override void ProjectileBehaviour()
    {
        if (!isReturning)
        {
            MoveForward();
        }
        else
        {
            MoveBakcward();
        }
    }

    private void MoveForward()
    {
        // Move bullet forward
        transform.position += moveDirection * projectileSpeed * Time.deltaTime;

        // Check if traveled max distance
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            StartReturning();
        }
    }

    private void StartReturning()
    {
        if (!isReturning)
        {
            isReturning = true;
            returnStartPosition = transform.position;
            returnDirection = (shooterObject.transform.position - transform.position).normalized;
        }
    }

    private void MoveBakcward()
    {
        transform.position += returnDirection * projectileSpeed * Time.deltaTime;
        float distanceTraveled = Vector3.Distance(returnStartPosition, transform.position);
        if (distanceTraveled >= returnMaxDistance)
        {
            DestroyBullet();
        }
    }
}
