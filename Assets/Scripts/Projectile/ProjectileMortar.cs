using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectileMortar : ProjectileBase
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float travelDuration = 1f;
    [SerializeField] private float height = 5f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private AnimationCurve heightCurve;
    bool bulletShot = false;

    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            projectileDamage = DeveloperDashboard.Instance.GetMortarDamage();
            projectileSpeed = DeveloperDashboard.Instance.GetMortarProjectileSpeed();
            maxCooldown = DeveloperDashboard.Instance.GetMortarMaxCooldown();
            height = DeveloperDashboard.Instance.GetMortarMaxHeight();
            explosionRadius = DeveloperDashboard.Instance.GetMortarExplosionRadius();
        }
    }

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        GameManager.Team myTeam = ShooterObject.GetComponent<PlayerController>().PlayerTeam;

        // If the player hits it's own team, then ignore
        if (other.TryGetComponent(out PlayerController controller))
        {
            if (controller.PlayerTeam != myTeam)
            {
                return;
            }
        }

        GetComponent<Collider>().enabled = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider item in hits)
        {
            Debug.Log(item.name);
            if (item.TryGetComponent(out TargetBase target))
            {
                if (target != null && target.GetComponent<PlayerController>().PlayerTeam != myTeam)
                {
                    Debug.Log($"Dealing {projectileDamage} mortar Damage to {target.name}");
                    target.ReceiveDamageRpc(projectileDamage);
                }
            }
        }
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        DestroyBullet();
    }

    private void Explode()
    {

    }

    internal override void ProjectileBehaviour()
    {
        if (!bulletShot)
        {
            bulletShot = true;
            Debug.Log("Shooting mortar projectile");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                cancellationTokenSource = new CancellationTokenSource();
                MoveParabolaAsync(transform.position, hit.point, cancellationTokenSource.Token);
            }
            else
            {
                Debug.Log("Did not hit anything");
            }
        }
    }

    public async void MoveParabolaAsync(Vector3 start, Vector3 end, CancellationToken token)
    {
        float time = 0f;

        while (time < travelDuration)
        {
            if (cancellationTokenSource.IsCancellationRequested) return;
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / travelDuration);

            // Linear interpolation
            Vector3 horizontal = Vector3.Lerp(start, end, t);

            // Vertical offset for arc
            float yOffset = height * 4f * (t - t * t); // Basic parabola
            if (heightCurve != null && heightCurve.length > 0)
                yOffset = height * heightCurve.Evaluate(t);

            transform.position = new Vector3(horizontal.x, horizontal.y + yOffset, horizontal.z);

            await Task.Yield(); // Wait for next frame
        }

        transform.position = end;
        DestroyBullet();
    }

    public float MaxHeight => height;
    public float ExplosionRadius => explosionRadius;
}
