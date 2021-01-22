using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-22 PM 4:59:37
// 작성자 : Rito
namespace Rito.FogOfWar
{
    /// <summary> 현재 존재, 과거 방문 여부 </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Visit
    {
        /// <summary> 현재 위치함 </summary>
        [FieldOffset(0)]
        public bool current;

        /// <summary> 과거에 방문한 적 있음 </summary>
        [FieldOffset(4)]
        public bool ever;
    }
}