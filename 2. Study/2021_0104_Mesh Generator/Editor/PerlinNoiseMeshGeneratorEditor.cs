using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-01-10 AM 1:21:43
// 작성자 : Rito

namespace Rito.MeshGenerator
{
    using static MeshGeneratorEditorHelper;

    [CustomEditor(typeof(PerlinNoiseMeshGenerator))]
    public class PerlinNoiseMeshGeneratorEditor : UnityEditor.Editor
    {
        public PerlinNoiseMeshGenerator selected;

        private void OnEnable()
        {
            selected = AssetDatabase.Contains(target) ? null : (PerlinNoiseMeshGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            if (selected == null)
                return;
            Color oldTextColor = GUI.contentColor;
            Color oldBgColor = GUI.backgroundColor;

            EditorGUILayout.Space();

            if (selected._resolution.x < 1)
                selected._resolution = new Vector2Int(1, selected._resolution.y);

            if (selected._resolution.y < 1)
                selected._resolution = new Vector2Int(selected._resolution.x, 1);

            if (selected._width.x <= 0f)
                selected._width = new Vector2(1f, selected._width.y);

            if (selected._width.y <= 0f)
                selected._width = new Vector2(selected._width.x, 1f);

            if (selected._maxHeight <= 0) selected._maxHeight = 1f;

            selected._resolution = EditorGUILayout.Vector2IntField("Resolution XY", selected._resolution);
            selected._width = EditorGUILayout.Vector2Field("Width XY", selected._width);
            selected._maxHeight = EditorGUILayout.FloatField("Max Height Limit", selected._maxHeight);

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Generate Mesh"))
            {
                selected.GenerateMesh();
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = selected._showVertexGizmo ? Color.green : Color.black;
            if (GUILayout.Button("Show Vertex"))
            {
                selected._showVertexGizmo = !selected._showVertexGizmo;
                FocusToSceneView();
            }

            GUI.backgroundColor = selected._showEdgeGizmo ? Color.green : Color.black;
            if (GUILayout.Button("Show Edge"))
            {
                selected._showEdgeGizmo = !selected._showEdgeGizmo;
                FocusToSceneView();
            }
            GUILayout.EndHorizontal();

            GUI.contentColor = oldTextColor;
            GUI.backgroundColor = oldBgColor;
        }
    }
}