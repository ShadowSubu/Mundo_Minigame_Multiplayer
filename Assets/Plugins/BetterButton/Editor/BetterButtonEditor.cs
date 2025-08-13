using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(BetterButton), true)]
[CanEditMultipleObjects]
public class BetterButtonEditor : Editor
{
    // SerializedProperties
    private SerializedProperty m_HoldableProperty;
    private SerializedProperty m_HoldForSecondsProperty;

    private SerializedProperty m_PressInProperty;
    private SerializedProperty m_PressInTransformProperty;
    private SerializedProperty m_PressInScalePoperty;
    
    private SerializedProperty m_AudioOnClickProperty;
    private SerializedProperty m_AudioOnHoldCompleteProperty;
    
    private SerializedProperty m_OnHoldCompleteProperty;
    private SerializedProperty m_OnHoldProperty;
    
    
    
    // Base Button properties
    private SerializedProperty m_NavigationProperty;
    private SerializedProperty m_TransitionProperty;
    private SerializedProperty m_InteractableProperty;
    
    private void OnEnable()
    {
        // Get properties from BetterButton
        m_HoldableProperty = serializedObject.FindProperty("holdable");
        m_HoldForSecondsProperty = serializedObject.FindProperty("holdForSeconds");
        
        m_PressInProperty = serializedObject.FindProperty("pressIn");
        m_PressInTransformProperty = serializedObject.FindProperty("pressInTransform");
        m_PressInScalePoperty = serializedObject.FindProperty("pressInScale");    
            
        m_AudioOnClickProperty = serializedObject.FindProperty("audioOnClick");
        m_AudioOnHoldCompleteProperty = serializedObject.FindProperty("audioOnHoldComplete");
        
        m_OnHoldCompleteProperty = serializedObject.FindProperty("onHoldComplete");
        m_OnHoldProperty = serializedObject.FindProperty("onHold");
        
        // Get properties from the base Button class
        m_NavigationProperty = serializedObject.FindProperty("m_Navigation");
        m_TransitionProperty = serializedObject.FindProperty("m_Transition");
        m_InteractableProperty = serializedObject.FindProperty("m_Interactable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Set default properties as requested
        var navigation = m_NavigationProperty.FindPropertyRelative("m_Mode");
        navigation.enumValueIndex = (int)UnityEngine.UI.Navigation.Mode.Automatic;
        m_TransitionProperty.enumValueIndex = (int)UnityEngine.UI.Selectable.Transition.None;
        
        // Expose properties.
        EditorGUILayout.PropertyField(m_InteractableProperty);  // Interactable property from base class
        EditorGUILayout.PropertyField(m_HoldableProperty);
        
        // Holdable fields
        bool showHoldFields = false;    // Handle rendering of hold related fields (also for multi select)
        if (!m_HoldableProperty.hasMultipleDifferentValues)
        {
            int holdableValue = m_HoldableProperty.enumValueIndex;
            showHoldFields = (holdableValue == 1 || holdableValue == 2); // Yes or YesAndClick
        }
        else
        {
            // If mixed values, show hold fields
            showHoldFields = true;
        }
        if (showHoldFields)
        {
            EditorGUILayout.PropertyField(m_HoldForSecondsProperty);
        }
        
        // PressIn section
        EditorGUILayout.PropertyField(m_PressInProperty);
        bool showPressInFields = m_PressInProperty.boolValue;
        if (showPressInFields)
        {
            EditorGUILayout.PropertyField(m_PressInTransformProperty);
            EditorGUILayout.PropertyField(m_PressInScalePoperty);
        }
        
        // Sounds section
        EditorGUILayout.PropertyField(m_AudioOnClickProperty);
        if (showHoldFields)
        {
            EditorGUILayout.PropertyField(m_AudioOnHoldCompleteProperty);
        }
        
        // Events section
        if (showHoldFields)
        {
            EditorGUILayout.PropertyField(m_OnHoldCompleteProperty);
            EditorGUILayout.PropertyField(m_OnHoldProperty);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}