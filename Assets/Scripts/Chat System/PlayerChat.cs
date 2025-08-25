using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerChat : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerChatText;
    [SerializeField] private TMP_InputField playerChatInputField;
    [SerializeField] private GameObject chatPanel;

    [SerializeField] private float chatDisplayDuration = 5f;
    private CancellationTokenSource chatInactivityCancellationToken;
    private bool isChatInputOpen = false;

    private void Awake()
    {
        playerChatInputField.onValueChanged.AddListener(OnChatInput);
        playerChatInputField.onDeselect.AddListener(OnChatDeselect);
    }

    private void OnChatDeselect(string arg0)
    {
        StopInputFieldDeselection();
    }

    private async void StopInputFieldDeselection()
    {
        await Task.Yield();
        playerChatInputField.Select();
    }

    private void Start()
    {
        ChatSystem.Instance.OnChatInputRecieved += ChatSystem_OnChatInputRecieved;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isChatInputOpen)
            {
                isChatInputOpen = true;
                OpenChatInput();
            }
            else
            {
                CloseChatInput();
            }
        }
    }

    private void OpenChatInput()
    {
        chatPanel.SetActive(true);
        playerChatInputField.text = string.Empty;
        playerChatInputField.gameObject.SetActive(true);

        // This ensures the input field is focused and ready for typing without clicking on it
        playerChatInputField.Select();

        StartChatInactivityTimer();
    }

    private void CloseChatInput()
    {
        StartChatInactivityTimer();
        playerChatInputField.DeactivateInputField();
    }

    private void OnChatInput(string text)
    {
        StartChatInactivityTimer();

        ChatSystem.Instance.RequestToSendChatInputRpc(text, NetworkManager.Singleton.LocalClientId);
    }

    private void DisplayChatMessage(string message, ulong clientId)
    {
        Debug.Log($"Received chat message from Client {clientId}: {message}");
        if (OwnerClientId != clientId)
        {
            // Handle Local Player Chat Display
        }
        else
        {
            playerChatText.text = message;
            playerChatText.gameObject.SetActive(true);
            chatPanel.SetActive(true);
            StartChatInactivityTimer();
        }
    }

    private void ChatSystem_OnChatInputRecieved(object sender, ChatSystem.ChatMessageEventArgs e)
    {
        if (NetworkManager.Singleton.LocalClientId == e.senderCliendId) return;
        DisplayChatMessage(e.message, e.senderCliendId);
    }

    private void DisableChat()
    {
        playerChatText.gameObject.SetActive(false);
        playerChatInputField.gameObject.SetActive(false);
        playerChatInputField.text = string.Empty;
        chatPanel.SetActive(false);
        isChatInputOpen = false;
    }

    #region Chat Inactivity Timer

    private void StartChatInactivityTimer()
    {
        CancelInActivityTimer();
        chatInactivityCancellationToken = new CancellationTokenSource();
       _ = DisableAfterInactivityAsync(chatInactivityCancellationToken.Token);
    }

    private async Task DisableAfterInactivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(chatDisplayDuration), cancellationToken);

            DisableChat();
        }
        catch (OperationCanceledException ex)
        {
            Debug.Log($"Chat inactivity timer cancelled: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.Log($"Error in chat inactivity timer: {ex.Message}");
        }
    }

    private void CancelInActivityTimer()
    {
        chatInactivityCancellationToken?.Cancel();
        chatInactivityCancellationToken?.Dispose();
        chatInactivityCancellationToken = null;
    }

    #endregion
}
