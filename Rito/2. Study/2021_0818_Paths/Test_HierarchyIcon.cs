using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-08-18 PM 8:49:43
// 작성자 : Rito

[DisallowMultipleComponent]
public class Test_HierarchyIcon : MonoBehaviour
{
#if UNITY_EDITOR
    public static string CurrentFolderPath { get; private set; } // "Assets\......\이 스크립트가 있는 폴더 경로"

    private static Texture2D iconTexture;
    private static string iconTextureFileName = "Icon.png";

    [UnityEditor.InitializeOnLoadMethod]
    private static void ApplyHierarchyIcon()
    {
        InitFolderPath();

        if (iconTexture == null)
        {
            // "Assets\...\Icon.png"
            string texturePath = System.IO.Path.Combine(CurrentFolderPath, iconTextureFileName);
            iconTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
        }

        UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconHandler;
    }

    private static void InitFolderPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        CurrentFolderPath = System.IO.Path.GetDirectoryName(sourceFilePath);
        int rootIndex = CurrentFolderPath.IndexOf(@"Assets\");
        if (rootIndex > -1)
        {
            CurrentFolderPath = CurrentFolderPath.Substring(rootIndex, CurrentFolderPath.Length - rootIndex);
        }
    }

    static void HierarchyIconHandler(int instanceID, Rect selectionRect)
    {
        const float Pos = -10f;

        // 1. Icon Rect
        Rect iconRect = new Rect(selectionRect);
        iconRect.x = iconRect.width + Pos;
        iconRect.width = 16f;

        // 2. Label Rect
        Rect labelRect = new Rect(iconRect);
        labelRect.x += 20f; 
        labelRect.width = 60f;

        GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go != null && go.activeSelf && go.GetComponent<Test_HierarchyIcon>() != null)
        {
            GUI.DrawTexture(iconRect, iconTexture);

            Color c = GUI.color;
            GUI.color = Color.yellow;
            {
                if (GUI.Button(labelRect, "Nyang"))
                {
                    Debug.Log($"{selectionRect.xMax}, {labelRect.xMax}");
                }
            }

            GUI.color = c;
        }
    }
#endif
}