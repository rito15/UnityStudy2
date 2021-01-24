using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-21 PM 5:30:55
// 작성자 : Rito

namespace Rito.FogOfWar
{
    /// <summary> 타일의 (x, y) 좌표 </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    public struct TilePos
    {
        [FieldOffset(0)] public int x;
        [FieldOffset(4)] public int y;
        [FieldOffset(8)] public float height;

        public TilePos(int x, int y, float height)
        {
            this.x = x;
            this.y = y;
            this.height = height;
        }
        public int Distance(TilePos other)
        {
            int distX = other.x - x;
            int distY = other.y - x;
            return (distX * distX) + (distY * distY);
        }

        /// <summary> (x, y) 인덱스를 일차원배열의 인덱스로 변환 </summary>
        public int GetTileIndex(in int mapWidth)
        {
            return x + y * mapWidth;
        }
    }
}