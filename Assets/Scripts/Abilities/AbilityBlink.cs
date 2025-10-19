using UnityEngine;
using UnityEngine.AI;

public class AbilityBlink : AbilityBase
{
    [Header("Blink Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float blinkRadius = 10f;

    internal override void OnAbilityUse(Ray ray)
    {
        Blink(ray);
    }

    private void Blink(Ray ray)
    {
        Vector3 targetPosition;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = hit.point;
        }
        else
        {
            Debug.Log("Raycast did not hit the ground layer.");
            targetPosition = Vector3.zero;
        }
        targetPosition = GetValidBlinkPosition(hit.point);
        Debug.Log("Target Position: " + targetPosition);
        casterObject.GetComponent<NavMeshAgent>().Warp(targetPosition);
    }

    /// <summary>
    /// This function checks if the target position is inside the blink radius and on a valid NavMesh area.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    Vector3 GetValidBlinkPosition(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        int allowedNavmeshArea = casterObject.GetComponent<PlayerController>().AllowedNavmeshArea;

        // Check if target is within blink radius, If target is outside radius, clamp it to the radius boundary
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        Vector3 clampedPosition = targetPosition;
        if (distanceToTarget > blinkRadius)
        {
            Vector3 direction = (targetPosition - currentPosition).normalized;
            clampedPosition = currentPosition + direction * blinkRadius;
        }

        // Find the closest valid NavMesh position, If no valid position found, try to find any valid position within radius
        NavMeshHit hit;
        if (NavMesh.SamplePosition(clampedPosition, out hit, blinkRadius, allowedNavmeshArea))
        {
            return hit.position;
        }
        for (float radius = blinkRadius; radius > 0.5f; radius -= 0.5f)
        {
            if (NavMesh.SamplePosition(clampedPosition, out hit, radius, allowedNavmeshArea))
            {
                return hit.position;
            }
        }

        Debug.Log("No valid blink position found!");
        return Vector3.zero;
    }

    public float BlinkRadius => blinkRadius;
}
