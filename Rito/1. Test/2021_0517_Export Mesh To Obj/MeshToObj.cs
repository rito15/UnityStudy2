#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-05-17 PM 8:57:35
// 작성자 : Rito

namespace Rito.Tests.ExportObj
{
    public class MeshToObj : MonoBehaviour
    {
        [SerializeField]
        private string meshName;
        private MeshFilter mf;
        private MeshRenderer mr;

        [CustomEditor(typeof(MeshToObj))]
        private class Custom : UnityEditor.Editor
        {
            private MeshToObj m;

            private void OnEnable()
            {
                m = target as MeshToObj;
                if(m.mf == null) m.TryGetComponent(out m.mf);
                if(m.mr == null) m.TryGetComponent(out m.mr);
            }

            public override void OnInspectorGUI()
            {
                if (m.mf == null)
                {
                    EditorGUILayout.HelpBox("Mesh Filter Does not Exist", MessageType.Error);
                    return;
                }
                if (m.mr == null)
                {
                    EditorGUILayout.HelpBox("Mesh Renderer Does not Exist", MessageType.Error);
                    return;
                }

                Undo.RecordObject(m, "Change Mesh Name");
                m.meshName = EditorGUILayout.TextField("Mesh Name", m.meshName);

                if (!m.meshName.IsNotEmpty())
                {
                    EditorGUILayout.HelpBox("Please Input Mesh Name", MessageType.Warning);
                    return;
                }

                if (GUILayout.Button("Export To OBJ"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save To OBJ", m.meshName, "obj", "");
                    
                    if (path.IsNotEmpty())
                    {
                        ObjExporter.SaveMeshToFile(m.mf.sharedMesh, m.mr, m.meshName, path);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }

    static class Extensions
    {
        public static bool IsNotEmpty(this string str)
            => !string.IsNullOrWhiteSpace(str);
    }
}

#endif