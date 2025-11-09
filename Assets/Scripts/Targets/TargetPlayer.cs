using UnityEngine;

public class TargetPlayer : TargetBase
{
    internal override void OnHitpointsDepletedBehaviour()
    {
        GameManager.Instance.CheckGameOverRpc();
    }

    internal override void OnTriggerEnterBehaviour(Collider other)
    {

    }
}
