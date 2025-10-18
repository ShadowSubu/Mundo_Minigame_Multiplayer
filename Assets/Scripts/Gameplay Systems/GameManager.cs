using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

    public event EventHandler<SpawnPlayerEventArgs> OnSpawnPlayer;
    public class SpawnPlayerEventArgs : EventArgs
    {
        public Team PlayerTeam;
        public ulong clientId;
        public string projectileType;
        public string abilityType;
    }

    public event EventHandler OnGameOver;

    private Team localPlayerTeam;

    #region Testing

    public event EventHandler OnSpawnPlayerTesting;

    #endregion

    public enum Team
    {
        None,
        A,
        B
    }

    private async void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            RequestSpawnPlayer();
        }
        else
        {
            bool started = NetworkManager.Singleton.StartHost();

            // Wait until we're actually connected
            while (NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsConnectedClient)
            {
                await Task.Yield(); // Wait one frame
            }
            SpawnTestSetup();
        }
    }

    public async void RequestSpawnPlayer()
    {
        if (IsServer)
        {
            await Task.Delay(1000);
            ShuffleTeam();
        }
    }

    private void ShuffleTeam()
    {
        Debug.Log("Shuffling Teams...");
        Team[] teams = { Team.A, Team.B };
        var shuffled = teams.OrderBy(_ => UnityEngine.Random.value).ToArray();

        ulong[] clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
        for (int i = 0; i < clientIds.Length; i++)
        {
            // print clients
            Debug.Log($"Client {i}: {clientIds[i]}");
        }

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
        }
        
        var projectileData = LobbyManager.Instance.GetPlayerProjectileSelections(LobbyManager.Instance.GetPlayerIdFromClientId(clientId));
        Debug.Log($"Projectile Data for Client {clientId}: {(projectileData != null ? projectileData.projectileType : "null")}");
        string projectileType;
        if (projectileData != null)
        {
            projectileType = projectileData.projectileType;
        }
        else
        {
            projectileType = "Bullet";
        }
        var abilityData = LobbyManager.Instance.GetPlayerAbilitySelections(LobbyManager.Instance.GetPlayerIdFromClientId(clientId));
        Debug.Log($"Ability Data for Client {clientId}: {(abilityData != null ? abilityData.abilityType : "null")}");
        string abilityType;
        if (abilityData != null)
        {
            abilityType = abilityData.abilityType;
        }
        else
        {
            abilityType = "Blink";
        }

        OnSpawnPlayer?.Invoke(this, new SpawnPlayerEventArgs { PlayerTeam = team, clientId = clientId, projectileType = projectileType, abilityType = abilityType });
        //TriggerSpawnPlayerRpc(team, clientId);
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    public void RestartGame()
    {
        SceneLoadingManager.Instance.LoadSceneAsync("Game");
    }

    public Team GetLocalPlayerTeam()
    {
        return localPlayerTeam;
    }

    #region Testing

    private void SpawnTestSetup()
    {
        localPlayerTeam = Team.A;
        OnSpawnPlayerTesting?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
