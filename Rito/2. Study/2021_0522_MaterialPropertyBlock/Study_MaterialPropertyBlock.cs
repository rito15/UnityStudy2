using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-05-22 PM 6:11:32
// 작성자 : Rito

namespace Rito
{
    public class Study_MaterialPropertyBlock : MonoBehaviour
    {
        [SerializeField] private bool _useMaterialPropertyBlock = false;
        [SerializeField] private bool _update = false;

        private MeshRenderer[] renderers;
        private MaterialPropertyBlock mpb;
        private MaterialPropertyBlock[] mpbArr;

        private void Start()
        {
            renderers = GetComponentsInChildren<MeshRenderer>();
            mpb = new MaterialPropertyBlock();

            // 렌더러마다 MPB 객체 하나씩 생성하여 사용
            {
                mpbArr = new MaterialPropertyBlock[renderers.Length];
                for (int i = 0; i < mpbArr.Length; i++)
                {
                    mpbArr[i] = new MaterialPropertyBlock();
                }
            }

            if (!_useMaterialPropertyBlock)
                SetRandomProperty();
            else
                SetRandomPropertyWithMPB();
        }

        float t;
        private void Update()
        {
            if(!_update) return;

            t += Time.deltaTime;
            if (t >= 0.5f)
            {
                t -= 0.5f;
                SetRandomPropertyWithMPB();
            }
        }

        private void SetRandomProperty()
        {
            foreach (var r in renderers)
            {
                r.material.SetColor("_Color", UnityEngine.Random.ColorHSV());
                r.material.SetFloat("_Metallic", UnityEngine.Random.Range(0f, 1f));
            }
        }

        private void SetRandomPropertyWithMPB()
        {
            //foreach (var r in renderers)
            //{
            //    mpb.SetColor("_Color", UnityEngine.Random.ColorHSV());
            //    mpb.SetFloat("_Metallic", UnityEngine.Random.Range(0f, 1f));
            //    r.SetPropertyBlock(mpb);
            //}

            for (int i = 0; i < renderers.Length; i++)
            {
                mpbArr[i].SetColor("_Color", UnityEngine.Random.ColorHSV());
                mpbArr[i].SetFloat("_Metallic", UnityEngine.Random.Range(0f, 1f));
                renderers[i].SetPropertyBlock(mpbArr[i]);
            }
        }
    }
}