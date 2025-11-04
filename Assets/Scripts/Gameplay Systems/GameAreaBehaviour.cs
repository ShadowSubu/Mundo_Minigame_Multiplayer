using DG.Tweening;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameAreaBehaviour : NetworkBehaviour
{
    [SerializeField] private Transform teamASpawnPoint;
    [SerializeField] private Transform teamBSpawnPoint;
    [SerializeField] private Transform teamACameraLocation;
    [SerializeField] private Transform teamBCameraLocation;

    [SerializeField] private PlayerController playerControllerPrefab;

    [Header("Arena Heal")]
    [SerializeField] private GameObject healCollider;
    [SerializeField] private float healDropDuration = 1f;
    [SerializeField] private int healCountdown = 5;
    [SerializeField] private float healCooldown = 10f;
    private CancellationTokenSource healdropCancellationTokenSource;
    public event EventHandler<int> OnHealCountdown;

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
                SpawnNetworkPlayerRpc(teamASpawnPoint, teamACameraLocation, e.clientId, e.PlayerTeam, e.projectileType, e.abilityType);
                SetCameraRpc(teamACameraLocation.position, teamACameraLocation.rotation, e.clientId);
                break;
            case GameManager.Team.B:
                Debug.Log("Spawning Player in Team B");
                SpawnNetworkPlayerRpc(teamBSpawnPoint, teamBCameraLocation, e.clientId, e.PlayerTeam, e.projectileType, e.abilityType);
                SetCameraRpc(teamBCameraLocation.position, teamBCameraLocation.rotation, e.clientId);
                break;
            default:
                break;
        }
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
                healCollider.SetActive(false);

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
        healCollider.SetActive(value);
    }

    #endregion

    #region Testing

    private void GameManager_SpawnTestPlayer(object sender, EventArgs e)
    {
        PlayerController playerController = Instantiate(playerControllerPrefab, teamASpawnPoint.position, teamASpawnPoint.rotation);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        playerController.InitializeRpc(GameManager.Team.A);
        Camera.main.transform.SetPositionAndRotation(teamACameraLocation.position, teamACameraLocation.rotation);

        PlayerController dummyEnemy = Instantiate(playerControllerPrefab, teamBSpawnPoint.position, teamBSpawnPoint.rotation);
        dummyEnemy.InitializeRpc(GameManager.Team.B);
    }

    #endregion
}
