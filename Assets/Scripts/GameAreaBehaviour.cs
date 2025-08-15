using Unity.Netcode;
using UnityEngine;

public class GameAreaBehaviour : MonoBehaviour
{
    [SerializeField] private Transform teamASpawnPoint;
    [SerializeField] private Transform teamBSpawnPoint;
    [SerializeField] private Transform teamACameraPosition;
    [SerializeField] private Transform teamBCameraPosition;

    [SerializeField] private PlayerController playerControllerPrefab;

    private void Start()
    {
        GameManager.Instance.OnSpawnPlayer += GameManager_OnSpawnPlayer;
        Debug.Log("GameAreaBehaviour started and subscribed to GameManager.OnSpawnPlayer event.");
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
                SpawnNetworkPlayerRpc(teamASpawnPoint, teamACameraPosition, e.clientId, e.PlayerTeam);
                SetCameraRpc(teamACameraPosition.position, teamACameraPosition.rotation, e.clientId);
                break;
            case GameManager.Team.B:
                Debug.Log("Spawning Player in Team B");
                SpawnNetworkPlayerRpc(teamBSpawnPoint, teamBCameraPosition, e.clientId, e.PlayerTeam);
                SetCameraRpc(teamBCameraPosition.position, teamBCameraPosition.rotation, e.clientId);
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
}
