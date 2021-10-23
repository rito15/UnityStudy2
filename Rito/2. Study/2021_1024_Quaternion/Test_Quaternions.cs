using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-24 AM 2:20:46
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_Quaternions : MonoBehaviour
{
    public Transform target;
    private Quaternion q1;
    private Quaternion q2;

    private void Start()
    {
        q1 = Quaternion.Euler(0f, 0f, 15f);
        q2 = Quaternion.Euler(0, 45f, 0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation *= q1;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            //transform.rotation *= (Quaternion.Inverse(transform.rotation) * q1 * transform.rotation);
            transform.rotation = q1 * transform.rotation;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation *= Quaternion.Inverse(transform.rotation);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            transform.rotation *= q2;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            transform.rotation *= q1 * q2;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.rotation *= q2 * q1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Quaternion q = transform.rotation;
            Debug.Log($"{q} {q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w}");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Vector3 dir = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.right);
        }

    }
}