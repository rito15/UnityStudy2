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
    private Quaternion qZ15;
    private Quaternion qY45;
    private Quaternion qX30;
    private Quaternion qX30Y45Z15;

    private void Start()
    {
        qX30 = Quaternion.Euler(30f, 0f, 0f);
        qY45 = Quaternion.Euler(0f, 45f, 0f);
        qZ15 = Quaternion.Euler(0f, 0f, 15f);
        qX30Y45Z15 = Quaternion.Euler(30f, 45f, 15f);
    }

    [SerializeField, Range(0f, 100f)]
    private float hRotationSpeed = 50f;  // 좌우 회전 속도

    [SerializeField, Range(0f, 100f)]
    private float vRotationSpeed = 100f; // 상하 회전 속도

    private void Update()
    {
        float t = Time.deltaTime;

        // 마우스 움직임 감지
        float h =  Input.GetAxisRaw("Mouse X") * hRotationSpeed * t;
        float v = -Input.GetAxisRaw("Mouse Y") * vRotationSpeed * t;

        // [1] 좌우 회전 : 월드 Y축 기준
        Quaternion hRot = Quaternion.AngleAxis(h, Vector3.up);
        transform.rotation = hRot * transform.rotation;

        // [2] 상하 회전 : 로컬 X축 기준
        float xNext = transform.eulerAngles.x + v;
        if (xNext > 180f)
            xNext -= 360f;

        // 상하 회전 각도 제한
        if (-90f < xNext && xNext < 90f)
        {
            Quaternion vRot = Quaternion.AngleAxis(v, Vector3.right);
            transform.rotation = transform.rotation * vRot;
        }
    }

    private void Update2()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.rotation *= qX30;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation *= qY45;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            //transform.rotation *= (Quaternion.Inverse(transform.rotation) * q1 * transform.rotation);
            transform.rotation = qZ15 * transform.rotation;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation *= Quaternion.Inverse(transform.rotation);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            transform.rotation *= qZ15;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            transform.rotation *= qZ15 * qY45;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.rotation *= qY45 * qZ15;
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


        if (Input.GetKeyDown(KeyCode.B))
        {
            transform.rotation = qY45 * qX30 * qZ15;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            transform.rotation = qZ15 * qX30 * qY45;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            transform.rotation = Quaternion.Euler(0, 0, 30) * transform.rotation;
            Debug.Log(Quaternion.Angle(Quaternion.AngleAxis(7, Vector3.one), Quaternion.AngleAxis(79, Vector3.one)));
        }
    }
}