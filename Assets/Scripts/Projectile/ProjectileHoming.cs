using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class ProjectileHoming : ProjectileBase
{
    [SerializeField] private float maxDistance = 30f;
    [SerializeField, Range(0.001f, 0.01f)] private float turnSensitivity = 0.002f;
    private Camera mainCamera;
    [SerializeField] private LayerMask shootingLayer;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    internal override void OnTriggerEnterBehaviour(Collider other)
    {
        TargetBase hit = other.GetComponent<TargetBase>();
        Debug.Log($"Projectile hit: {other.name}, OwnerClientId: {hit?.OwnerClientId}");
        if (hit != null && hit.OwnerClientId != ShooterObject.OwnerClientId)
        {
            // Hit an opponent - destroy bullet
            hit.ReceiveDamageRpc(projectileDamage);
            DestroyBullet();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsOwner)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootingLayer))
            {
                Vector3 tempMoveDir = moveDirection;

                float difference = hit.point.x - moveDirection.x;
                tempMoveDir.x += difference * turnSensitivity;

                Debug.Log("Move direction: " + tempMoveDir);
                RequestMoveDirectionRpc(tempMoveDir.x);
            }
        }
    }

    internal override void ProjectileBehaviour()
    {
        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.position += moveDirection * projectileSpeed * Time.deltaTime;

        // Check if traveled max distance
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            DestroyBullet();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestMoveDirectionRpc(float moveDirectionX)
    {
        UpdateMoveDirectionRpc(moveDirectionX);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateMoveDirectionRpc(float moveDirectionX)
    {
        this.moveDirection.x = moveDirectionX;
    }

    public float MaxDistance => maxDistance;
}
