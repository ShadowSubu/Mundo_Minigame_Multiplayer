using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ProjectileCurved : ProjectileBase
{
    [Header("Curved Projectile Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float travelDuration = 1f;
    [SerializeField] private float curveStrength = 1f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float wobbleAngle = 10f;
    [SerializeField] private float wobbleSpeed = 8f;
    bool bulletShot = false;
    Quaternion baseRotation;

    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            projectileDamage = DeveloperDashboard.Instance.GetCurveDamage();
            projectileSpeed = DeveloperDashboard.Instance.GetCurveProjectileSpeed();
            maxCooldown = DeveloperDashboard.Instance.GetCurveMaxCooldown();
            curveStrength = DeveloperDashboard.Instance.GetCurveStrength();
        }
        baseRotation = transform.rotation;
    }

    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        other.TryGetComponent(out NetworkObject hit);
        if (hit != null && hit.GetComponent<PlayerController>().PlayerTeam != ShooterObject.GetComponent<PlayerController>().PlayerTeam)
        {
            hit.TryGetComponent(out TargetBase enemy);
            if (enemy != null)
            {
                enemy.ReceiveDamageRpc(projectileDamage);
            }
        }
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
                Vector3 end = hit.point;
                end.y = 2.8f;
                MoveCurvedSwingAsync(transform.position, end, cancellationTokenSource.Token);
            }
            else
            {
                Debug.Log("Did not hit anything");
            }
        }

        // Base rotation (spin around Y)
        baseRotation *= Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);

        // Wobble around forward axis
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle;
        Quaternion wobbleRot = Quaternion.AngleAxis(wobble, Vector3.forward);

        // Combine
        transform.rotation = baseRotation * wobbleRot;
    }

    public async void MoveCurvedSwingAsync(Vector3 start, Vector3 end, CancellationToken token)
    {
        float time = 0f;
        Vector3 direction = end - start;
        Vector3 mid = (start + end) * 0.5f;

        // Calculate curve offset
        Vector3 right = Vector3.Cross(Vector3.up, direction.normalized);
        mid += right * curveStrength;

        while (time < travelDuration)
        {
            if (token.IsCancellationRequested) return;
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / travelDuration);

            // Quadratic Bezier curve
            Vector3 a = Vector3.Lerp(start, mid, t);
            Vector3 b = Vector3.Lerp(mid, end, t);
            transform.position = Vector3.Lerp(a, b, t);

            await Task.Yield();
        }

        transform.position = end;
        DestroyBullet();
    }

    public float CurveStrength => curveStrength;
}
