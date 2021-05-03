using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

// 날짜 : 2021-05-01 PM 3:31:50
// 작성자 : Rito

// http://dotween.demigiant.com/documentation.php
public class Study_DoTween : MonoBehaviour
{
    public float fValue;
    public Transform target0;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Test();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            TestTween();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            target0
                .DOMoveX(target0.position.x + 2f, 1f);
        }
    }

    private void Test()
    {
        DOTween.To(() => fValue, value => fValue = value, 4f, 2f);
    }

    private void TestTween()
    {
        target0
            .DOScale(3f, 1f)
            .SetEase(Ease.OutBounce)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() => Debug.Log("Completed"));
    }
}