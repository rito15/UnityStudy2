#if UNITY_EDITOR__
using UnityEditor;

// 날짜 : 2021-02-09 PM 12:29:44
// 작성자 : Rito
// 유니티 에디터 종료 시 확인 다이얼로그 띄우기

[InitializeOnLoad]
public class EditorExitDialog
{
    static EditorExitDialog()
    {
        EditorApplication.wantsToQuit += 
            () => EditorUtility.DisplayDialog("Unity Editor", "Are you sure to quit ?", "Yes", "No");
    }
}
#endif