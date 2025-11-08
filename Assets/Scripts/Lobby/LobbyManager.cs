using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    #region Constants

    public int MAX_PLAYERS = 2;
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_START_GAME = "StartGame_RelayCode";
    private const string PROJECTILE_SELECTION_KEY_PREFIX = "PlayerProjectileSelection_";
    private const string ABILITY_SELECTION_KEY_PREFIX = "PlayerAbilitySelection_";
    private const string CLIENT_ID_PREFIX = "ClientId_";

    #endregion

    #region Lobby Events

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnPlayerLoadoutSelection;
    public event EventHandler<string> OnServiceError;

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<PublicLobbyListChangedEventArgs> OnPublicLobbyListChanged;
    public class PublicLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> Lobbies;
    }

    #endregion

    [SerializeField, Tooltip("Interval to sent a heartbeat ping, to keep the lobby alive")] 
    float lobbyHeartbeatTimeMax = 15f;
    [SerializeField, Tooltip("Interval to refresh lobby data of the current lobby")] 
    float lobbyPollTimerMax = 2f;
    [SerializeField, Tooltip("Interval to refresh the public lobby list")] 
    float publicLobbyListRefreshTimerMax = 5f;
    private Lobby currentLobby;

    private float lobbyHeartbeatTime;
    private float lobbyPollTimer;
    private float publicLobbyListRefreshTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        lobbyHeartbeatTime = lobbyHeartbeatTimeMax;
        lobbyPollTimer = lobbyPollTimerMax;
        publicLobbyListRefreshTimer = publicLobbyListRefreshTimerMax;
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SaveClientIdMapping(clientId);
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            lobbyHeartbeatTime -= Time.deltaTime;
            if (lobbyHeartbeatTime < 0f)
            {
                lobbyHeartbeatTime = lobbyHeartbeatTimeMax;

                Debug.Log("Heartbeat");
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                }
                catch (LobbyServiceException ex)
                {
                    OnServiceError?.Invoke(this, ex.Message);
                }
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (currentLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;

                try
                {
                    currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                }
                catch (LobbyServiceException ex)
                {
                    OnServiceError?.Invoke(this, ex.Message);
                    return;
                }

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = currentLobby });

                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = currentLobby });

                    currentLobby = null;
                }

                if (currentLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost() && !NetworkManager.Singleton.IsConnectedClient)
                    {
                        RelayManager.Instance.JoinRelayAsync(currentLobby.Data[KEY_START_GAME].Value);
                    }
                }
            }
        }
    }

    public Lobby GetCurrentLobby()
    {
        return currentLobby;
    }

    public bool IsLobbyReady()
    {
        return currentLobby != null && 
               currentLobby.Players != null && 
               currentLobby.Players.Count >= MAX_PLAYERS && 
               NetworkManager.Singleton.ConnectedClients.Count >= MAX_PLAYERS &&
               currentLobby.Data.ContainsKey(KEY_START_GAME) && 
               currentLobby.Data[KEY_START_GAME].Value != "0" &&
               AllClientsMapped() &&
               AllPlayersHaveSelections();
    }

    private bool AllClientsMapped()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (!GetCurrentLobby().Data.ContainsKey(CLIENT_ID_PREFIX + client.ClientId))
            {
                return false;
            }
        }
        return true;
    }

    private bool AllPlayersHaveSelections()
    {
        var lobby = GetCurrentLobby();
        if (lobby == null || lobby.Players == null) return false;

        foreach (var player in lobby.Players)
        {
            // Skip disconnected / null data players just in case
            if (player == null || player.Data == null)
                return false;

            string projKey = PROJECTILE_SELECTION_KEY_PREFIX + player.Id;
            string abilKey = ABILITY_SELECTION_KEY_PREFIX + player.Id;

            if (!player.Data.ContainsKey(projKey) || string.IsNullOrEmpty(player.Data[projKey].Value))
                return false;

            if (!player.Data.ContainsKey(abilKey) || string.IsNullOrEmpty(player.Data[abilKey].Value))
                return false;
        }

        return true;
    }

    public bool IsLobbyHost()
    {
        return currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (currentLobby != null && currentLobby.Players != null)
        {
            foreach (Player player in currentLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerName) }
        });
    }

    public async void CreateLobby(string lobbyName, bool isPrivate, GameMode gameMode)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
        };

        switch (gameMode)
        {
            case GameMode.oneVone:
                MAX_PLAYERS = 2;
                break;
            case GameMode.twoVtwo:
                MAX_PLAYERS = 4;
                break;
            default:
                break;
        }

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, options);
            currentLobby = lobby;

            StartConnectingGame();

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            Debug.Log("Created Lobby " + lobby.Name);
        }
        catch (LobbyServiceException ex)
        {
            OnServiceError?.Invoke(this, ex.Message);
            return;
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            OnPublicLobbyListChanged?.Invoke(this, new PublicLobbyListChangedEventArgs { Lobbies = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            OnServiceError?.Invoke(this, e.Message);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        Player player = GetPlayer();

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
            {
                Player = player
            });
            currentLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException ex)
        {
            OnServiceError?.Invoke(this, ex.Message);
            return;
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();

        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException ex)
        {
            OnServiceError?.Invoke(this, ex.Message);
        }
    }

    public async void LeaveLobby()
    {
        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);

                currentLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                OnServiceError?.Invoke(this, e.Message);
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                OnServiceError?.Invoke(this, e.Message);
            }
        }
    }

    #region ClientId Mapping

    public void SaveClientIdMapping(ulong clientId)
    {
        RequestSaveClientMappingRpc(clientId, AuthenticationService.Instance.PlayerId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestSaveClientMappingRpc(ulong clientId, string playerId)
    {
        SaveClientIdMappingHost(clientId, playerId);
    }

    private async void SaveClientIdMappingHost(ulong clientId, string playerId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        var lobby = GetCurrentLobby();
        if (lobby == null) return;

        string key = CLIENT_ID_PREFIX + clientId;

        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { key, new DataObject(DataObject.VisibilityOptions.Member, playerId) }
            }
            });

            Debug.Log($"Saved mapping: {key} -> {playerId}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to save clientId mapping: {ex.Message}");
        }
    }

    public string GetPlayerIdFromClientId(ulong clientId)
    {
        var lobby = GetCurrentLobby();
        if (lobby == null || lobby.Data == null) return null;

        string key = CLIENT_ID_PREFIX + clientId;
        if (lobby.Data.ContainsKey(key))
        {
            Debug.Log($"Found mapping: {key} -> {lobby.Data[key].Value}");
            return lobby.Data[key].Value;
        }

        Debug.LogWarning($"No mapping found for clientId: {clientId}");

        return null;
    }

    #endregion

    #region Loadout

    public async Task SavePlayerProjectileSelection(string projectileType)
    {
        var currentLobby = GetCurrentLobby();
        if (currentLobby == null) return;

        var selection = new PlayerProjectileSelectionData
        {
            playerId = AuthenticationService.Instance.PlayerId,
            projectileType = projectileType,
        };

        string selectionKey = PROJECTILE_SELECTION_KEY_PREFIX + AuthenticationService.Instance.PlayerId;

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                currentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { selectionKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, selection.ToJson()) }
                    }
                });
            OnPlayerLoadoutSelection?.Invoke(this, new LobbyEventArgs { lobby = currentLobby });
            Debug.Log($"Saved selection: {selection.ToJson()}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to save selection: {ex.Message}");
        }
    }

    public async Task SavePlayerAbilitySelection(string abilityType)
    {
        var currentLobby = GetCurrentLobby();
        if (currentLobby == null) return;

        var selection = new PlayerAbilitySelectionData
        {
            playerId = AuthenticationService.Instance.PlayerId,
            abilityType = abilityType,
        };

        string selectionKey = ABILITY_SELECTION_KEY_PREFIX + AuthenticationService.Instance.PlayerId;

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(
                currentLobby.Id,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { selectionKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, selection.ToJson()) }
                    }
                });
            OnPlayerLoadoutSelection?.Invoke(this, new LobbyEventArgs { lobby = currentLobby });
            Debug.Log($"Saved selection: {selection.ToJson()}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to save selection: {ex.Message}");
        }
    }

    public PlayerProjectileSelectionData GetPlayerProjectileSelections(string playerId)
    {
        if (GetCurrentLobby()?.Players == null) return null;

        foreach (var player in GetCurrentLobby().Players)
        {
            if (player.Id == playerId && player.Data != null)
            {
                string selectionKey = PROJECTILE_SELECTION_KEY_PREFIX + playerId;
                if (player.Data.ContainsKey(selectionKey))
                {
                    try
                    {
                        return PlayerProjectileSelectionData.FromJson(player.Data[selectionKey].Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse selection: {ex.Message}");
                        return null;
                    }
                }
            }
        }

        return null;
    }

    public PlayerAbilitySelectionData GetPlayerAbilitySelections(string playerId)
    {
        if (GetCurrentLobby()?.Players == null) return null;

        foreach (var player in GetCurrentLobby().Players)
        {
            if (player.Id == playerId && player.Data != null)
            {
                string selectionKey = ABILITY_SELECTION_KEY_PREFIX + playerId;
                if (player.Data.ContainsKey(selectionKey))
                {
                    try
                    {
                        return PlayerAbilitySelectionData.FromJson(player.Data[selectionKey].Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse selection: {ex.Message}");
                        return null;
                    }
                }
            }
        }

        return null;
    }

    #endregion

    #region Relay

    public async void StartConnectingGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Starting game...");
                string relayJoinCode = await RelayManager.Instance.CreateRelayAsync(MAX_PLAYERS);
                if (string.IsNullOrEmpty(relayJoinCode))
                {
                    OnServiceError?.Invoke(this, "Failed to create relay.");
                    return;
                }

                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                });
                currentLobby = lobby;
            }
            catch (RelayServiceException ex)
            {
                OnServiceError?.Invoke(this, ex.Message);
            }
        }
    }

    #endregion
}

[Serializable]
public class PlayerProjectileSelectionData
{
    public string playerId;
    public string projectileType;

    public string ToJson() => JsonConvert.SerializeObject(this);
    public static PlayerProjectileSelectionData FromJson(string json) => JsonConvert.DeserializeObject<PlayerProjectileSelectionData>(json);
}

[Serializable]
public class PlayerAbilitySelectionData
{
    public string playerId;
    public string abilityType;
    public string ToJson() => JsonConvert.SerializeObject(this);
    public static PlayerAbilitySelectionData FromJson(string json) => JsonConvert.DeserializeObject<PlayerAbilitySelectionData>(json);
}

public enum GameMode
{
    oneVone,
    twoVtwo
}