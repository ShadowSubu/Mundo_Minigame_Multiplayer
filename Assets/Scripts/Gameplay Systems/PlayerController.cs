using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    private bool externalRotationApplied = false;
    private List<Transform> enemies = new List<Transform>();

    private GameManager.Team playerTeam;
    private string playerAreaTag;
    private Camera playerCamera;
    private LayerMask allowedLayer;
    private NavMeshAgent navMeshAgent;
    private int allowedNavmeshArea;

    public event EventHandler OnCooldownChanged;
    public event EventHandler<float> OnPlayerMove;

    private Shooter shooter;
    private Transform targetToFace;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.AddPlayerRpc(this.OwnerClientId);
    }

    private void Awake()
    {
        playerCamera = Camera.main;
        navMeshAgent = GetComponent<NavMeshAgent>();
        shooter = GetComponent<Shooter>();
        navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
        HandleMovement();
        FindNearestEnemy();
        FaceTheNearestEnemy();
        OnPlayerMove?.Invoke(this, navMeshAgent.velocity.sqrMagnitude);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeRpc(GameManager.Team team, string projectileType, string abilityType)
    {
        Debug.Log($"Initializing PlayerController for team: {team}, Projectile: {projectileType}, Ability: {abilityType}");
        playerTeam = team;
        switch (team)
        {
            case GameManager.Team.None:
                break;
            case GameManager.Team.A:
                playerAreaTag = "Team A";
                break;
            case GameManager.Team.B:
                playerAreaTag = "Team B";
                break;
            default:
                break;
        }

        SetProjectile(projectileType);
        SetAbility(abilityType);

        allowedLayer = LayerMask.GetMask(playerAreaTag);
        navMeshAgent.enabled = true;

        allowedNavmeshArea = GetNavmeshArea(playerAreaTag);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeRpc(GameManager.Team team)
    {
        Debug.Log($"Initializing PlayerController for team: {team}");
        switch (team)
        {
            case GameManager.Team.None:
                break;
            case GameManager.Team.A:
                playerAreaTag = "Team A";
                break;
            case GameManager.Team.B:
                playerAreaTag = "Team B";
                break;
            default:
                break;
        }

        allowedLayer = LayerMask.GetMask(playerAreaTag);
        navMeshAgent.enabled = true;

        allowedNavmeshArea = GetNavmeshArea(playerAreaTag);
    }

    private void SetProjectile(string projectileType)
    {
        Shooter shooter = GetComponent<Shooter>();
        shooter.SelectProjectile(projectileType);
    }

    private void SetAbility(string abilityType)
    {
        Caster caster = GetComponent<Caster>();
        caster.SelectAbility(abilityType);
    }

    private void HandleMovement()
    {
        if (!IsOwner) return;
        if (shooter.IsShooting) return;
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (IsCorrectNavmeshArea(hit.point))
                {
                    RequestMoveServerRpc(hit.point);
                }
            }
        }
    }

    private void FindNearestEnemy()
    {
        if(GameManager.Instance.CurrentPlayers.Count == 0) return;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < GameManager.Instance.CurrentPlayers.Count; i++)
        {
            if (GameManager.Instance.CurrentPlayers[i].PlayerTeam != PlayerTeam)
            {
                float distance = Vector3.Distance(transform.position, GameManager.Instance.CurrentPlayers[i].transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    targetToFace = GameManager.Instance.CurrentPlayers[i].transform;
                }
            }
        }
    }

    private void FaceTheNearestEnemy()
    {
        if(externalRotationApplied) return;
        if (targetToFace == null) return;
        Vector3 direction = targetToFace.position - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private bool IsCorrectNavmeshArea(Vector3 position)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(position, out navHit, 1.0f, AllowedNavmeshArea))
        {
            return true;
        }
        return false;
    }

    [Rpc(SendTo.Server)]
    public void RotateToDirectionRpc(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        externalRotationApplied = true;
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = targetRotation;
        externalRotationApplied = false;
    }

    private int GetNavmeshArea(string areaName)
    {
        int areaIndex = NavMesh.GetAreaFromName(areaName);

        if (areaIndex == -1)
        {
            Debug.LogError($"NavMesh area '{areaName}' not found! Make sure it exists in Navigation window.");
            return NavMesh.AllAreas;
        }

        // Convert area index to area mask (bit flag)
        return 1 << areaIndex;
    }

    [Rpc(SendTo.Server)]
    private void RequestMoveServerRpc(Vector3 targetPosition)
    {
        navMeshAgent.ResetPath();
        navMeshAgent.SetDestination(targetPosition);
    }

    public int AllowedNavmeshArea => allowedNavmeshArea;
    public LayerMask AllowedLayer => allowedLayer;
    public string PlayerAreaTag => playerAreaTag;
    public GameManager.Team PlayerTeam => playerTeam;

    #region Developer Dashboard values

    public void SetPlayerMovementSpeed(float value)
    {
        navMeshAgent.speed = value;
    }

    public void SetPlayerChannelDuration(int value)
    {
        shooter.SetChannelDuration(value);
    }

    #endregion
}
