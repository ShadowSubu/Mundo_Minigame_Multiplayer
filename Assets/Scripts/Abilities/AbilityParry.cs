using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AbilityParry : AbilityBase
{
    [SerializeField] private Collider parryCollider;
    [SerializeField] private float parryDuration = 0.5f;
    [SerializeField] private GameObject parryVisual;

    private void Awake()
    {
        parryCollider = GetComponent<Collider>();
        parryVisual.SetActive(false);
        parryCollider.enabled = false;
    }

    internal override void OnAbilityUse(Ray ray)
    {
        Debug.Log("Parry Ability Used");
        RequestParryRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestParryRpc()
    {
        ParryRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ParryRpc()
    {
        // turn off the health player hit collider
        casterObject.GetComponent<Collider>().enabled = false;
        // turn on the parry collider
        parryCollider.enabled = true;
        parryVisual.SetActive(true);
        // after a short duration, turn off the parry collider and turn on the health player hit collider
        Invoke(nameof(EndParry), parryDuration);
    }

    private void EndParry()
    {
        casterObject.GetComponent<Collider>().enabled = true;
        parryCollider.enabled = false;
        parryVisual.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if the other projectile collides with this parry collider, destroy the projectile
        other.TryGetComponent(out NetworkObject hit);
        if (hit != null && hit.OwnerClientId != casterObject.OwnerClientId)
        {
            hit.TryGetComponent(out ProjectileBase projectile);
            if (projectile != null)
            {
                Vector3 shooterPosition = projectile.GetOwnerNetworkObject().transform.position;
                ProjectileType opponentProjectileType = projectile.ProjectileType;
                projectile.DestroyBullet();
                RequestDeflectProjectileRpc(shooterPosition, opponentProjectileType);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestDeflectProjectileRpc(Vector3 shooterPosition, ProjectileType opponentProjectileType)
    {
        DeflectProjectileRpc(shooterPosition, opponentProjectileType);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DeflectProjectileRpc(Vector3 shooterPosition, ProjectileType opponentProjectileType)
    {
        casterObject.TryGetComponent(out Shooter shooter);

        if (shooter != null && shooter.IsOwner)
        {
            Debug.Log("Parry Successful - Deflecting Projectile");
            ProjectileType myProjectileType = shooter.SelectedProjectileType;

            // Switch to opponent's projectile type and shoot
            shooter.SelectProjectile(opponentProjectileType);

            Vector3 shootDirection = (shooterPosition - casterObject.transform.position).normalized;
            shootDirection.y = 0;

            shooter.Shoot(shootDirection);

            // Switch back to original projectile type
            shooter.SelectProjectile(myProjectileType);
        }
    }
}
