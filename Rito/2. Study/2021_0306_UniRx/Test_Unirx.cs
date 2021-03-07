using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
using UniRx.Triggers;

using Rito.UnityLibrary.Tester;
using Debug = Rito.UnityLibrary.Debug;

using System.Threading;

// 날짜 : 2021-03-06 PM 8:44:40
// 작성자 : Rito

public class Test_Unirx : MonoBehaviour
{
    public int _repeatCount = 100000;
    public float _fValue = 1f;

    private void Start()
    {
        //MainThreadDispatcher.StartCoroutine(TestRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Calculate();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            CalculateUniRxMultithread();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CalculateCSharpMultithread();
        }
    }

    private void Calculate()
    {
        int[] arr = new int[_repeatCount];

        RitoWatch.BeginCheck("Calculate");
        for (int i = 0; i < _repeatCount; i++)
        {
            arr[i] = i * i + i / (i + 1);
        }

        RitoWatch.EndCheck();
        RitoWatch.PrintAllLogs();
        RitoWatch.Clear();
    }

    private void CalculateUniRxMultithread()
    {
        int threadCount = 4;
        int range = _repeatCount / threadCount;

        int[] ranges = new int[threadCount + 1];
        for (int i = 0; i < ranges.Length; i++)
        {
            ranges[i] = i * range;
        }

        int[] arr = new int[_repeatCount];

        List<IObservable<Unit>> threadList = new List<IObservable<Unit>>();

        for (int j = 0; j < threadCount; j++)
        {
            threadList.Add(
                Observable.Start(() =>
                {
                    for (int i = ranges[j]; i < ranges[j + 1]; i++)
                    {
                        arr[i] = i * i + i / (i + 1);
                    }
                })
            );
        }

        RitoWatch.BeginCheck("Calculate With Unirx Threads");

        Observable.WhenAll(threadList)
            .ObserveOnMainThread()
            .Subscribe();

        RitoWatch.EndCheck();
        RitoWatch.PrintAllLogs();
        RitoWatch.Clear();
    }

    private void CalculateCSharpMultithread()
    {
        int[] arr = new int[_repeatCount];

        ThreadPool.SetMaxThreads(4, 4);

        RitoWatch.BeginCheck("Calculate With Unirx Threads");

        for (int i = 0; i < _repeatCount; i++)
        {
            ThreadPool.QueueUserWorkItem(_ =>
                {
                    arr[i] = i * i + i / (i + 1);
                }
            );
        }
        RitoWatch.EndCheck();
        RitoWatch.PrintAllLogs();
        RitoWatch.Clear();
    }

    private void CheckDoubleClick()
    {
        // 좌클릭 입력을 감지하는 스트림 생성
        var dbClickStream = 
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0));

        // 스트림의 동작 정의, 종료 가능한 객체 반환
        var dbClickStreamDisposable =
            dbClickStream
                .Buffer(dbClickStream.Throttle(TimeSpan.FromMilliseconds(250)))
                .Where(xs => xs.Count >= 2)
                //.TakeUntilDisable(this) // 게임오브젝트 비활성화 시 스트림 종료
                .Subscribe(
                    xs => Debug.Log("DoubleClick Detected! Count:" + xs.Count), // OnNext
                    _  => Debug.Log("DoubleClick Stream - Error Detected"),     // OnError
                    () => Debug.Log("DoubleClick Stream - Disposed")            // OnCompleted
                );

        // 스트림 종료
        //dbClickStreamDisposable.Dispose();
    }

    private IEnumerator TestRoutine()
    {
        //Debug.Log("Coroutine Begin");
        yield return new WaitForSeconds(1f);
        //Debug.Log("Coroutine End");
    }
}