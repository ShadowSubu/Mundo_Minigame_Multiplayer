using System;
using Unity.Netcode;

public class ChatSystem : NetworkBehaviour
{
    public static ChatSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler<ChatMessageEventArgs> OnChatInputRecieved;

    public class ChatMessageEventArgs : EventArgs
    {
        public ulong senderCliendId;
        public char character;
    }

    //[Rpc(SendTo.Server)]
    //public void RequestToSendChatInputRpc(string message, ulong clientId)
    //{
    //    if (!IsServer) return;
    //    SendChatInputRpc(message, clientId);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //public void SendChatInputRpc(string message, ulong clientId)
    //{
    //    OnChatInputRecieved?.Invoke(this, new ChatMessageEventArgs
    //    {
    //        senderCliendId = clientId,
    //        character = message
    //    });
    //}

    [Rpc(SendTo.Server)]
    public void RequestToSendChatInputRpc(char character, ulong clientId)
    {
        if (!IsServer) return;
        SendChatInputRpc(character, clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendChatInputRpc(char character, ulong clientId)
    {
        OnChatInputRecieved?.Invoke(this, new ChatMessageEventArgs
        {
            senderCliendId = clientId,
            character = character
        });
    }

    //[Rpc(SendTo.Server)]
    //public void RequestToSendChatMessageRpc(string playerName, string message)
    //{
    //    if (!IsServer) return;

    //    SendChatMessageRpc(playerName, message);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //private void SendChatMessageRpc(string playerName, string message)
    //{
    //    OnChatMessageEntered?.Invoke(this, new ChatMessageEventArgs
    //    {
    //        playerName = playerName,
    //        message = message
    //    });
    //}
}
