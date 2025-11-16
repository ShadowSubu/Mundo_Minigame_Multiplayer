using System;
using UnityEngine;
using UnityEngine.AI;

public class AbilitySpeedBoost : AbilityBase
{
    [SerializeField] private float boostedSpeed = 25f;
    [SerializeField] private float duration = 5f;
    private float defaulSpeed;

    private void Start()
    {
        if (DeveloperDashboard.Instance.OverrideValues)
        {
            maxCooldown = DeveloperDashboard.Instance.GetSpeedBoostCooldown();
            duration = DeveloperDashboard.Instance.GetSpeedBoostDuration();
        }
    }

    internal override void OnAbilityUse(Ray ray, GameManager.Team team)
    {
        BoostSpeed();
    }

    private void BoostSpeed()
    {
        NavMeshAgent agent = casterObject.GetComponent<NavMeshAgent>();
        defaulSpeed = agent.speed;
        agent.speed = boostedSpeed;
        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        casterObject.GetComponent<NavMeshAgent>().speed = defaulSpeed;
    }

    public float SpeedBoostDuration => duration;
}
