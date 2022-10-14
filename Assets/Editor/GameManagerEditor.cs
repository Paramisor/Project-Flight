using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (GameManager)target;
        if (GUILayout.Button("Fill Files Paths"))
        {
            ///MasterConfig.TrackList = MasterConfig.GetStreamingAssetsFiles();
        }
    }
}
