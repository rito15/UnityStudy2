using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

// 날짜 : 2021-06-26 PM 3:36:40
// 작성자 : Rito

namespace Rito.Tests
{
    public class MemoryTestController : MonoBehaviour
    {
        public string nextSceneName;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveNextScene();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                AdditionallyLoadNextScene();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                UnloadAdditionalScene();
            }
        }

        private void MoveNextScene()
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }

        private void AdditionallyLoadNextScene()
        {
            if(!IsAlreadyLoaded())
                SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        }

        private void UnloadAdditionalScene()
        {
            if (IsAlreadyLoaded())
                SceneManager.UnloadSceneAsync(nextSceneName);
        }

        private bool IsAlreadyLoaded()
        {
            return SceneManager.GetSceneByName(nextSceneName).IsValid();
        }
    }
}