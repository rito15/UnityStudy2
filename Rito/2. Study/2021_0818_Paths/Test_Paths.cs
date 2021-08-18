using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-08-18 PM 8:09:47
// 작성자 : Rito

#if UNITY_EDITOR


public class Test_Paths : MonoBehaviour
{
    public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 있는 폴더 경로"
    public static string ScriptFolderInProjectPath { get; private set; } // "Assets\......\이 스크립트가 있는 폴더 경로"
    public static string AssetFolderPath { get; private set; }           // "....../Assets"

    private static Texture2D texture;
    private static string textureFileName = "TextureName.png";

    [UnityEditor.InitializeOnLoadMethod]
    private static void Init()
    {
        InitFolderPath();
        AssetFolderPath = Application.dataPath;


        /* 현재 스크립트가 위치한 폴더로부터 텍스쳐 로드하는 예제 */

        if (texture == null)
        {
            // "Assets\...\TextureName.png"
            string texturePath = System.IO.Path.Combine(ScriptFolderInProjectPath, textureFileName);

            // AssetDatabase.LoadAssetAtPath() : "......\프로젝트폴더\" 에서부터 경로 시작
            texture = UnityEditor.AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
        }
    }

    private static void InitFolderPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
    {
        ScriptFolderFullPath = System.IO.Path.GetDirectoryName(sourceFilePath);
        int rootIndex = ScriptFolderFullPath.IndexOf(@"Assets\");
        if (rootIndex > -1)
        {
            ScriptFolderInProjectPath = ScriptFolderFullPath.Substring(rootIndex, ScriptFolderFullPath.Length - rootIndex);
        }
    }
}

#endif