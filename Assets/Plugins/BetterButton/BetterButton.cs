using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AudioSystem;
using DG.Tweening;
using NullReferenceDetection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Extends upon the base unity button, adds more functionality.
/// - 
/// - Sounds to play on click.
/// - "Press In" Animation
/// -
///
///
/// NOTE : Whenever you want to add more fields, you also need to implement them in the BetterButtonEditor.cs script
/// </summary>
public class BetterButton : Button
{
    [Tooltip("Is this button holdable or not"), SerializeField, ValueRequired] public HoldableBehavior holdable = HoldableBehavior.No;
    [Tooltip("How long should the button be held before the holdComplete event fires."), SerializeField, ValueRequired] public float holdForSeconds = 1f;
    
    [Tooltip("Should the button be pressed in when held"), SerializeField, ValueRequired] bool pressIn = false;
    [Tooltip("This is the transform that will be scaled down when held"), SerializeField, ValueRequired] RectTransform pressInTransform = null;
    [Tooltip("What will the content graphics scale down to when held"), SerializeField, ValueRequired] float pressInScale = .8f;
    
    [Header("Sounds")]
    [Tooltip("Audio played when clicked"), SerializeField, ValueRequired] public SoundData audioOnClick;
    [Tooltip("Audio played when held."), SerializeField, ValueRequired] public SoundData audioOnHoldComplete;

    // Public events
    [Tooltip("Fires after a hold is completed successfully"), SerializeField, ValueRequired] public UnityEvent onHoldComplete;
    [Tooltip("Fires every frame during a hold, also reports the hold status from 0 -> 100"), SerializeField, ValueRequired] public UnityEvent<float> onHold;
    
    // Memory
    // Cache SoundBuilder for performance
    private SoundBuilder _soundBuilder; // Play using _soundBuilder.Play(SoundData);
    private bool _holdCompleted = false;
    private Coroutine _holdCoroutine = null;
    private Tween _pressInTween=null;

    // Context Menu Methods
#if UNITY_EDITOR
[ContextMenu("Wrap Children in Container")]
void WrapChildrenInContainer()
{
    // Register undo operations
    Undo.SetCurrentGroupName("Wrap Children In Container");
    int undoGroupIndex = Undo.GetCurrentGroup();
    
    GameObject intermediate = new GameObject("Graphics Parent", typeof(RectTransform));
    // Register the creation of the new container
    Undo.RegisterCreatedObjectUndo(intermediate, "Create Container");
    
    intermediate.transform.SetParent(transform, false);
    RectTransform intermediateRt = intermediate.GetComponent<RectTransform>();

    intermediateRt.anchorMin = Vector2.zero;
    intermediateRt.anchorMax = Vector2.one;
    intermediateRt.offsetMin = Vector2.zero;
    intermediateRt.offsetMax = Vector2.zero;
    intermediateRt.SetAsFirstSibling();

    // Copy ContentSizeFitter
    CopyComponent<ContentSizeFitter>(transform, intermediate.transform);

    // Copy VerticalLayoutGroup
    CopyComponent<VerticalLayoutGroup>(transform, intermediate.transform);

    // Copy HorizontalLayoutGroup
    CopyComponent<HorizontalLayoutGroup>(transform, intermediate.transform);

    // Loop over all children and make them the children of 'intermediate'
    // We need to collect children first since we'll be modifying the hierarchy
    List<Transform> childrenToMove = new List<Transform>();
    for (int i = 1; i < transform.childCount; i++)
    {
        childrenToMove.Add(transform.GetChild(i));
    }
    
    // Now reparent them with Undo support
    foreach (Transform child in childrenToMove)
    {
        Undo.SetTransformParent(child, intermediate.transform, "Move Child to Container");
    }

    // Set the reference field if it exists in the class
    Undo.RecordObject(this, "Set pressInTransform reference");
    pressInTransform = intermediate.transform as RectTransform;
    
    // Group all these operations together
    Undo.CollapseUndoOperations(undoGroupIndex);
    
    // Mark scene as dirty
    EditorSceneManager.MarkSceneDirty(gameObject.scene);
    
    // <summary>
    // Used to copy a component with its settings from one object to another.
    // </summary>
    // <param name="source">Where to copy from</param>
    // <param name="destination">Where to paste this</param>
    // <typeparam name="T">Which component to copy</typeparam>
    void CopyComponent<T>(Transform source, Transform destination) where T : Component
    {
        T original = source.GetComponent<T>();
        if (original != null)
        {
            // Register undo for component addition
            T copy = Undo.AddComponent<T>(destination.gameObject);
            
            // Copy all fields
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            
            // Also copy properties that have both getter and setter
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && property.CanRead && property.GetIndexParameters().Length == 0)
                {
                    try
                    {
                        property.SetValue(copy, property.GetValue(original));
                    }
                    catch (System.Exception)
                    {
                        // Skip properties that can't be copied
                    }
                }
            }
        }
    }
}
#endif
    
    // Unity Events
    protected override void Awake()
    {
        base.Awake();
        _soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
        // Initialize events if they're null
        if (onHoldComplete == null)
            onHoldComplete = new UnityEvent();
            
        if (onHold == null)
            onHold = new UnityEvent<float>();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable() || !IsActive())
            return;
            
        base.OnPointerDown(eventData);
        
        if (holdable != HoldableBehavior.No)
        {
            // Stop any existing coroutine
            StopHoldCoroutine();
            
            // Start a new hold coroutine
            _holdCompleted = false;
            _holdCoroutine = StartCoroutine(HoldCoroutine());
        }

        // Press in Animation
        if (pressIn)
        {
            if(_pressInTween!=null)_pressInTween.Kill();
            Transform target = pressInTransform??transform;
            target.localScale = Vector3.one * pressInScale;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable() || !IsActive())
            return;
            
        // If we're in YesAndClick mode, let the base button handle the click
        // Otherwise, only process click if we're not in holding mode or holding is disabled
        if (holdable == HoldableBehavior.YesAndClick || holdable == HoldableBehavior.No)
        {
            base.OnPointerUp(eventData);
        }
   
        // Press in Animation
        if (pressIn)
        {
            if(_pressInTween!=null)_pressInTween.Kill();
            Transform target = pressInTransform??transform;
            _pressInTween = target.DOScale(Vector3.one, AnimationConstants.TinyDuration);
        }
        
        // Stop the hold coroutine
        StopHoldCoroutine();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // Only process click in certain conditions:
        // 1. If holding is disabled (No)
        // 2. If YesAndClick mode and hold wasn't completed
        // 3. If "Yes" mode, don't process clicks at all
        
        if (!IsInteractable() || !IsActive())
            return;
            
        if (holdable == HoldableBehavior.No || 
            (holdable == HoldableBehavior.YesAndClick && !_holdCompleted))
        {
            // Play click sound before calling base implementation
            if (audioOnClick != null)
            {
                _soundBuilder.WithRandomPitch().Play(audioOnClick);
            }
            
            Vibration.VibratePop();
            base.OnPointerClick(eventData);
        }
        
        // Reset hold completed state after click processing
        _holdCompleted = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        
        // Cancel holding when pointer exits the button
        StopHoldCoroutine();
    }
    
    private void StopHoldCoroutine()
    {
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }
    
    private IEnumerator HoldCoroutine()
    {
        float startTime = Time.time;
        
        while (true)
        {
            // Calculate hold progress
            float elapsed = Time.time - startTime;
            float holdProgress = Mathf.Clamp01(elapsed / holdForSeconds);
            float holdPercentage = holdProgress * 100f;
            
            // Invoke the progress event
            onHold.Invoke(holdPercentage);
            
            // Check if hold is complete
            if (holdProgress >= 1f && !_holdCompleted)
            {
                _holdCompleted = true;
                
                // Play hold complete sound
                if (audioOnHoldComplete != null)
                {
                    _soundBuilder.Play(audioOnHoldComplete);
                }
                
                // Invoke hold complete event
                onHoldComplete.Invoke();
                
                // No need to continue after hold is complete
                break;
            }
            
            yield return null; // Wait until next frame
        }
        
        _holdCoroutine = null;
    }

    [Serializable]
    public enum HoldableBehavior
    {
        No,
        Yes,
        YesAndClick
    }
}