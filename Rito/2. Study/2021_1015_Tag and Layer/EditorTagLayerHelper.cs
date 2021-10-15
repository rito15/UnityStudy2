using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// 날짜 : 2021-10-15 PM 4:06:35
// 작성자 : Rito

public static class EditorTagLayerHelper
{
#if UNITY_EDITOR
    private static bool isPlaymode = false;

    [InitializeOnEnterPlayMode]
    private static void OnEnterPlayMode()
    {
        isPlaymode = true;
    }

    [InitializeOnLoadMethod]
    private static void OnLoadMethod()
    {
        if (isPlaymode) return;

        AddNewLayer("Layer01");
        AddNewLayer("PostProcess");

        Debug.Log("Hi");
    }
#endif

    /// <summary> 태그 중복 확인 및 추가 </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void AddNewTag(string tagName)
    {
#if UNITY_EDITOR
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        int tagCount = tagsProp.arraySize;

        // [1] 해당 태그가 존재하는지 확인
        bool found = false;
        for (int i = 0; i < tagCount; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

            if (t.stringValue.Equals(tagName))
            {
                found = true;
                break;
            }
        }

        // [2] 배열 마지막에 태그 추가
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagCount);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(tagCount);
            n.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }
#endif
    }

    /// <summary> 레이어 중복 확인 및 추가 </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void AddNewLayer(string layerName)
    {
#if UNITY_EDITOR
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        int layerCount = layersProp.arraySize;
        int targetIndex = -1;

        // [1] 해당 레이어가 존재하는지 확인
        // NOTE : 0 ~ 7까지는 Buit-in Layer 공간이므로 무시
        for (int i = 8; i < layerCount; i++)
        {
            SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
            string strValue = t.stringValue;

            // 빈 레이어 공간을 찾은 경우
            if (targetIndex == -1 && string.IsNullOrWhiteSpace(strValue))
            {
                targetIndex = i;
            }

            // 이미 해당 레이어 이름이 존재할 경우
            else if (strValue.Equals(layerName))
            {
                targetIndex = -1;
                break;
            }
        }

        // [2] 빈 공간에 레이어 추가
        if (targetIndex != -1)
        {
            SerializedProperty n = layersProp.GetArrayElementAtIndex(targetIndex);
            n.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
#endif
    }
}