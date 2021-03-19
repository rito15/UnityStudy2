using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-19 PM 4:35:19
// 작성자 : Rito

public class Test_Rotation : MonoBehaviour
{
    public Vector3 rotVector;

    private void Update()
    {
        RotateDirectionVectorWithAxis();
    }

    // 유니티에서 회전의 순서 : Z - X - Y

    // 트랜스폼 자신의 축으로 회전시키기
    private void RotateSelf()
    {
        // rotVector가 (1, 0, 0)인 경우 : 자신의 X축으로 회전
        // 트랜스폼의 rotation 값 자체는 x, y, z 모두 변화하지만,
        // 회전 자체는 자신의 축을 기반으로 수행

        transform.Rotate(rotVector * Time.deltaTime, Space.Self);

        // Space.Self일 때의 Rotate() 내부 구현
        Quaternion nextRotation = transform.localRotation * Quaternion.Euler(rotVector * Time.deltaTime);
        transform.localRotation = nextRotation;


        // 축약
        transform.localRotation *= Quaternion.Euler(rotVector * Time.deltaTime);
    }

    // 월드 축으로 회전시키기
    private void RotateGlobal()
    {
        // rotVector의 각각 x, y, z 값만큼 해당 축 회전 적용
        transform.Rotate(rotVector * Time.deltaTime, Space.World);

        // Space.World일 때의 Rotate() 내부 구현
        Quaternion rot = transform.rotation;
        Quaternion nextRotation = 
            rot * Quaternion.Inverse(rot) * Quaternion.Euler(rotVector * Time.deltaTime) * rot;

        transform.rotation = nextRotation;


        //Quaternion rot = transform.rotation;
        transform.rotation *= 
            rot * Quaternion.Inverse(rot) * Quaternion.Euler(rotVector * Time.deltaTime) * rot;
    }

    // 방향 벡터 회전시키기
    private void RotateDirectionVector()
    {
        Vector3 dirVec = new Vector3(1f, 0f, 0f);  // X축 방향 벡터
        Vector3 rotVec = new Vector3(0f, 45f, 0f); // Y축 45도 회전

        Vector3 rotatedDirVec = Quaternion.Euler(rotVec) * dirVec;

        Debug.DrawRay(default, dirVec * 5f, Color.red);
        Debug.DrawRay(default, rotatedDirVec * 5f, Color.blue);
    }

    Vector3 _dirVec = Vector3.right; // 실시간으로 회전되는 벡터 저장

    // 방향벡터를 특정 회전축으로 회전시키기
    private void RotateDirectionVectorWithAxis()
    {
        Vector3 dirVec = new Vector3(1f, 0f, 0f); // 회전시킬 방향벡터
        Vector3 axisVec = new Vector3(-1f, 1f, 0f).normalized; // 회전 기준축 벡터

        Vector3 rotatedDirVec = Quaternion.AngleAxis(45f * Time.deltaTime, axisVec) * _dirVec;
        _dirVec = rotatedDirVec;

        Debug.DrawRay(default, dirVec * 15f, Color.red);
        Debug.DrawRay(default, axisVec * 15f, Color.green);
        Debug.DrawRay(default, rotatedDirVec * 5f, Color.blue, 5f);
    }
}