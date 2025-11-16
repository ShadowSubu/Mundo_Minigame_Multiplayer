using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private List<PlayerController> currentPlayers = new();

    public event EventHandler<SpawnPlayerEventArgs> OnSpawnPlayer;
    public event EventHandler<List<PlayerController>> OnAllPlayersSpawned;
    public class SpawnPlayerEventArgs : EventArgs
    {
        public Team PlayerTeam;
        public ulong clientId;
        public string projectileType;
        public string abilityType;
    }

    public event EventHandler<Team> OnGameOver;

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

        CheckForAllPLayerSpawned();
    }

    private async void CheckForAllPLayerSpawned()
    {
        while (currentPlayers.Count < NetworkManager.Singleton.ConnectedClients.Count)
        {
            Debug.Log("Waiting for all the clients to connect");
            await Task.Delay(200);
        }
        Debug.Log("All the clients connected : "+ currentPlayers.Count);
        OnAllPlayersSpawned?.Invoke(this, CurrentPlayers);
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

        ulong[] clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
        int totalPlayers = clientIds.Length;

        // Log connected clients
        for (int i = 0; i < totalPlayers; i++)
        {
            Debug.Log($"Client {i}: {clientIds[i]}");
        }

        // Shuffle the client IDs randomly
        var shuffledClients = clientIds.OrderBy(_ => UnityEngine.Random.value).ToArray();

        // Determine players per team based on total players
        int playersPerTeam = totalPlayers / 2;

        // Assign first half to Team A, second half to Team B
        for (int i = 0; i < totalPlayers; i++)
        {
            Team assignedTeam = i < playersPerTeam ? Team.A : Team.B;
            AssignTeamRpc(shuffledClients[i], assignedTeam);
            Debug.Log($"Assigned Client {shuffledClients[i]} to {assignedTeam}");
        }
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

    [Rpc(SendTo.Server)]
    public void CheckGameOverRpc()
    {
        List<TargetPlayer> teamAPlayers = new();
        List<TargetPlayer> teamBPlayers = new();

        for (int i = 0; i < CurrentPlayers.Count; i++)
        {
            if (CurrentPlayers[i].PlayerTeam == Team.A)
            {
                teamAPlayers.Add(CurrentPlayers[i].GetComponent<TargetPlayer>());
            }
            else if (CurrentPlayers[i].PlayerTeam == Team.B)
            {
                teamBPlayers.Add(CurrentPlayers[i].GetComponent<TargetPlayer>());
            }
        }

        int defeatedAPLayers = 0;
        for (int i = 0; i < teamAPlayers.Count; i++)
        {
            if (teamAPlayers[i].GetComponent<TargetPlayer>().GetCurrentHealth() <= 0)
            {
                defeatedAPLayers++;
            }
        }
        if (defeatedAPLayers >= teamAPlayers.Count())
        {
            // All players in this team are dead
            OnGameOver?.Invoke(this, Team.B);
            return;
        }

        int defeatedBPLayers = 0;
        for (int i = 0; i < teamBPlayers.Count; i++)
        {
            if (teamBPlayers[i].GetComponent<TargetPlayer>().GetCurrentHealth() <= 0)
            {
                defeatedBPLayers++;
            }
        }
        if (defeatedBPLayers >= teamBPlayers.Count())
        {
            // All players in this team are dead
            OnGameOver?.Invoke(this, Team.A);
        }
    }

    public void RestartGame()
    {
        switch (LobbyManager.Instance.gameMode)
        {
            case GameMode.oneVone:
                if (NetworkManager.Singleton.ConnectedClients.Count == 2)
                {
                    SceneLoadingManager.Instance.LoadSceneAsync("Game 1v1");
                }
                break;
            case GameMode.twoVtwo:
                if (NetworkManager.Singleton.ConnectedClients.Count == 4)
                {
                    SceneLoadingManager.Instance.LoadSceneAsync("Game 2v2");
                }
                break;
            default:
                break;
        }
    }

    public void ReturnToLobby()
    {
        NetworkManager.Singleton.Shutdown();
        if (!IsServer)
        {
            SceneManager.LoadSceneAsync("Lobby");
        }
        SceneLoadingManager.Instance.LoadSceneAsync("Lobby");
    }

    public Team GetLocalPlayerTeam()
    {
        return localPlayerTeam;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AddPlayerRpc(ulong clientId)
    {
        WaitForPlayerAndAdd(clientId);
        PlayerController player = GetNetworkObject<PlayerController>(clientId);
        if (player != null)
        {
            if (!currentPlayers.Contains(player))
            {
                currentPlayers.Add(player);
            }
        }
        else
        {
            Debug.Log("Did not find any player with client id : " +  clientId);
        }
    }

    private async void WaitForPlayerAndAdd(ulong clientId)
    {
        PlayerController player = null;
        while (player == null)
        {
            player = GetNetworkObject<PlayerController>(clientId);

            if (player == null)
            {
                await Task.Delay(100);
            }
        }
        if (player != null)
        {
            if (!currentPlayers.Contains(player))
            {
                currentPlayers.Add(player);
                Debug.Log($"Successfully added player with client id: {clientId}. Total players: {currentPlayers.Count}");
            }
        }
        else
        {
            Debug.LogError($"Failed to find player with client id: {clientId}");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RemovePlayerRpc(ulong clientId)
    {
        PlayerController player = GetNetworkObject<PlayerController>(clientId);
        if (player != null)
        {
            if (currentPlayers.Contains(player))
            {
                currentPlayers.Remove(player);
            }
        }
        else
        {
            Debug.Log("Did not find any player with client id : " + clientId);
        }
    }

    public List<PlayerController> CurrentPlayers => currentPlayers;

    public static T GetNetworkObject<T>(ulong clientId) where T : NetworkBehaviour
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager is not active");
            return null;
        }

        // Get the player object for this client
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            if (client.PlayerObject != null)
            {
                return client.PlayerObject.GetComponent<T>();
            }
        }

        return null;
    }

    #region Testing

    private void SpawnTestSetup()
    {
        localPlayerTeam = Team.A;
        OnSpawnPlayerTesting?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
