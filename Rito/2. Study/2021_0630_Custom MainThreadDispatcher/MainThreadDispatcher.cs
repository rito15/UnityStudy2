/*
    Copyright 2015 Pim de Witte All Rights Reserved.

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

    https://github.com/PimDeWitte/UnityMainThreadDispatcher
*/

// 날짜 : 2021-06-30 AM 2:56:52

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rito
{
    using MTD = MainThreadDispatcher;

    /// <summary> 
    /// 워커스레드로부터 작업을 받아 처리하는 싱글톤 모노비헤이비어
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        /***********************************************************************
        *                               Singleton
        ***********************************************************************/
        #region .

        private static MTD _instance;
        public static MTD Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MTD>();

                    if (_instance == null)
                    {
                        GameObject container = new GameObject($"Main Thread Dispatcher");
                        _instance = container.AddComponent<MTD>();
                    }
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(this);
            }
            else
            {
                if (_instance != this)
                {
                    if (GetComponents<Component>().Length <= 2)
                        Destroy(gameObject);
                    else
                        Destroy(this);
                }
            }
        }

        #endregion

        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        private void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        /// <summary> 메인 스레드에 작업 요청(코루틴) </summary>
        public void Request(IEnumerator action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() => 
                {
                    StartCoroutine(action);
                });
            }
        }

        /// <summary> 메인 스레드에 작업 요청(메소드) </summary>
        public void Request(Action action)
        {
            Request(ActionWrapper(action));
        }

        /// <summary> 메인 스레드에 작업 요청 및 대기(await) </summary>
        public Task RequestAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            void WrappedAction()
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            Request(ActionWrapper(WrappedAction));
            return tcs.Task;
        }

        /// <summary> 메인 스레드에 작업 요청 및 대기(await) + 값 받아오기 </summary>
        public Task<T> RequestAsync<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            void WrappedAction()
            {
                try
                {
                    var result = action();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            Request(ActionWrapper(WrappedAction));
            return tcs.Task;
        }

        /// <summary> Action -> IEnumerator </summary>
        private IEnumerator ActionWrapper(Action a)
        {
            a();
            yield return null;
        }
    }
}