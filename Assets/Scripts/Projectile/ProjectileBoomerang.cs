using Unity.Netcode;
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

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            projectileDamage = DeveloperDashboard.Instance.GetBoomerangDamage();
            projectileSpeed = DeveloperDashboard.Instance.GetBoomerangProjectileSpeed();
            maxCooldown = DeveloperDashboard.Instance.GetBoomerangMaxCooldown();
            maxDistance = DeveloperDashboard.Instance.GetBoomerangMaxDistance();
            returnMaxDistance = DeveloperDashboard.Instance.GetBoomerangReturnMaxDistance();
            cooldownReduction = DeveloperDashboard.Instance.GetBoomerangCooldownReduction();
        }
    }

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        other.TryGetComponent(out NetworkObject hit);
        if (hit == null) return;

        hit.TryGetComponent(out PlayerController playerController);

        // Enemy
        if (playerController != null  && playerController.PlayerTeam != ShooterObject.GetComponent<PlayerController>().PlayerTeam)
        {
            if (!isReturning && !hasHitTargetForward)
            {
                hit.TryGetComponent(out TargetBase enemy);
                if (enemy != null)
                {
                    enemy.ReceiveDamageRpc(projectileDamage);
                }
            }
            else if (isReturning && !hasHitTargetReturn)
            {
                hit.TryGetComponent(out TargetBase enemy);
                if (enemy != null)
                {
                    enemy.ReceiveDamageRpc(projectileDamage);
                }
            }
        }
        // Self
        else if (playerController != null && hit.OwnerClientId == ShooterObject.OwnerClientId)
        {
            if (isReturning)
            {
                hit.TryGetComponent(out Shooter shooter);
                if (shooter != null)
                {
                    shooter.ReduceCooldownRpc(cooldownReduction);
                }
                DestroyBullet();
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
            returnDirection = (ShooterObject.transform.position - transform.position).normalized;
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

    public float MaxDistance => maxDistance;
    public float ReturnMaxDistance => returnMaxDistance;
    public float CooldownReduction => cooldownReduction;
}
