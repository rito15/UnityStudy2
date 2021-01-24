using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using Unity.Mathematics;
using Unity.Collections; // NativeArray
using Unity.Jobs;
using UnityEngine.Jobs;  // IJobParallelForTransform
using Unity.Burst;       // BurstCompile

// 날짜 : 2021-01-24 AM 10:17:03
// 작성자 : Rito
public class Test_TransformJob : MonoBehaviour
{
    // XZ 평면 회전
    [BurstCompile]
    public struct RotateJob : IJobParallelForTransform
    {
        public float t;
        public float speed;
        public float radius;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(
                pos.x + Mathf.Sin(t * speed) * radius,
                pos.y,
                pos.z + Mathf.Cos(t * speed) * radius
            );
        }
    }
    
    public Transform[] _transformArray; // 대상 트랜스폼들 등록
    private TransformAccessArray _transformAccessArray;

    private void Start()
    {
        // Transform 배열을 이용해 TransformAccessArray 초기화
        _transformAccessArray = new TransformAccessArray(_transformArray);
    }

    private void Update()
    {
        // 잡 생성
        RotateJob rJob = new RotateJob { t = Time.time, speed = 2f, radius = 0.05f };

        // 잡 예약(실행)
        JobHandle handle = rJob.Schedule(_transformAccessArray);
    }

    private void OnDestroy()
    {
        // 메모리 해제
        _transformAccessArray.Dispose();
    }
}