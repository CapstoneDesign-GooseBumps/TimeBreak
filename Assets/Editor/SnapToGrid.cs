using UnityEngine;
using UnityEditor;

public class SnapToGrid : EditorWindow {
    private float gridSize = 1f;

    [MenuItem("Tools/Snap Selected to Grid")]
    public static void ShowWindow() {
        GetWindow<SnapToGrid>("Snap To Grid");
    }

    void OnGUI() {
        GUILayout.Label("선택한 오브젝트를 격자에 정렬", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);

        if (GUILayout.Button("Snap Selected")) {
            SnapSelectedObjects(gridSize);
        }
    }

    private static void SnapSelectedObjects(float gridSize) {
        foreach (GameObject obj in Selection.gameObjects) {
            Vector3 pos = obj.transform.position;

            // 각 축에 대해 가장 가까운 격자 위치로 스냅
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
            pos.z = Mathf.Round(pos.z / gridSize) * gridSize;

            Undo.RecordObject(obj.transform, "Snap To Grid");
            obj.transform.position = pos;
        }
    }
}