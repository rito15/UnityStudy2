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

            EditorGUILayout.Space();

            if (GUILayout.Button("Reset"))
            {
                t._binding.ResetAll();
            }

            if (GUILayout.Button("Save"))
            {
                t._binding.SaveToFile();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Load Preset 001"))
            {
                t._binding.id = "001";
                t._binding.LoadFromFile();
            }

            if (GUILayout.Button("Load Preset 002"))
            {
                t._binding.id = "002";
                t._binding.LoadFromFile();
            }

            if (GUILayout.Button("Load Preset 003"))
            {
                t._binding.id = "003";
                t._binding.LoadFromFile();
            }
        }
    }
}
#endif