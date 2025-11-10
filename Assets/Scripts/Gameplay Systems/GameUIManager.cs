using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [Header("Arena Heal Countdown")]
    [SerializeField] private GameAreaBehaviour gameAreaBehaviour;
    [SerializeField] private TextMeshProUGUI arenaHealCountdownText;

    [Header("Player")]
    [SerializeField] private PublicPlayerHealthIndicator publicPlayerHealthIndicatorPrefab;
    [SerializeField] private Transform playerHealthParent;

    private void Start()
    {
        arenaHealCountdownText.text = "";
        GameManager.Instance.OnAllPlayersSpawned += GameManager_OnAllPlayersSpawned;
    }

    private void GameManager_OnAllPlayersSpawned(object sender, List<PlayerController> players)
    {
        SpawnPlayerHealthUIRpc();
    }

    public override void OnNetworkSpawn()
    {
        gameAreaBehaviour.OnHealCountdown += GameAreaBehaviour_OnHealCountdown;
    }

    private void GameAreaBehaviour_OnHealCountdown(object sender, int countdown)
    {
        if (IsServer)
        {
            ArenaHealCountdownRpc((byte)countdown);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ArenaHealCountdownRpc(byte countdown)
    {
        if (countdown == 0)
        {
            arenaHealCountdownText.text = "";
        }
        else
        {
            arenaHealCountdownText.text = $"Heal Incoming In {countdown}";
        }
    }

    private void SpawnPlayerHealthUIRpc()
    {
        Debug.Log("Spawning Player health Prefabs");
        for (int i = 0; i < GameManager.Instance.CurrentPlayers.Count; i++)
        {
            PublicPlayerHealthIndicator item = Instantiate(publicPlayerHealthIndicatorPrefab, playerHealthParent);
            TargetPlayer targetPlayer = GameManager.Instance.CurrentPlayers[i].GetComponent<TargetPlayer>();
            item.Initialize(targetPlayer, targetPlayer.GetMaxHealth(), LobbyManager.Instance.GetPlayerNameFromClientId(GameManager.Instance.CurrentPlayers[i].OwnerClientId));
        }
    }
}
