using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using Unity.Collections; // NativeArray
using Unity.Jobs;
using Unity.Burst;       // BurstCompile

using Debug = UnityEngine.Debug;

// 날짜 : 2021-01-24 AM 11:01:49
// 작성자 : Rito
public class Test_ParallelCalcJob : MonoBehaviour
{
    const int Size = 15000000;

    // 1. 단순 덧셈
    static float JustAdd(in float a, in float b)
    {
        return a + b;
    }

    // 2. 복합 계산
    static float SomeCalc(in float a, in float b)
    {
        return a * 123 + b / 85 + a * b * Mathf.Pow(a, 2.5f) / Mathf.Pow(b, 0.25f)
            * Mathf.Sin(a) * Mathf.Cos(b) * Mathf.Sqrt(a) * Mathf.Abs(b);
    }

    // 두 배열의 동일 인덱스에 있는 값 계산하기
    //[BurstCompile]
    public struct ParallelCalcJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> a; // 읽기 전용
        [ReadOnly] public NativeArray<float> b;
        [WriteOnly] public NativeArray<float> result; // 결과 저장

        public void Execute(int i)
        {
            //result[i] = JustAdd(a[i], b[i]);
            result[i] = SomeCalc(a[i], b[i]);
        }
    }

    // 일반적인 수행
    private void TestCommon()
    {
        // 배열 생성
        float[] a = new float[Size];
        float[] b = new float[Size];
        float[] result = new float[Size];

        // 배열 초기화
        for (int i = 0; i < Size; i++)
        {
            a[i] = i;
            b[i] = i * 2;
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // 계산
        for (int i = 0; i < Size; i++)
        {
            //result[i] = JustAdd(a[i], b[i]);
            result[i] = SomeCalc(a[i], b[i]);
        }

        sw.Stop();
        Debug.Log($"Common : {sw.ElapsedMilliseconds}");
    }

    // 잡으로 수행
    private void TestJob(int batch)
    {
        // 배열 생성
        float[] a = new float[Size];
        float[] b = new float[Size];

        // 배열 초기화
        for (int i = 0; i < Size; i++)
        {
            a[i] = i;
            b[i] = i * 2;
        }

        // 네이티브 배열 생성
        NativeArray<float> arrayA = new NativeArray<float>(a, Allocator.TempJob);
        NativeArray<float> arrayB = new NativeArray<float>(b, Allocator.TempJob);
        NativeArray<float> result = new NativeArray<float>(Size, Allocator.TempJob);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // 계산
        ParallelCalcJob job = new ParallelCalcJob { a = arrayA, b = arrayB, result = result };
        JobHandle handle = job.Schedule(result.Length, batch);
        handle.Complete();

        sw.Stop();
        Debug.Log($"Job [Batch {batch} ] : {sw.ElapsedMilliseconds}");

        // 해제
        arrayA.Dispose();
        arrayB.Dispose();
        result.Dispose();
    }


    private void Start()
    {
        // 테스트 이전에 한 번씩 실행
        TestCommon();
        TestJob(1);

        // 테스트
        TestCommon();
        TestJob(1);
        TestJob(2);
        TestJob(4);
        TestJob(8);
        TestJob(16);
        TestJob(32);
    }
}