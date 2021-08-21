using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-08-17 AM 3:52:51
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Sample_PlayModeSave : MonoBehaviour
{
    public float a;

    /***********************************************************************
    *                           Save Play Mode Changes
    ***********************************************************************/
    #region .
#if UNITY_EDITOR
    private class Inner_PlayModeSave
    {
        private static UnityEditor.SerializedObject[] targetSoArr;

        [UnityEditor.InitializeOnLoadMethod]
        private static void Run()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                switch (state)
                {
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        //var targets = FindObjectsOfType(typeof(Inner_PlayModeSave).DeclaringType); // 비활성 오브젝트 제외
                        var targets = Resources.FindObjectsOfTypeAll(typeof(Inner_PlayModeSave).DeclaringType); // 비활성 오브젝트 포함
                        targetSoArr = new UnityEditor.SerializedObject[targets.Length];
                        for (int i = 0; i < targets.Length; i++)
                            targetSoArr[i] = new UnityEditor.SerializedObject(targets[i]);
                        break;

                    case UnityEditor.PlayModeStateChange.EnteredEditMode:
                        foreach (var oldSO in targetSoArr)
                        {
                            if (oldSO.targetObject == null) continue;
                            var oldIter = oldSO.GetIterator();
                            var newSO = new UnityEditor.SerializedObject(oldSO.targetObject);
                            while (oldIter.NextVisible(true))
                                newSO.CopyFromSerializedProperty(oldIter);
                            newSO.ApplyModifiedProperties();
                        }
                        break;
                }
            };
        }
    }
#endif
    #endregion
}