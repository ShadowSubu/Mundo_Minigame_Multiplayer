using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NullReferenceDetection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

namespace Chat_UI.Chatbox
{
    public class Animations : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("TMP which displays the actual typed message."), SerializeField, ValueRequired] TMP_Text messageTMP ;
        [Tooltip("Horizontal Layout Group"), SerializeField, ValueRequired] HorizontalLayoutGroup chatboxLG ;

        [Header("Add animation settings")]
        public float addDuration = .2f;
        
        [Header("Test Animation Settings")]
        public bool playAnimation = true;
        public float frequency = 20f;
        public float amplitude = 1f;
        public float delay = .1f;
        
        [Header("Animation Settings")]
        [Tooltip("Duration for the opening animation"), SerializeField] private float openDuration = 0.5f;
        [Tooltip("Easing Graph for the opening animation"), SerializeField] private AnimationCurve openEase ;
        
        [Space]
        [Tooltip("Duration for the closing animation"), SerializeField] private float closeDuration = 0.5f;
        [Tooltip("Easing Graph for the opening animation"), SerializeField] private AnimationCurve closeEase ;
        
        // Memory
        private Sequence openSequence = null;
        private Sequence closeSequence = null;
        private Vector3[][] cachedVertices;
        
        private void Start()
        {
            messageTMP.ForceMeshUpdate();
        }

        Queue<char> inputQueue = new Queue<char>();
        bool processing = false;

        void Update()
        {
            foreach (char c in Input.inputString)
            {
                if (IsAllowedCharacter(c))
                {
                    inputQueue.Enqueue(c);
                }
            }

            if (!processing && inputQueue.Count > 0)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        bool IsAllowedCharacter(char c)
        {
            return
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == ' ';
        }

        IEnumerator ProcessQueue()
        {
            processing = true;

            while (inputQueue.Count > 0)
            {
                char c = inputQueue.Dequeue();
                AddCharacter(c);
                yield return new WaitForSeconds(addDuration);
            }

            processing = false;
        }
        
        // Animation for opening.
        [Button]
        void OpenChat()
        {
            if (!Application.isPlaying) return;
            
            closeSequence?.Kill();
            openSequence?.Kill();
            
            openSequence = DOTween.Sequence();
            openSequence.Append(transform.DOScale(Vector3.one, openDuration).SetEase(openEase));
            openSequence.Play();
        }
        
        // Animation for closing.
        [Button]
        void CloseChat()
        {
            if (!Application.isPlaying) return;
            
            closeSequence?.Kill();
            openSequence?.Kill();
            
            closeSequence = DOTween.Sequence();
            closeSequence.Append(transform.DOScale(Vector3.zero, closeDuration).SetEase(closeEase));
            closeSequence.Play();
        }
        
        [Button]
        void AddCharacter(char c)
        {
            if(!Application.isPlaying) return;
            StartCoroutine(AddCharacterCoroutine(c));

            // StartCoroutine(AddCharacterCoroutine(GetRandomChar()));
            string GetRandomChar()
            {
                var random = new System.Random();
                char c = (char)('a' + random.Next(0, 26));
                return c.ToString();
            }
        }

        IEnumerator AddCharacterCoroutine(char c)
        {
            messageTMP.text += c;
            messageTMP.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatboxLG.transform as RectTransform);
            yield return new WaitForEndOfFrame();

            // Force TMP to parse + generate mesh immediately
            TMP_TextInfo textInfo = messageTMP.textInfo;
            int lastCharIndex = textInfo.characterCount - 1;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[lastCharIndex];
            if(charInfo.isVisible == false) yield break;
            
            TMP_MeshInfo meshInfo = textInfo.meshInfo[0];   // Assuming there is only one mesh

            // Debug.Log("CharacterInfo Length: " + textInfo.characterInfo.Length);
            // Debug.Log("Character Count: " + textInfo.characterCount);
            // Debug.Log("Vertex Index for '"+charInfo.character+"' : " + charInfo.vertexIndex);
            // Debug.Log("Vertex Index Alt: " + textInfo.characterInfo[^1].vertexIndex);
            
            // string s = "";
            // for (int i = 0; i < textInfo.characterInfo.Length; i++)
            // {
            //     Debug.Log("Vertex Index for '"+textInfo.characterInfo[i].character+"' : "+textInfo.characterInfo[i].vertexIndex);
            //     s += textInfo.characterInfo[i].character;
            // }
            // Debug.Log("Final string = " + s);
            
            float step = 0f;
            Vector3 v0 = meshInfo.vertices[charInfo.vertexIndex];
            Vector3 v1 = meshInfo.vertices[charInfo.vertexIndex+1];
            Vector3 v2 = meshInfo.vertices[charInfo.vertexIndex+2];
            Vector3 v3 = meshInfo.vertices[charInfo.vertexIndex+3];
            while (step < addDuration)
            {
                float t = Mathf.InverseLerp(0, addDuration, step);
                
                Vector3 center = (v0 + v2) / 2;

                meshInfo.vertices[charInfo.vertexIndex] = Vector3.Lerp(center, v0, t);
                meshInfo.vertices[charInfo.vertexIndex+1] = Vector3.Lerp(center, v1, t);
                meshInfo.vertices[charInfo.vertexIndex+2] = Vector3.Lerp(center, v2, t);
                meshInfo.vertices[charInfo.vertexIndex+3] = Vector3.Lerp(center, v3, t);

                messageTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                step += Time.deltaTime;
                yield return null;
            }
        }
        
        [Button]
        void TestAnimation()
        {
            StartCoroutine(AnimationCoroutine());
        }
        IEnumerator AnimationCoroutine()
        {
            CacheTextVertices();
            float step = 0f;
            TMP_TextInfo textInfo = messageTMP.textInfo;
            
            while (playAnimation)
            {
                // Loop over all meshes.
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    Vector3[] vertices = textInfo.meshInfo[i].vertices;
                    Vector3[] baseVertices = cachedVertices[i];
                    
                    // Loop over all vertexes of a mesh.
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        float offset = Mathf.Sin(
                            (step + delay * Mathf.Floor(j / 4f)) * Mathf.Deg2Rad * frequency
                        );

                        vertices[j] = baseVertices[j] + (Vector3.up * (offset * amplitude));
                    }
                }

                messageTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                step += Time.deltaTime;
                yield return null;
            }
            
            void CacheTextVertices()
            {
                messageTMP.ForceMeshUpdate();

                TMP_TextInfo textInfo = messageTMP.textInfo;

                cachedVertices = new Vector3[textInfo.meshInfo.Length][];

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    Vector3[] src = textInfo.meshInfo[i].vertices;
                    cachedVertices[i] = new Vector3[src.Length];
                    System.Array.Copy(src, cachedVertices[i], src.Length);
                }
            }
        }
        
        // Animation for characters appearing.
        
        // Animation for characters disappearing.
    }
}