using System;
using NullReferenceDetection;
using UnityEngine;

namespace Chat_UI.Chatbox
{
    public class FollowPlayer : MonoBehaviour
    {
        // References
        [Tooltip("Overlay Canvas that this chatbox is going to render inside"), SerializeField, ValueRequired] Canvas canvas ;
        [Tooltip("Player Transform that needs to be followed"), SerializeField, ValueRequired] Transform playerTransform ;
        [Tooltip("Set to true when the chat window is open, and false when the chat window is closed"), SerializeField] public bool isChatOpen ;
        [Tooltip("How much to offset the y position by (useful if the chatbox has some margin needed at the bottom)"), SerializeField, ValueRequired] float yOffset ;
    
        // Private Memory
        private RectTransform rectTransform;

        /// <summary>
        /// Get self References
        /// </summary>
        private void Awake()
        {
            TryGetComponent(out rectTransform);
        }

        /// <summary>
        /// Start running when the chat window is open, and stop running when the chat window is closed
        /// </summary>
        void Update()
        {
            if (isChatOpen)
            {
                Vector2 targetPosition = GetWorldPositionInCanvasSpace(playerTransform.position, yOffset);
                rectTransform.anchoredPosition = targetPosition;
            }
        }


        /// <summary>
        /// Takes the world position of an object on the screen and returns the position of that object in the canvas space (Overlay)
        /// You can also provide a yOffset to offset the y position by a fixed amount.
        /// </summary>
        Vector2 GetWorldPositionInCanvasSpace(Vector3 worldPosition, float yOffset)
        {
            //Position in screenspace (not the same as canvasspace)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            if (screenPos.z < 0)
            {
                // Object is behind the camera
                Debug.LogError("Object Is behind the camera!", this);
                return Vector2.zero;
            }
        
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null, // Overlay canvas → null camera
                out uiPos
            );
            uiPos.y += yOffset;
            return uiPos;
        }
    }
}
