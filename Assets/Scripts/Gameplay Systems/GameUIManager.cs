using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [Header("Arena Heal Countdown")]
    [SerializeField] private GameAreaBehaviour gameAreaBehaviour;
    [SerializeField] private TextMeshProUGUI arenaHealCountdownText;

    private void Start()
    {
        arenaHealCountdownText.text = "";
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
}
