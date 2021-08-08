
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 날짜 : 2021-07-06 PM 3:02:23
// 작성자 : Rito

namespace Rito.Tests
{
    /// <summary> 
    /// 비동기 씬 로드
    /// </summary>
    public class Test_AsyncSceneLoader : MonoBehaviour
    {
        /***********************************************************************
        *                               Inspector Fields
        ***********************************************************************/
        #region .
        public string _nextSceneName;

        public Image _loadingBar;
        public Button _LoadNextSceneButton;
        public Button _moveToNextSceneButton;

        /// <summary> 최소 로딩 소요 시간 </summary>
        [Space]
        public float _minLoadDuretion = 4.0f;

        #endregion
        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .
        private AsyncOperation _loadOperation;

        public bool LoadCompleted
        {
            get => _loadCompleted;
            set
            {
                _loadCompleted = value;
                _moveToNextSceneButton.interactable = value;
            }
        }
        private bool _loadCompleted;

        public float LoadRatio
        {
            get => _loadRatio;
            set
            {
                _loadRatio = value;
                _loadingBar.fillAmount = value;
            }
        }
        private float _loadRatio;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            LoadCompleted = false;
            LoadRatio = 0.0f;

            _LoadNextSceneButton.onClick.AddListener(LoadNextScene);
            _moveToNextSceneButton.onClick.AddListener(MoveToNextScene);
            _moveToNextSceneButton.gameObject.SetActive(false);
        }

        #endregion
        /***********************************************************************
        *                               Methods
        ***********************************************************************/
        #region .
        private void LoadNextScene()
        {
            _LoadNextSceneButton.gameObject.SetActive(false);
            _moveToNextSceneButton.gameObject.SetActive(true);
            StartCoroutine(LoadSceneRoutine());
        }

        private void MoveToNextScene()
        {
            _loadOperation.allowSceneActivation = true;
        }

        /// <summary> 로 딩 루 틴 </summary>
        private IEnumerator LoadSceneRoutine()
        {
            /*
             * - 로딩 구성 : 진짜 비동기 로딩 + 가짜 로딩 => 최소값으로 로딩 게이지에 설정
             * - 가짜 로딩 쓰는 이유 : 최소 로딩 시간을 지정해서 좀더 로딩 같아 보이도록(테스트용)
             * 
             * - Note : _loadOperation.allowSceneActivation이 false인 동안에는
             *          _loadOperation.progress 값이 0.9까지만 상승
             *          그리고 allowSceneActivation이 true가 되면 즉시 로딩됨
             * 
             */

            // 진짜 로딩
            _loadOperation = SceneManager.LoadSceneAsync(_nextSceneName);
            _loadOperation.allowSceneActivation = false;

            // 가짜 로딩
            float fakeLoadTime = 0f;
            float fakeLoadRatio = 0f;

            while (!_loadOperation.isDone)
            {
                // 일부러 최소 로딩 시간 지키도록 
                if (fakeLoadRatio < 1.0f)
                {
                    fakeLoadTime += Time.deltaTime;
                    fakeLoadRatio = fakeLoadTime / _minLoadDuretion;
                }

                // 실제 로딩과 가짜 로딩 중 최소값으로 로딩률 지정
                LoadRatio = Mathf.Min(_loadOperation.progress + 0.1f, fakeLoadRatio);
                Debug.Log("Load.." + _loadOperation.progress);

                if (LoadRatio >= 1.0f)
                    break;

                yield return null;
            }

            // Finished
            LoadRatio = 1.0f;
            LoadCompleted = true;
        }

        #endregion
    }
}