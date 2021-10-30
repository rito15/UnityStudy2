using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-30 PM 8:57:44
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_CamMatrix : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    public Vector3 clipPos;
    public Vector3 clipPos2;

    private void Update()
    {
        clipPos = cam.cullingMatrix.MultiplyPoint(target.position);
        clipPos2 = cam.cullingMatrix.MultiplyPoint3x4(target.position);
    }
}