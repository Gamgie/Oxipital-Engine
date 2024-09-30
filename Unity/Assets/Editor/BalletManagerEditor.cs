using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BalletManager))]
[CanEditMultipleObjects]
public class BalletManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BalletManager balletMngr = (BalletManager)target;

        ///////////////////////////////////////////////

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Pattern"))
        {
            balletMngr.AddPattern(BalletManager.PatternGroup.Orb);
        }

        if (GUILayout.Button("Remove Pattern"))
        {
            balletMngr.RemovePattern(BalletManager.PatternGroup.Orb);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Synchronize Pattern"))
        {
            balletMngr.SynchronizePattern();
        }

        base.DrawDefaultInspector();
    }
}
