using System;
using Unity.Netcode;
using UnityEngine;

public class ChatSystem : NetworkBehaviour
{
    public static ChatSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler<ChatMessageEventArgs> OnChatMessageEntered;

    public class ChatMessageEventArgs : EventArgs
    {
        public string playerName;
        public string message;
    }

    [Rpc(SendTo.Server)]
    public void RequestToSendChatMessageRpc(string playerName, string message)
    {
        if (!IsServer) return;

        SendChatMessageRpc(playerName, message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendChatMessageRpc(string playerName, string message)
    {
        OnChatMessageEntered?.Invoke(this, new ChatMessageEventArgs
        {
            playerName = playerName,
            message = message
        });
    }
}
