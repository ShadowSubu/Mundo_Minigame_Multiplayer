using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class GameAreaBehaviour : MonoBehaviour
{
    [SerializeField] private Transform teamASpawnPoint;
    [SerializeField] private Transform teamBSpawnPoint;
    [SerializeField] private Transform teamACameraLocation;
    [SerializeField] private Transform teamBCameraLocation;

    [SerializeField] private PlayerController playerControllerPrefab;

    private void Start()
    {
        GameManager.Instance.OnSpawnPlayer += GameManager_OnSpawnPlayer;
        Debug.Log("GameAreaBehaviour started and subscribed to GameManager.OnSpawnPlayer event.");

        #region Testing

        GameManager.Instance.OnSpawnPlayerTesting += GameManager_SpawnTestPlayer;

        #endregion
    }



    private void GameManager_OnSpawnPlayer(object sender, GameManager.SpawnPlayerEventArgs e)
    {
        Debug.Log($"GameManager_OnSpawnPlayer: Player Team = {e.PlayerTeam}, Client ID = {e.clientId}");

        //if (NetworkManager.Singleton.LocalClientId != e.clientId) return;

        switch (e.PlayerTeam)
        {
            case GameManager.Team.None:
                break;
            case GameManager.Team.A:
                Debug.Log("Spawning Player in Team A");
                SpawnNetworkPlayerRpc(teamASpawnPoint, teamACameraLocation, e.clientId, e.PlayerTeam);
                SetCameraRpc(teamACameraLocation.position, teamACameraLocation.rotation, e.clientId);
                break;
            case GameManager.Team.B:
                Debug.Log("Spawning Player in Team B");
                SpawnNetworkPlayerRpc(teamBSpawnPoint, teamBCameraLocation, e.clientId, e.PlayerTeam);
                SetCameraRpc(teamBCameraLocation.position, teamBCameraLocation.rotation, e.clientId);
                break;
            default:
                break;
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnNetworkPlayerRpc(Transform playerPosition, Transform cameraPostion, ulong clientId, GameManager.Team team)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        PlayerController playerController = Instantiate(playerControllerPrefab, playerPosition.position , playerPosition.rotation);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        playerController.InitializeRpc(team);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetCameraRpc(Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        Camera.main.transform.SetPositionAndRotation(position, rotation);
    }

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
