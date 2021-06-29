using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

// 날짜 : 2021-06-26 PM 4:06:33
// 작성자 : Rito

namespace Rito.Tests
{
    public class Test_AddressableManager : MonoBehaviour
    {
        // 1. 애셋 레퍼런스 기반

        private AssetReferenceGameObject redCubeReference;
        private AsyncOperationHandle redCubeHandle;
        private GameObject redCubePrefab;

        private void Method()
        {
            // * Valid 검사를 수행하지 않으면
            // 이미 로드되었는데도 중복 로드될 수 있다.

            // 1-1. 애셋 레퍼런스로부터 메모리에 로드 및 핸들 참조
            if (!redCubeReference.IsValid())
                redCubeHandle = redCubeReference.LoadAssetAsync();

            // 1-2. 메모리에 로드 + 성공 시 수행할 동작 지정
            if (!redCubeReference.IsValid())
            {
                redCubeReference.LoadAssetAsync().Completed +=
                    (AsyncOperationHandle<GameObject> handle) =>
                    {
                        // 핸들 참조 등록
                        redCubeHandle = handle;

                        // 로드 완료 시 수행할 동작들
                        redCubePrefab = handle.Result;
                    };
            }


            // 2. 메모리에 적재된 애셋을 실제로 사용
            Instantiate(redCubePrefab);


            // 3-1. 애셋 레퍼런스를 기반으로 메모리에서 해제
            if (redCubeReference.IsValid())
                redCubeReference.ReleaseAsset();

            // 3-2. 핸들을 기반으로 메모리에서 해제
            if (redCubeHandle.IsValid())
            {
                Addressables.Release(redCubeHandle);
                redCubePrefab = null;
            }
        }


        // 2. 주소 기반

        private static readonly string RedCubeAddress = "Group01/Red Cube";
        //private AsyncOperationHandle redCubeHandle;
        //private GameObject redCubePrefab;

        private void Method2()
        {
            // * Valid 검사를 수행하지 않으면
            // 이미 로드되었는데도 중복 로드될 수 있다.

            // 1-1. 주소를 기반으로 메모리에 로드
            if (!redCubeHandle.IsValid())
            {
                redCubeHandle =
                    Addressables.LoadAssetAsync<GameObject>(RedCubeAddress);
            }

            // 1-2. 메모리에 로드 + 성공 시 수행할 동작 지정
            if (!redCubeHandle.IsValid())
            {
                Addressables.LoadAssetAsync<GameObject>(RedCubeAddress).Completed +=
                (AsyncOperationHandle<GameObject> handle) =>
                {
                    // 핸들 참조 등록
                    redCubeHandle = handle;

                    // 로드 완료 시 수행할 동작들
                    redCubePrefab = handle.Result;
                };
            }

            // 2. 메모리에 적재된 애셋을 실제로 사용
            Instantiate(redCubePrefab);


            // 3. 핸들을 기반으로 메모리에서 해제
            if (redCubeHandle.IsValid())
            {
                Addressables.Release(redCubeHandle);
                redCubePrefab = null;
            }
        }





        private class AddressableObject<TObject> where TObject : UnityEngine.Object
        {
            public string Address { get; private set; }
            public AsyncOperationHandle Handle { get; private set; }
            public TObject Prefab { get; private set; }

            public bool IsValid => Handle.IsValid();

            public AddressableObject(string address)
            {
                this.Address = address;
            }

            public void Load()
            {
                if (Handle.IsValid() == false)
                {
                    Addressables.LoadAssetAsync<TObject>(Address).Completed +=
                        handle =>
                        {
                            this.Handle = handle;
                            Prefab = handle.Result;
                        };
                }
            }

            public void Release()
            {
                if (Handle.IsValid())
                {
                    Addressables.Release(Handle);
                    Prefab = null;
                }
            }

            public TObject Clone()
            {
                if (IsValid == false) return null;
                return Instantiate(Prefab);
            }
        }

        private AddressableObject<GameObject> addressableExample
            = new AddressableObject<GameObject>("Group01/SpriteSet");


        public Button gameStartButton;
        public string gameSceneAddress;
        private AsyncOperationHandle gameSceneHandle;

        private void Awake()
        {

            gameStartButton.onClick.AddListener(() =>
            {
                LoadGameSceneData();
            });
        }

        private void LoadGameSceneData()
        {
            Addressables.LoadAssetAsync<SceneInstance>(gameSceneAddress).Completed +=
                handle =>
                {
                    gameSceneHandle = handle;
                };
        }

    }
}