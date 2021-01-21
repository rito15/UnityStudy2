using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    public class FowFogRenderer : MonoBehaviour
    {
        public FowManager FM => FowManager.Instance;
        public GameObject rendererPrefab;
        Material material;

        void Start()
        {
            var renderer = Instantiate(rendererPrefab, transform);
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = new Vector3(FM._fogWidthX * 0.5f, 1, FM._fogWidthZ * 0.5f);
            material = renderer.GetComponentInChildren<Renderer>().material;
        }

        void Update()
        {
            if (FM.Map.FogTexture != null)
            {
                material.SetTexture("_MainTex", FM.Map.FogTexture);
            }
        }
    }

}
