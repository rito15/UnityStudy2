using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using System.Threading;

// 날짜 : 2021-02-03 AM 1:50:12
// 작성자 : Rito

namespace Rito.UnitaskStudy
{
    public class Test_Unitask : MonoBehaviour
    {
        private LinkedList<CancellationTokenSource> _ctsActiveList;
        private LinkedList<CancellationTokenSource> _ctsWaitList;

        /// <summary> 캔슬토큰 하나 가져오기 </summary>
        private CancellationTokenSource GetCTS()
        {
            CancellationTokenSource cts;

            if (_ctsWaitList.Count.Equals(0))
            {
                cts = new CancellationTokenSource();
            }
            else
            {
                cts = _ctsWaitList.First.Value;
                _ctsWaitList.RemoveFirst();
            }

            _ctsActiveList.AddLast(cts);
            return cts;
        }

        /// <summary> 사용한 캔슬토큰을 액티브 -> 대기로 이동 </summary>
        private void ReleaseCTS(CancellationTokenSource cts)
        {
            _ctsActiveList.Remove(cts);
            _ctsWaitList.AddLast(cts);
        }

        CancellationTokenSource _cts1;
        CancellationTokenSource _cts2;

        private void Awake()
        {
            _ctsActiveList = new LinkedList<CancellationTokenSource>();
            _ctsWaitList = new LinkedList<CancellationTokenSource>();

            _cts1 = GetCTS();
            _cts2 = GetCTS();
            TemporaryTask(_cts1).Forget();
            TemporaryTask(_cts2).Forget();
        }

        private void OnDisable()
        {
            foreach (var cts in _ctsActiveList)
            {
                cts.Cancel();
            }
            _ctsActiveList.Clear();
            _ctsWaitList.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Cancel 1");
                _cts1.Cancel();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Cancel 2");
                _cts2.Cancel();
            }
        }

        private async UniTaskVoid TemporaryTask(CancellationTokenSource cts)
        {
            int count = 0;

            while (count < 100)
            {
                Debug.Log($"Temporary : {count++}");

                try
                {
                    await UniTask.Delay(200, cancellationToken: cts.Token);
                }
                catch (OperationCanceledException)
                {
                    ReleaseCTS(cts);
                    break;
                }
            }
        }
    }
}