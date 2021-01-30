#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-01-29 PM 6:34:45
// 작성자 : Rito

namespace Rito.InputBindings
{
    [CustomEditor(typeof(Test_InputBinding))]
    public class Test_InputBindingEditor : Editor
    {
        private Test_InputBinding t;

        private void OnEnable()
        {
            t = target as Test_InputBinding;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Debug"))
            {
                Debug.Log(t._binding);
            }

            if (GUILayout.Button("Save"))
            {
                t._binding.SaveToFile();
            }

            if (GUILayout.Button("Load"))
            {
                t._binding.LoadFromFile();
            }
        }
    }
}
#endif