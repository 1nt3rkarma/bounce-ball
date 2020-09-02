using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var level = (Level)target;

        if (GUILayout.Button("Clear save"))
        {
            level.ClearSave();
        }
    }
}
