using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BookMasterData))]
public class BookMasterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (BookMasterData)target;

        if (GUILayout.Button("Update Release Time", GUILayout.Height(30)))
        {
            script.UpdateCurrentTime();
        }

    }
}
