using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    public class FowFogRenderer : MonoBehaviour
    {
        public FowManager FM => FowManager.I;
        public GameObject rendererPrefab;
        private Material material;

        void Start()
        {
            var renderer = Instantiate(rendererPrefab, transform);
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = new Vector3(FM._fogWidthX / 2, 1, FM._fogWidthZ / 2);
            material = renderer.GetComponentInChildren<Renderer>().material;
        }

        // Update is called once per frame
        void Update()
        {
            if (FM.Map.FogTexture != null)
            {
                material.SetTexture("_MainTex", FM.Map.FogTexture);
            }

        }
    }

}
