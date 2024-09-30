using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BalletPattern), true)]
[CanEditMultipleObjects]
public class BalletPatternEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BalletPattern balletPattern = (BalletPattern) target;

        /////////////////////////////////////////////////

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Dancer"))
        {
            balletPattern.AddDancer();
        }

        if (GUILayout.Button("Remove Dancer"))
        {
            balletPattern.RemoveDancer();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset Speed"))
        {
            balletPattern.ResetSpeed();
        }

        base.DrawDefaultInspector();
    }
}
