using System;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler OnSpawnPlayer;

    private Team localPlayerTeam;

    public enum Team
    {
        None,
        A,
        B
    }

    private void Start()
    {
        RequestSpawnPlayer();
    }

    public void RequestSpawnPlayer()
    {
        if (IsServer)
        {
            ShuffleTeam();
        }
    }

    private void ShuffleTeam()
    {
        Debug.Log("Shuffling Teams...");
        Team[] teams = { Team.A, Team.B };
        var shuffled = teams.OrderBy(_ => UnityEngine.Random.value).ToArray();

        ulong[] clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();

        AssignTeamRpc(clientIds[0], teams[0]);
        AssignTeamRpc(clientIds[1], teams[1]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AssignTeamRpc(ulong clientId, Team team)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            localPlayerTeam = team;
            Debug.Log($"{AuthenticationService.Instance.PlayerName} was Assigned to team: {team}");
            OnSpawnPlayer?.Invoke(this, EventArgs.Empty);
        }
    }
}
