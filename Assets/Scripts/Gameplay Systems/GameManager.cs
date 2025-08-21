using System;
using System.Linq;
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
        OnSpawnPlayer?.Invoke(this, new SpawnPlayerEventArgs { PlayerTeam = team, clientId = clientId});
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
