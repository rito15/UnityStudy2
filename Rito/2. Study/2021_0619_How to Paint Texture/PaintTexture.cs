using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-06-19 PM 5:45:10
// 작성자 : Rito

namespace Rito
{
    /*
     * - 그림 그릴 대상 게임오브젝트에 컴포넌트로 넣기
     * 
     */
    public class PaintTexture : MonoBehaviour
    {
        public int resolution = 512;
        [Range(0.01f, 1f)]
        public float brushSize = 0.1f;
        public Texture2D brushTexture;

        private Texture2D mainTex;
        private MeshRenderer mr;
        private RenderTexture rt;

        private void Awake()
        {
            TryGetComponent(out mr);
            rt = new RenderTexture(resolution, resolution, 32);

            if (mr.material.mainTexture != null)
            {
                mainTex = mr.material.mainTexture as Texture2D;
            }
            // 메인 텍스쳐가 없을 경우, 하얀 텍스쳐를 생성하여 사용
            else
            {
                mainTex = new Texture2D(resolution, resolution);
            }

            // 메인 텍스쳐 -> 렌더 텍스쳐 복제
            Graphics.Blit(mainTex, rt);

            // 렌더 텍스쳐를 메인 텍스쳐에 등록
            mr.material.mainTexture = rt;

            // 브러시 텍스쳐가 없을 경우 임시 생성(red 색상)
            if (brushTexture == null)
            {
                brushTexture = new Texture2D(resolution, resolution);
                for (int i = 0; i < resolution; i++)
                    for (int j = 0; j < resolution; j++)
                        brushTexture.SetPixel(i, j, Color.red);
                brushTexture.Apply();
            }
        }

        private void Update()
        {
            // NOTE : 텍스쳐 페인팅의 대상이 될 모든 컴포넌트에서 레이캐스트 검사를 수행하므로 비효율적
            // 실제로 사용하려면 하나의 컴포넌트에서 레이캐스트 수행하도록 구조 변경

            // 마우스 클릭 지점에 브러시로 그리기
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool raycast = Physics.Raycast(ray, out var hit);
                Collider col = hit.collider;

                //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1f);

                // 본인이 레이캐스트에 맞았으면 그리기
                if (raycast && col && col.transform == transform)
                {
                    Vector2 pixelUV = hit.lightmapCoord;
                    pixelUV *= resolution;
                    DrawTexture(pixelUV);
                }
            }
        }

        /// <summary> 렌더 텍스쳐에 브러시 텍스쳐로 그리기 </summary>
        public void DrawTexture(in Vector2 uv)
        {
            RenderTexture.active = rt; // 페인팅을 위해 활성 렌더 텍스쳐 임시 할당
            GL.PushMatrix();                                  // 매트릭스 백업
            GL.LoadPixelMatrix(0, resolution, resolution, 0); // 알맞은 크기로 픽셀 매트릭스 설정

            float brushPixelSize = brushSize * resolution;

            // 렌더 텍스쳐에 브러시 텍스쳐를 이용해 그리기
            Graphics.DrawTexture(
                new Rect(
                    uv.x - brushPixelSize * 0.5f,
                    (rt.height - uv.y) - brushPixelSize * 0.5f,
                    brushPixelSize,
                    brushPixelSize
                ),
                brushTexture
            );

            GL.PopMatrix();              // 매트릭스 복구
            RenderTexture.active = null; // 활성 렌더 텍스쳐 해제
        }
    }
}