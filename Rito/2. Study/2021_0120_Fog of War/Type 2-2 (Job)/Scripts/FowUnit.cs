using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWarJob
{
    /// <summary> 안개 적용 대상 유닛 </summary>
    public class FowUnit : MonoBehaviour
    {
        /// <summary> xz 시야 범위 </summary>
        public float sightRange = 5;

        /// <summary> y 시야 범위 </summary>
        public float sightHeight = 0.5f;

        void OnEnable() => FowManager.AddUnit(this);

        private void OnDisable() => FowManager.RemoveUnit(this);
        private void OnDestroy() => FowManager.RemoveUnit(this);
    }
}
