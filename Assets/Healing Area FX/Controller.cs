using System.Collections;
using DG.Tweening;
using NullReferenceDetection;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;
using VInspector;

namespace Healing_Area_FX
{
    public class Controller : MonoBehaviour
    {
        // References
        [Tooltip("TMP text element showing the countdown"), SerializeField, ValueRequired] TMP_Text countdownTMP;
        [Tooltip("Main Cinemachine camera that is active."), SerializeField, ValueRequired] Camera mainCamera;
        [Tooltip("VFX to indicate aniticipation for upcoming heal"), SerializeField, ValueRequired] VisualEffect ancitipationVFX;
        [Tooltip("VFX to indicate that healing just occured"), SerializeField, ValueRequired] VisualEffect activateVFX;

        //Controls
        [Header("Countdown Animation")]
        [Tooltip("How much should the countdownTMP's scale be punched"), SerializeField, ValueRequired] float punchAmount = 1f ;
        
        // Memory
        private bool _isEffectActive = false;

        private void Start()
        {
            countdownTMP.gameObject.SetActive(false);
            ancitipationVFX.gameObject.SetActive(false);
            activateVFX.gameObject.SetActive(false);
        }

        /// <summary>
        /// call this to start the visual, please pass in how many seconds are remaining before heal happens
        /// </summary>
        /// <param name="waitTime">Heal will happen after how much time?</param>
        public IEnumerator PlayVisual(int waitTime)
        {
            _isEffectActive = true;
            StartCoroutine(PointCountDownToCamera());

            // Reset visuals
            ancitipationVFX.gameObject.SetActive(false);
            activateVFX.gameObject.SetActive(false);
            activateVFX.Stop();
            countdownTMP.gameObject.SetActive(true);

            
            // --- Handle Countdown and awaiting VFX
            countdownTMP.gameObject.SetActive(true);
            ancitipationVFX.gameObject.SetActive(true);
            for (int i = waitTime; i > 0; i--)
            {
                ancitipationVFX.Reinit();
                ancitipationVFX.Play();
                countdownTMP.text = i.ToString();
                countdownTMP.transform.DOPunchScale(punchAmount*Vector3.one, AnimationConstants.TinyDuration);  // Small punch animation to the countdownTMP Text.
                yield return new WaitForSeconds(1f);
                ancitipationVFX.Stop();
            }

            // Hide other VFX and TMP
            countdownTMP.gameObject.SetActive(false);
            ancitipationVFX.gameObject.SetActive(false);

            // --- ARRIVED VFX ---
            activateVFX.gameObject.SetActive(true);
            activateVFX.Reinit();
            activateVFX.Play();

            _isEffectActive = false;
        }

        /// <summary>
        /// A method that runs as long as the VFX is active.
        /// </summary>
        public IEnumerator PointCountDownToCamera()
        {
            while (_isEffectActive)
            {
                if (mainCamera != null && countdownTMP != null)
                {
                    Vector3 dir = mainCamera.transform.position - countdownTMP.transform.position;
                    dir.y = 0; // Remove vertical influence for pure Y rotation

                    if (dir != Vector3.zero)
                    {
                        // Desired rotation only in Y axis
                        Quaternion lookRot = Quaternion.LookRotation(dir);
                        float targetY = lookRot.eulerAngles.y;

                        // Preserve existing X and Z rotations
                        Vector3 currentEuler = countdownTMP.transform.eulerAngles;

                        countdownTMP.transform.rotation = Quaternion.Euler(
                            currentEuler.x, // preserve X tilt
                            targetY+180,        // rotate only Y towards camera
                            currentEuler.z  // preserve Z roll
                        );
                    }
                }
                else
                {
                    Debug.LogError("Can't point countdownTMP towards Camera, cause a reference is missing", this);
                    yield break;
                }

                yield return null;
            }
        }

        [Button]
        // Private method to run coroutine in editor during playtime to test
        private void TestInEditor(int waitTime)
        {
            // Safety
            if (!Application.isPlaying)
            {
                Debug.LogError("Try Again in PlayMode!", this);
                return;
            }
            if (_isEffectActive)
            {
                Debug.LogError("Effect is already active, Try again later!", this);
                return;
            }
            StartCoroutine(PlayVisual(waitTime));
        }
    }
}
