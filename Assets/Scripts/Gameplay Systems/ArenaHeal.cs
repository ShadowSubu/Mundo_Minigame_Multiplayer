using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaHeal : MonoBehaviour
{
    [SerializeField] private byte healAmount = 20;
    private void OnTriggerEnter(Collider other)
    {
        TargetBase hit = other.GetComponent<TargetBase>();
        if (hit != null)
        {
            hit.ReceiveHealRpc(healAmount);
        }
    }
}
