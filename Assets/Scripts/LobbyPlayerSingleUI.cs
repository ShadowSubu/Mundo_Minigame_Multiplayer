using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button kickButton;

    public void Initialize(string playerName, bool isHost, Action onKickButtonClicked)
    {
        playerNameText.text = playerName;

        if (LobbyManager.Instance.IsLobbyHost())
        {
            kickButton.gameObject.SetActive(!isHost);
        }
        else
        {
            kickButton.gameObject.SetActive(false);
        }

            kickButton.onClick.RemoveAllListeners();
        kickButton.onClick.AddListener(() =>
        {
            onKickButtonClicked?.Invoke();
        });
    }
}
