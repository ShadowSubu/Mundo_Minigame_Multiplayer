using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NullReferenceDetection;
using Unity.Netcode;
using UnityEngine;

public class GameAreaBehaviour : NetworkBehaviour
{
    [SerializeField] private List<Transform> teamASpawnPoints;
    [SerializeField] private List<Transform> teamBSpawnPoints;
    [SerializeField] private Transform teamACameraLocation;
    [SerializeField] private Transform teamBCameraLocation;
    private List<Transform> availableA = new();
    private List<Transform> availableB = new();

    [SerializeField] private PlayerController playerControllerPrefab;

    [Header("Arena Heal")]
    [SerializeField] private ArenaHeal healCollider;
    [Tooltip("VFX Controller for heal arena"), SerializeField, ValueRequired] private Healing_Area_FX.Controller healVFX;
    [SerializeField] private float healDropDuration = 1f;
    [SerializeField] private int healCountdown = 5;
    [SerializeField] private float healCooldown = 10f;
    private CancellationTokenSource healdropCancellationTokenSource;
    public event EventHandler<int> OnHealCountdown;

    private void Awake()
    {
        availableA.AddRange(teamASpawnPoints);
        availableB.AddRange(teamBSpawnPoints);
    }

    private void Start()
    {
        GameManager.Instance.OnSpawnPlayer += GameManager_OnSpawnPlayer;
        Debug.Log("GameAreaBehaviour started and subscribed to GameManager.OnSpawnPlayer event.");

        StartHealDropCycle();

        #region Testing

        GameManager.Instance.OnSpawnPlayerTesting += GameManager_SpawnTestPlayer;

        #endregion
    }

    private void GameManager_OnSpawnPlayer(object sender, GameManager.SpawnPlayerEventArgs e)
    {
        Debug.Log($"GameManager_OnSpawnPlayer: Player Team = {e.PlayerTeam}, Client ID = {e.clientId}, Selected Projectile = {e.projectileType}, Selected Ability = {e.abilityType}");

        //if (NetworkManager.Singleton.LocalClientId != e.clientId) return;

        switch (e.PlayerTeam)
        {
            case GameManager.Team.None:
                break;
            case GameManager.Team.A:
                Debug.Log("Spawning Player in Team A");
                SpawnNetworkPlayerRpc(GetSpawnPoint(e.PlayerTeam), teamACameraLocation, e.clientId, e.PlayerTeam, e.projectileType, e.abilityType);
                SetCameraRpc(teamACameraLocation.position, teamACameraLocation.rotation, e.clientId);
                break;
            case GameManager.Team.B:
                Debug.Log("Spawning Player in Team B");
                SpawnNetworkPlayerRpc(GetSpawnPoint(e.PlayerTeam), teamBCameraLocation, e.clientId, e.PlayerTeam, e.projectileType, e.abilityType);
                SetCameraRpc(teamBCameraLocation.position, teamBCameraLocation.rotation, e.clientId);
                break;
            default:
                break;
        }
    }

    private Transform GetSpawnPoint(GameManager.Team team)
    {
        List<Transform> list = team == GameManager.Team.A ? availableA : availableB;
        if (list.Count == 0)
        {
            Debug.LogWarning($"No available spawn points for team {team}");
            return null;
        }

        Transform spawn = list[0];
        list.Remove(spawn);
        return spawn;
    }

    //[Rpc(SendTo.Server)]
    private void SpawnNetworkPlayerRpc(Transform playerPosition, Transform cameraPostion, ulong clientId, GameManager.Team team, string projectileType, string abilityType)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        PlayerController playerController = Instantiate(playerControllerPrefab, playerPosition.position , playerPosition.rotation);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        playerController.InitializeRpc(team, projectileType, abilityType);
    }

    //[Rpc(SendTo.ClientsAndHost)]
    private void SetCameraRpc(Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Camera.main.transform.SetPositionAndRotation(position, rotation);
    }

    #region Arena Heal

    private void StartHealDropCycle()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        healdropCancellationTokenSource = new CancellationTokenSource();
        HealDropCycle(healdropCancellationTokenSource.Token);
    }

    private void StopHealDropCycle()
    {
        healdropCancellationTokenSource?.Cancel();
        healdropCancellationTokenSource?.Dispose();
    }

    private async void HealDropCycle(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(healCooldown), cancellationToken);
                
                // Added by Om - 8.35pm - 22.11.25
                StartCoroutine(healVFX.PlayVisual(healCountdown));

                healCollider.gameObject.SetActive(false);

                int countdown = healCountdown;

                for (int i = 0; i < healCountdown; i++)
                {
                    OnHealCountdown?.Invoke(this, countdown);
                    await Task.Delay(1000, cancellationToken);
                    countdown--;
                }
                OnHealCountdown?.Invoke(this, 0);

                ToggleArenaHealRpc(true);
                
                await Task.Delay((int)(healDropDuration * 1000), cancellationToken);

                ToggleArenaHealRpc(false);
                
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ToggleArenaHealRpc(bool value)
    {
        Debug.Log("Heal collider state : " + value);
        healCollider.gameObject.SetActive(value);
    }

    public float HealCooldown => healCooldown;

    #endregion

    #region Testing

    private void GameManager_SpawnTestPlayer(object sender, EventArgs e)
    {
        PlayerController playerController = Instantiate(playerControllerPrefab, teamASpawnPoints[0].position, teamASpawnPoints[0].rotation);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        playerController.InitializeRpc(GameManager.Team.A);
        Camera.main.transform.SetPositionAndRotation(teamACameraLocation.position, teamACameraLocation.rotation);

        PlayerController dummyEnemy = Instantiate(playerControllerPrefab, teamBSpawnPoints[0].position, teamBSpawnPoints[0].rotation);
        dummyEnemy.InitializeRpc(GameManager.Team.B);
    }

    public void SetArenaHealCooldown(float cooldown)
    {
        healCooldown = cooldown;
    }

    #endregion
}
