using UnityEngine;

[DisallowMultipleComponent]
public class Test_HierarchyLeftIcon : MonoBehaviour
{
#if UNITY_EDITOR
    public static string CurrentFolderPath { get; private set; } // "Assets\......\이 스크립트가 있는 폴더 경로"

    private static Texture2D iconTexture;
    private static readonly string iconTextureFileName = "Icon.png";

    [UnityEditor.InitializeOnLoadMethod]
    private static void ApplyHierarchyIcon()
    {
        InitFolderPath();

        if (iconTexture == null)
        {
            string texturePath = System.IO.Path.Combine(CurrentFolderPath, iconTextureFileName);
            iconTexture = UnityEditor.AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
        }

        if (iconTexture != null)
        {
            UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
        }
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

    static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
    {
        const float Pos =
#if UNITY_2019_3_OR_NEWER      
            32f;
#else
            0f
#endif

        Rect iconRect = new Rect(selectionRect);
        iconRect.x = Pos;
        iconRect.width = 16f;

        GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go != null && go.GetComponent<Test_HierarchyLeftIcon>() != null)
        {
            GUI.DrawTexture(iconRect, iconTexture);
        }
    }
#endif
}