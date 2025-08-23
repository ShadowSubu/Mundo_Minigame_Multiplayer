using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PublicLobbySingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI numberOfPlayersText;
    [SerializeField] private Button joinLobbyButton;

    private Lobby lobby;

    public void Initialize(Lobby lobby, Action callback)
    {
        joinLobbyButton.onClick.RemoveAllListeners();
        this.lobby = lobby;
        UpdateUI();
        joinLobbyButton.onClick.AddListener(() =>
        {
            callback?.Invoke();
        });
    }

    private void UpdateUI()
    {
        if (lobby == null) return;
        lobbyNameText.text = lobby.Name;
        numberOfPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        if (lobby.Players.Count >= lobby.MaxPlayers)
        {
            joinLobbyButton.interactable = false;
        }
        else
        {
            joinLobbyButton.interactable = true;
        }
    }
}
