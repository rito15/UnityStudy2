using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// 날짜 : 2021-09-07 PM 3:03:10
// 작성자 : Rito

namespace Rito.Tests
{
    public class Test_ScreenShot : MonoBehaviour
    {
        public Button screenShotButton;

        private Camera _camera;
        private bool _willTakeScreenShot = false;

        private void Awake()
        {
            screenShotButton.onClick.AddListener(TakeScreenShot);
            _camera = GetComponent<Camera>();
        }

        private void TakeScreenShot()
        {
            _willTakeScreenShot = true;
            _camera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        }

        private void OnPostRender()
        {
            if (_willTakeScreenShot == false) return;
            else _willTakeScreenShot = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            string rootPath = Application.dataPath;
#elif UNITY_ANDROID
            string rootPath = Application.persistentDataPath;
#endif
            string folderName = "ScreenShots";
            string fileName = "MyScreenShot.png";

            string folderPath = $"{rootPath}/{folderName}";
            string totalPath = $"{folderPath}/{fileName}";

            RenderTexture rt = _camera.targetTexture;

            Texture2D screenTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            Rect area = new Rect(0f, 0f, rt.width, rt.height);

            // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
            screenTex.ReadPixels(area, 0, 0);
            screenTex.Apply();

            bool succeeded = true;
            try
            {
                // 폴더가 존재하지 않으면 새로 생성
                if (System.IO.Directory.Exists(folderPath) == false)
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                // 스크린샷 저장
                System.IO.File.WriteAllBytes(totalPath, screenTex.EncodeToPNG());
            }
            catch(Exception e)
            {
                succeeded = false;
                Debug.Log($"Screen Shot Save Failed : {totalPath}");
                Debug.Log(e);
            }

            // 마무리 작업
            _camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            Destroy(screenTex);

            if(succeeded)
                Debug.Log($"Screen Shot Saved : {totalPath}");
        }
    }
}