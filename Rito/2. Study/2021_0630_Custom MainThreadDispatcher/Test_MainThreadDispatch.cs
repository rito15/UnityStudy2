using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

// 날짜 : 2021-06-30 AM 3:38:28
// 작성자 : Rito

namespace Rito.Tests
{
    using Random = UnityEngine.Random;

    public class Test_MainThreadDispatch : MonoBehaviour
    {
        private MainThreadDispatcher mtd;

        private void Start()
        {
            mtd = MainThreadDispatcher.Instance;

            Task.Run(() => TestBody());
        }

        private async void TestBody()
        {
            int res1 = -1, res2 = -1;

            await Task.Delay(500);

            // 1. 비동기 수행
            mtd.Request(() => res1 = Random.Range(0, 1000));
            Debug.Log(res1);

            await Task.Delay(500);

            // 2. 대기 - Action
            await mtd.RequestAsync(() => { res2 = Random.Range(0, 1000); });
            Debug.Log(res2);

            await Task.Delay(500);

            // 3. 대기 - Func<int>
            Task<int> resultTask = mtd.RequestAsync(() => Random.Range(0, 1000));
            await resultTask;
            Debug.Log(resultTask.Result);
        }
    }
}