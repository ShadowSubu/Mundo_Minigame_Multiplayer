using UnityEditor;
using System.Linq;
using UnityEngine;

public class HierarchyShortcuts : Editor
{

    private enum SiblingChangeDirection
    {
    Up,
    Down  
    }

  [MenuItem("GameObject/Set to Next Sibling &S")] 
  static void SetToNextSibling()
  {
    ChangeSibling(SiblingChangeDirection.Up);
  }

  [MenuItem("GameObject/Set to Previous Sibling &W")]
  static void SetToPreviousSibling()
  { 
    ChangeSibling(SiblingChangeDirection.Down);
  }

  [MenuItem("GameObject/Reparent Below &D")]
  static void ReparentBelow()
  {
    GameObject selected = Selection.activeGameObject;
    if (selected != null) {
      Transform parent = selected.transform.parent;
      int index = selected.transform.GetSiblingIndex();

      if (index < parent.childCount - 1) {
        GameObject newParent = parent.GetChild(index + 1).gameObject;
        Undo.SetCurrentGroupName("Reparent Below");
        selected.transform.SetParent(newParent.transform);
      }
    }
  }
 
  static void ChangeSibling(SiblingChangeDirection direction)
  {
    var siblings = Selection.gameObjects;
    if (siblings == null)
    {
      return;
    }

    foreach (var sibling in siblings)
    {
      if (direction == SiblingChangeDirection.Up &&
          sibling.transform.GetSiblingIndex() == sibling.transform.parent.childCount - 1)
      {
        MoveToParentLevel(sibling, true);
      }
      else if (direction == SiblingChangeDirection.Down &&
               sibling.transform.GetSiblingIndex() == 0)
      {
        MoveToParentLevel(sibling, false);
      }
      else
      {
        // Normal reordering
        ReorderSiblings(siblings, direction);
      }

      if (direction == SiblingChangeDirection.Down) {
        MoveIntoExpandedParent(sibling);
      }
    }
  }

  static void ReorderSiblings(GameObject[] siblings, SiblingChangeDirection direction)
  {
    if (siblings.Any(s => s.transform.parent != siblings[0].transform.parent))
    {
      return;
    }

    if (siblings[0].transform.parent != null)
    {
      Undo.RegisterCompleteObjectUndo(siblings[0].transform.parent, "Change Sibling");
    }

    int sortOrder = (direction == SiblingChangeDirection.Up) ? 1 : -1;

    foreach (var sib in siblings.OrderBy(x => -1 * sortOrder * x.transform.GetSiblingIndex()))
    {
      var index = sib.transform.GetSiblingIndex();
      if (index + sortOrder < 0) continue;

      sib.transform.SetSiblingIndex(index + sortOrder);
    }
  }

  static void MoveToParentLevel(GameObject child, bool aboveParent)
  {
    GameObject parent = child.transform.parent.gameObject;
   
    if (aboveParent) {
      child.transform.SetParent(parent.transform.parent);
      child.transform.SetSiblingIndex(parent.transform.GetSiblingIndex() + 1);
    }
    else {
      child.transform.SetParent(parent.transform.parent);
      child.transform.SetSiblingIndex(parent.transform.GetSiblingIndex());
    }
  }

  static void MoveIntoExpandedParent(GameObject child) {
  Transform parent = child.transform.parent;
  if (parent && IsParentExpanded(parent.gameObject)) {
    child.transform.SetParent(parent);
  }
}

  static bool IsParentExpanded(GameObject parent) {

    // Fallback for older Unity versions
    var serializedObject = new SerializedObject(parent);
    var isExpandedProperty = serializedObject.FindProperty("m_IsActive");

    if (isExpandedProperty != null)
    {
      return isExpandedProperty.boolValue;
    }
    else return false;
  }

}