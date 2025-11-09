using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RestartGameUI : NetworkBehaviour
{
    [SerializeField] private GameObject bg;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToLobbyButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private void OnEnable()
    {
        restartButton.onClick.AddListener(RestartGame);
        returnToLobbyButton.onClick.AddListener(ReturnToLobby);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(RestartGame);
        returnToLobbyButton.onClick.RemoveListener(ReturnToLobby);
    }

    private void Start()
    {
        GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver(object sender, GameManager.Team winner)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ShowGameOverScreenRpc(winner);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowGameOverScreenRpc(GameManager.Team winnerTeam)
    {
        if (GameManager.Instance.GetLocalPlayerTeam() == winnerTeam)
        {
            gameOverText.text = "You Won!!";
        }
        else
        {
            gameOverText.text = "You Lost";
        }

        if (IsServer)
        {
            restartButton.gameObject.SetActive(true);
        }

        bg.gameObject.SetActive(true);
        returnToLobbyButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        restartButton.gameObject.SetActive(false);
        returnToLobbyButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        bg.gameObject.SetActive(false);
    }

    private void RestartGame()
    {
        GameManager.Instance.RestartGame();
        Hide();
    }

    private void ReturnToLobby()
    {
        GameManager.Instance.ReturnToLobby();
    }
}
