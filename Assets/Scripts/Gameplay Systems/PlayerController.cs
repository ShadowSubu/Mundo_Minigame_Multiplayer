using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    private string playerAreaTag;
    private Camera playerCamera;
    private LayerMask allowedLayer;
    private NavMeshAgent navMeshAgent;
    private int allowedNavmeshArea;

    public event EventHandler OnCooldownChanged;

    private void Awake()
    {
        playerCamera = Camera.main;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        HandleMovement();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeRpc(GameManager.Team team, string projectileType, string abilityType)
    {
        Debug.Log($"Initializing PlayerController for team: {team}, Projectile: {projectileType}, Ability: {abilityType}");
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

    private bool IsCorrectNavmeshArea(Vector3 position)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(position, out navHit, 1.0f, AllowedNavmeshArea))
        {
            return true;
        }
        return false;
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
}
