using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections; // NativeArray
using Unity.Jobs;
using Unity.Burst;       // BurstCompile

// 날짜 : 2021-01-24 AM 10:56:53
// 작성자 : Rito
public class Test_AddJob : MonoBehaviour
{
    [BurstCompile]
    public struct AddJob : IJob
    {
        public float a;
        public float b;
        [WriteOnly] public NativeArray<float> result; // 결과 저장

        public void Execute()
        {
            result[0] = a + b;
        }
    }

    private void Start()
    {
        // 1. 결과 배열 생성
        NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);

        // 잡 생성
        AddJob jobData = new AddJob { a = 15, b = 20, result = result };

        // 스케줄링
        JobHandle handle = jobData.Schedule();

        // 대기
        handle.Complete();

        // 결과 확인
        float aPlusB = result[0];

        // 해제
        result.Dispose();
    }
}