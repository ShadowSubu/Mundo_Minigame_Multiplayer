using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private float chatWindowOpenedHeight = 500f;
    [SerializeField] private float chatWindowClosedHeight = 50f;

    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatMessagePrefab;
    [SerializeField] private Transform chatContainer;
    [SerializeField] private int maxChatMessages = 20;
    private Queue<TextMeshProUGUI> chatMessages = new();

    private bool isChatOpen = false;
    private RectTransform thisRectTransform;

    private void Awake()
    {
        thisRectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        chatInputField.onSubmit.AddListener((message) => 
        {
            SendChatMessage(message);
        });
    }

    private void OnDisable()
    {
        chatInputField.onSubmit.RemoveAllListeners();
    }

    private void Start()
    {
        chatInputField.DeactivateInputField();
        thisRectTransform.sizeDelta = new Vector2(thisRectTransform.sizeDelta.x, chatWindowClosedHeight);

        ChatSystem.Instance.OnChatMessageEntered += ChatSystem_OnChatMessageEntered;
    }

    private void Update()
    {
        if (isChatOpen)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(thisRectTransform, Input.mousePosition, null))
                {
                    thisRectTransform.sizeDelta = new Vector2(thisRectTransform.sizeDelta.x, chatWindowClosedHeight);
                    isChatOpen = false;
                    chatInputField.DeactivateInputField();
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        thisRectTransform.sizeDelta = new Vector2(thisRectTransform.sizeDelta.x, chatWindowOpenedHeight);
        isChatOpen = true;
        chatInputField.ActivateInputField();
    }

    private void ChatSystem_OnChatMessageEntered(object sender, ChatSystem.ChatMessageEventArgs e)
    {
        if (chatMessages.Count > maxChatMessages)
        {
            TextMeshProUGUI oldestMessage = chatMessages.Dequeue();
            Destroy(oldestMessage.gameObject);
        }
        else
        {
            TextMeshProUGUI newMessage = Instantiate(chatMessagePrefab, chatContainer);
            newMessage.text = $"<b>{e.playerName}:</b> {e.message}";
            chatMessages.Enqueue(newMessage);
        }
    }

    private void SendChatMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ChatSystem.Instance.RequestToSendChatMessageRpc(AuthenticationService.Instance.PlayerName, message);
            chatInputField.text = string.Empty;
        }
    }
}
