using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-21 PM 5:30:55
// 작성자 : Rito

namespace Rito.FogOfWar
{
    /// <summary> 타일의 (x, y) 좌표 </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
    public struct TilePos
    {
        [FieldOffset(0)] public int x;
        [FieldOffset(4)] public int y;

        public TilePos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int Distance(TilePos other)
        {
            int distX = other.x - x;
            int distY = other.y - x;
            return (distX * distX) + (distY * distY);
        }

        /// <summary> (x, y) 인덱스를 일차원배열의 인덱스로 변환 </summary>
        public int ConvertToTileIndex(in int mapWidth)
        {
            return x + y * mapWidth;
        }

        public static TilePos operator + (TilePos pos1, TilePos pos2)
            => new TilePos(pos1.x + pos2.x, pos1.y + pos2.y);

        public static TilePos operator + (TilePos pos1, (int x, int y) pos2)
            => new TilePos(pos1.x + pos2.x, pos1.y + pos2.y);
    }
}