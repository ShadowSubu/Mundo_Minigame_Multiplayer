using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class Bot : MonoBehaviour
{
    [Header("Movement Radius")]
    [Range(5f, 50f)]
    public float roamRadius = 15f;

    [Header("Wait Time At Point")]
    [Range(0f, 10f)]
    public float minWaitTime = 1f;

    [Range(0f, 10f)]
    public float maxWaitTime = 3f;

    [Header("Shoot Timing")]
    [Range(0.5f, 10f)]
    public float minShootInterval = 1.5f;

    [Range(0.5f, 10f)]
    public float maxShootInterval = 4f;

    [Range(0f, 10f)]
    public float shootCooldown = 1f;

    [Header("Ability Timing")]
    [Range(1f, 20f)]
    public float minAbilityInterval = 5f;

    [Range(1f, 20f)]
    public float maxAbilityInterval = 10f;

    [Range(0f, 20f)]
    public float abilityCooldown = 6f;

    private NavMeshAgent agent;
    private Shooter shooter;
    private Caster caster;
    private float nextShootTime;
    private float nextAbilityTime;
    private float nextMoveTime;

    public event EventHandler<float> OnBotMove;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        shooter = GetComponent<Shooter>();
        caster = GetComponent<Caster>();
    }

    void Start()
    {
        ScheduleNextShoot();
        ScheduleNextAbility();
        MoveToRandomPoint();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleAbility();
        OnBotMove?.Invoke(this, agent.velocity.sqrMagnitude);
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (Time.time >= nextMoveTime)
            {
                MoveToRandomPoint();
                ScheduleNextMove();
            }
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomPoint = GetRandomNavMeshPoint(transform.position, roamRadius);
        agent.SetDestination(randomPoint);
    }

    void ScheduleNextMove()
    {
        float waitTime = Random.Range(minWaitTime, Mathf.Max(minWaitTime, maxWaitTime));
        nextMoveTime = Time.time + waitTime;
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float distance)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * distance;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, distance, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }

    // ---------------- SHOOTING ----------------

    void HandleShooting()
    {
        if (Time.time >= nextShootTime)
        {
            Shoot();
            ScheduleNextShoot();
        }
    }

    void ScheduleNextShoot()
    {
        float interval = Random.Range(minShootInterval, Mathf.Max(minShootInterval, maxShootInterval));
        nextShootTime = Time.time + shootCooldown + interval;
    }

    // ---------------- ABILITY ----------------

    void HandleAbility()
    {
        if (Time.time >= nextAbilityTime)
        {
            UseAbility();
            ScheduleNextAbility();
        }
    }

    void ScheduleNextAbility()
    {
        float interval = Random.Range(minAbilityInterval, Mathf.Max(minAbilityInterval, maxAbilityInterval));
        nextAbilityTime = Time.time + abilityCooldown + interval;
    }

    // ---------------- YOUR HOOKS ----------------

    void Shoot()
    {
        // YOUR shoot logic here
        
    }

    void UseAbility()
    {
        // YOUR ability logic here
    }

#if UNITY_EDITOR
    // ---------------- VISUALIZATION ----------------

    void OnDrawGizmosSelected()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, roamRadius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
#endif
}
