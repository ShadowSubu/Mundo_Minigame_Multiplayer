#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
#endif
using TMPro;
using UnityEngine;

namespace Addons.UnityUIContextCommands
{
    public static class UnityUIContextCommands
    {
#if UNITY_EDITOR
        [MenuItem("CONTEXT/VerticalLayoutGroup/Convert to Horizontal")]
        static void ConvertToHorizontal(MenuCommand command)
        {
            VerticalLayoutGroup vlg = command.context as VerticalLayoutGroup;

            if (vlg == null)
            {
                Debug.LogError("Vertical Layout Group not found", command.context);
                return;
            }

            GameObject go = vlg.gameObject;

            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Convert to Horizontal Layout Group");
            int undoGroup = Undo.GetCurrentGroup();

            // Copy settings
            bool enabled = vlg.enabled;
            RectOffset padding = new RectOffset(vlg.padding.left, vlg.padding.right, vlg.padding.top, vlg.padding.bottom);
            float spacing = vlg.spacing;
            TextAnchor childAlignment = vlg.childAlignment;
            bool reverseArrangement = vlg.reverseArrangement;
            bool childControlWidth = vlg.childControlWidth;
            bool childControlHeight = vlg.childControlHeight;
            bool childScaleWidth = vlg.childScaleWidth;
            bool childScaleHeight = vlg.childScaleHeight;
            bool childForceExpandWidth = vlg.childForceExpandWidth;
            bool childForceExpandHeight = vlg.childForceExpandHeight;

            // Remove VerticalLayoutGroup with undo support
            Undo.DestroyObjectImmediate(vlg);

            // Add HorizontalLayoutGroup with undo support
            HorizontalLayoutGroup hlg = Undo.AddComponent<HorizontalLayoutGroup>(go);

            // Apply copied settings
            Undo.RecordObject(hlg, "Set Horizontal Layout Group properties");
            hlg.enabled = enabled;
            hlg.padding = padding;
            hlg.spacing = spacing;
            hlg.childAlignment = childAlignment;
            hlg.reverseArrangement = reverseArrangement;
            hlg.childControlWidth = childControlWidth;
            hlg.childControlHeight = childControlHeight;
            hlg.childScaleWidth = childScaleWidth;
            hlg.childScaleHeight = childScaleHeight;
            hlg.childForceExpandWidth = childForceExpandWidth;
            hlg.childForceExpandHeight = childForceExpandHeight;

            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);

            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("Converted VerticalLayoutGroup to HorizontalLayoutGroup", go);
        }

        [MenuItem("CONTEXT/HorizontalLayoutGroup/Convert to Vertical")]
        static void ConvertToVertical(MenuCommand command)
        {
            HorizontalLayoutGroup hlg = command.context as HorizontalLayoutGroup;

            if (hlg == null)
            {
                Debug.LogError("Horizontal Layout Group not found", command.context);
                return;
            }

            GameObject go = hlg.gameObject;

            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Convert to Vertical Layout Group");
            int undoGroup = Undo.GetCurrentGroup();

            // Copy settings
            bool enabled = hlg.enabled;
            RectOffset padding = new RectOffset(hlg.padding.left, hlg.padding.right, hlg.padding.top, hlg.padding.bottom);
            float spacing = hlg.spacing;
            TextAnchor childAlignment = hlg.childAlignment;
            bool reverseArrangement = hlg.reverseArrangement;
            bool childControlWidth = hlg.childControlWidth;
            bool childControlHeight = hlg.childControlHeight;
            bool childScaleWidth = hlg.childScaleWidth;
            bool childScaleHeight = hlg.childScaleHeight;
            bool childForceExpandWidth = hlg.childForceExpandWidth;
            bool childForceExpandHeight = hlg.childForceExpandHeight;

            // Remove HorizontalLayoutGroup with undo support
            Undo.DestroyObjectImmediate(hlg);

            // Add VerticalLayoutGroup with undo support
            VerticalLayoutGroup vlg = Undo.AddComponent<VerticalLayoutGroup>(go);

            // Apply copied settings
            Undo.RecordObject(vlg, "Set Vertical Layout Group properties");
            vlg.enabled = enabled;
            vlg.padding = padding;
            vlg.spacing = spacing;
            vlg.childAlignment = childAlignment;
            vlg.reverseArrangement = reverseArrangement;
            vlg.childControlWidth = childControlWidth;
            vlg.childControlHeight = childControlHeight;
            vlg.childScaleWidth = childScaleWidth;
            vlg.childScaleHeight = childScaleHeight;
            vlg.childForceExpandWidth = childForceExpandWidth;
            vlg.childForceExpandHeight = childForceExpandHeight;

            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);

            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("Converted HorizontalLayoutGroup to VerticalLayoutGroup", go);
        }

        [MenuItem("CONTEXT/RectTransform/Fit Width to Children")]
        static void FitWidthToChildren(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogError("RectTransform not found", command.context);
                return;
            }
        
            GameObject go = rectTransform.gameObject;
        
            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Fit Width to Children");
            int undoGroup = Undo.GetCurrentGroup();
        
            // Try to get layout group component
            LayoutGroup layoutGroup = go.GetComponent<LayoutGroup>();
            if (layoutGroup == null)
            {
                // No layout group exists, add a horizontal one
                layoutGroup = Undo.AddComponent<HorizontalLayoutGroup>(go);
                Debug.Log("Added HorizontalLayoutGroup component", go);
            }
            else
            {
                // Record changes to existing layout group
                Undo.RecordObject(layoutGroup, "Modify Layout Group");
            }
        
            // Try to get content size fitter
            ContentSizeFitter contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                // Add content size fitter
                contentSizeFitter = Undo.AddComponent<ContentSizeFitter>(go);
                Debug.Log("Added ContentSizeFitter component", go);
            }
            else
            {
                // Record changes to existing content size fitter
                Undo.RecordObject(contentSizeFitter, "Modify Content Size Fitter");
            }
        
            // Set content size fitter to preferred size for width
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        
            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);
        
            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);
        
            Debug.Log("RectTransform width set to fit children", go);
        }
        
        [MenuItem("CONTEXT/RectTransform/Fit Height to Children")]
        static void FitHeightToChildren(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogError("RectTransform not found", command.context);
                return;
            }
        
            GameObject go = rectTransform.gameObject;
        
            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Fit Height to Children");
            int undoGroup = Undo.GetCurrentGroup();
        
            // Try to get layout group component
            LayoutGroup layoutGroup = go.GetComponent<LayoutGroup>();
            if (layoutGroup == null)
            {
                // No layout group exists, add a vertical one
                layoutGroup = Undo.AddComponent<HorizontalLayoutGroup>(go);
                Debug.Log("Added VerticalLayoutGroup component", go);
            }
            else
            {
                // Record changes to existing layout group
                Undo.RecordObject(layoutGroup, "Modify Layout Group");
            }
        
            // Try to get content size fitter
            ContentSizeFitter contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                // Add content size fitter
                contentSizeFitter = Undo.AddComponent<ContentSizeFitter>(go);
                Debug.Log("Added ContentSizeFitter component", go);
            }
            else
            {
                // Record changes to existing content size fitter
                Undo.RecordObject(contentSizeFitter, "Modify Content Size Fitter");
            }
        
            // Set content size fitter to preferred size for height
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);
        
            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);
        
            Debug.Log("RectTransform height set to fit children", go);
        }
        
        [MenuItem("CONTEXT/TextMeshProUGUI/Auto Width")]
        static void AutoWidth(MenuCommand command)
        {
            TextMeshProUGUI textComponent = command.context as TextMeshProUGUI;
            if (textComponent == null)
            {
                Debug.LogError("TextMeshProUGUI component not found", command.context);
                return;
            }

            GameObject go = textComponent.gameObject;

            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Set TextMeshProUGUI Auto Width");
            int undoGroup = Undo.GetCurrentGroup();

            // Try to get content size fitter
            ContentSizeFitter contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                // Add content size fitter
                contentSizeFitter = Undo.AddComponent<ContentSizeFitter>(go);
                Debug.Log("Added ContentSizeFitter component", go);
            }
            else
            {
                // Record changes to existing content size fitter
                Undo.RecordObject(contentSizeFitter, "Modify Content Size Fitter");
            }

            // Set content size fitter to preferred size for width
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);

            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("TextMeshProUGUI set to auto width", go);
        }

        [MenuItem("CONTEXT/TextMeshProUGUI/Auto Height")]
        static void AutoHeight(MenuCommand command)
        {
            TextMeshProUGUI textComponent = command.context as TextMeshProUGUI;
            if (textComponent == null)
            {
                Debug.LogError("TextMeshProUGUI component not found", command.context);
                return;
            }

            GameObject go = textComponent.gameObject;

            // Start recording the complete operation for undo
            Undo.SetCurrentGroupName("Set TextMeshProUGUI Auto Height");
            int undoGroup = Undo.GetCurrentGroup();

            // Try to get content size fitter
            ContentSizeFitter contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                // Add content size fitter
                contentSizeFitter = Undo.AddComponent<ContentSizeFitter>(go);
                Debug.Log("Added ContentSizeFitter component", go);
            }
            else
            {
                // Record changes to existing content size fitter
                Undo.RecordObject(contentSizeFitter, "Modify Content Size Fitter");
            }

            // Set content size fitter to preferred size for height
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Mark the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(go);

            // Close the undo group
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("TextMeshProUGUI set to auto height", go);
        }
#endif
    }
}