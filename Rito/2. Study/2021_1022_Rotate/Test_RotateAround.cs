using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-22 PM 7:51:28
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_RotateAround : MonoBehaviour
{
    public Transform target;
    [Range(0, 100f)]
    public float rotateSpeed = 1f;
    public Vector3 axis = new Vector3(0f, 1f, 0f);
    public Vector3 diff = new Vector3(4f, 0f, 0f);

    [SerializeField]
    private int mode = 0;

    private float t = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            mode = (mode + 1) % 3;

        switch (mode)
        {
            case 0: RotateAround0(axis, rotateSpeed); break;
            case 1: RotateAround1(axis, diff, rotateSpeed, ref t); break;
            case 2: RotateAround2(axis, diff, rotateSpeed, ref t); break;
        }
    }

    /// <summary> 현재 타겟과의 관계에 따라 회전하기 </summary>
    private void RotateAround0(in Vector3 axis, float speed)
    {
        float t = speed * Time.deltaTime;
        transform.RotateAround(target.position, axis, t);
    }

    /// <summary> 타겟과 일정 거리를 유지한 채로 회전하기 </summary>
    private void RotateAround1(in Vector3 axis, in Vector3 diff, float speed, ref float t)
    {
        t += speed * Time.deltaTime;

        Vector3 offset = Quaternion.AngleAxis(t, axis) * diff;
        transform.position = target.position + offset;
    }

    /// <summary> 타겟과의 거리를 유지하고, 타겟을 바라보며 회전하기 </summary>
    private void RotateAround2(in Vector3 axis, in Vector3 diff, float speed, ref float t)
    {
        t += speed * Time.deltaTime;

        Vector3 offset = Quaternion.AngleAxis(t, Vector3.up) * diff;
        transform.position = target.position + offset;

        Quaternion rot = Quaternion.LookRotation(-offset, axis);
        transform.rotation = rot;
    }
}