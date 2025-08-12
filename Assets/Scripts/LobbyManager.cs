using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    #region Constants

    public const int MAX_PLAYERS = 2;
    public const string KEY_PLAYER_NAME = "PlayerName";

    #endregion

    #region Lobby Events

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
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

    [SerializeField] float lobbyHeartbeatTimeMax = 15f;
    [SerializeField] float lobbyPollTimerMax = 2f;
    [SerializeField] float publicLobbyListRefreshTimerMax = 5f;
    private Lobby currentLobby;

    private float lobbyHeartbeatTime;
    private float lobbyPollTimer;
    private float publicLobbyListRefreshTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        lobbyHeartbeatTime = lobbyHeartbeatTimeMax;
        lobbyPollTimer = lobbyPollTimerMax;
        publicLobbyListRefreshTimer = publicLobbyListRefreshTimerMax;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
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
            }
        }
    }

    public Lobby GetCurrentLobby()
    {
        return currentLobby;
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

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate
        };

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, options);
            currentLobby = lobby;

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
}
