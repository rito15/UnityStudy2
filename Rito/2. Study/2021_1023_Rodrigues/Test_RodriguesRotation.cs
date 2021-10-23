using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-23 PM 10:05:39
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_RodriguesRotation : MonoBehaviour
{
    public Transform origin;
    public Transform end;

    public Vector3 axis = Vector3.one;
    [Range(0f, 5f)]
    public float speed = 1f;

    private Vector3 vec;

    private void OnEnable()
    {
        vec = end.position - origin.position;
        Debug.Log(vec.magnitude);
    }

    private void Update()
    {
        Vector3 N = axis.normalized;

        Rodrigues(ref vec, N, Time.deltaTime * speed);
        Debug.DrawLine(Vector3.zero, vec, Color.red, Time.deltaTime);

        Vector3 prj = Vector3.Dot(vec, N) * N;
        Debug.DrawLine(prj, vec, Color.red, Time.deltaTime);

        Debug.Log(vec.magnitude);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (origin) Gizmos.DrawSphere(origin.position, 0.3f);

        Gizmos.color = Color.blue;
        if (end) Gizmos.DrawSphere(end.position, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(-axis * 10f, axis * 10f);
    }

    private void Rodrigues(ref Vector3 V, in Vector3 N, float radian)
    {
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);

        V = (V * cos) + (1 - cos) * (Vector3.Dot(V, N)) * N + (Vector3.Cross(N, V) * sin);
    }
}