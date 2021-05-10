using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// 날짜 : 2021-05-10 PM 5:26:15
// 작성자 : Rito

public class Test_RectTransformReal : MonoBehaviour
{
    public CanvasScaler _cs;
    public RectTransform _rt;

    private void Start()
    {
        float w = _rt.rect.width;
        float h = _rt.rect.height;
        var (rw, rh) = CalculateRealSize(_cs, _rt);

        Debug.Log($"Size : {w}x{h}, Real Size : {rw:F0}x{rh:F0}");
        Debug.Log($"Screen : {Screen.width}x{Screen.height}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Start();
        }
    }

    private (float width, float height) CalculateRealSize(CanvasScaler cs, RectTransform rt)
    {
        float wRatio = Screen.width / cs.referenceResolution.x;
        float hRatio = Screen.height / cs.referenceResolution.y;
        float ratio =
            wRatio * (1f - cs.matchWidthOrHeight) +
            hRatio * (cs.matchWidthOrHeight);

        return (rt.rect.width * ratio, rt.rect.height * ratio);
    }


    [CustomEditor(typeof(Test_RectTransformReal))]
    private class CE : UnityEditor.Editor
    {
        Test_RectTransformReal t;

        private void OnEnable()
        {
            t = target as Test_RectTransformReal;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Print"))
            {
                t.Start();
            }
        }
    }
}