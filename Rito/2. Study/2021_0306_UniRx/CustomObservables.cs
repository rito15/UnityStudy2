using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// 날짜 : 2021-04-13 PM 5:27:28
// 작성자 : Rito

namespace Rito.UniRx
{
    public class CustomObservables : MonoBehaviour
    {
        /***********************************************************************
        *                               Singleton
        ***********************************************************************/
        #region .

        public static CustomObservables Instance
        {
            get
            {
                if(_instance == null)
                    CreateSingletonInstance();

                return _instance;
            }
        }
        private static CustomObservables _instance;

        /// <summary> 싱글톤 인스턴스 생성 </summary>
        private static void CreateSingletonInstance()
        {
            GameObject go = new GameObject("Custom Observables (Singleton Instance)");
            _instance = go.AddComponent<CustomObservables>();
            DontDestroyOnLoad(go);
        }

        /// <summary> 싱글톤 인스턴스를 유일하게 유지 </summary>
        private void CheckSingletonInstance()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .

        private float _deltaTime;

        private void Awake()
        {
            CheckSingletonInstance();
            MouseDoubleClickAsObservable = _mouseDoubleClickSubject.AsObservable();
        }

        private void Update()
        {
            _deltaTime = Time.deltaTime;
            CheckDoubleClick();
        }

        #endregion
        /***********************************************************************
        *                           Mouse Double Click Checker
        ***********************************************************************/
        #region .
        public IObservable<Unit> MouseDoubleClickAsObservable { get; private set; }
        private Subject<Unit> _mouseDoubleClickSubject = new Subject<Unit>();

        private bool _checkingDoubleClick;
        private float _doubleClickTimer;
        private const float DoubleClickThreshold = 0.3f;

        private void CheckDoubleClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _doubleClickTimer = 0f;
                if (!_checkingDoubleClick)
                {
                    _checkingDoubleClick = true;
                }
                else
                {
                    _checkingDoubleClick = false;
                    _mouseDoubleClickSubject.OnNext(Unit.Default);
                }
            }

            if (_checkingDoubleClick)
            {
                if (_doubleClickTimer >= DoubleClickThreshold)
                {
                    _checkingDoubleClick = false;
                }
                else
                {
                    _doubleClickTimer += _deltaTime;
                }
            }
        }

        #endregion
    }
}