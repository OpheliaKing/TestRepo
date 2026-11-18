using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("세이브 삭제"))
            DeleteSave();
    }

    private static void DeleteSave()
    {
        PlayerInfo.DeleteLocal();
        if (!Application.isPlaying || GameManager.Instance == null)
            return;

        PlayerDataManager playerData = GameManager.Instance.PlayerDataManager;
        if (playerData != null)
            playerData.Load();
    }
}
