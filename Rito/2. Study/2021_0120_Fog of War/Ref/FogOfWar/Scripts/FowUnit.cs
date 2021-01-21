using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    /// <summary> 안개 적용 대상 유닛 </summary>
    public class FowUnit : MonoBehaviour
    {
        public float sightRange = 5;

        void OnEnable() => FowManager.AddUnit(this);

        private void OnDisable() => FowManager.RemoveUnit(this);
        private void OnDestroy() => FowManager.RemoveUnit(this);
    }
}
