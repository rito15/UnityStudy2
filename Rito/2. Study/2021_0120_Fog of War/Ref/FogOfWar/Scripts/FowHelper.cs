using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-21 PM 5:08:40
// 작성자 : Rito

namespace Rito.FogOfWar
{
    public static class FowHelper
    {
        /// <summary> 해당 좌표가 맵 내에 있는지 검사 </summary>
        public static bool InMap(TilePos pos, int mapWidth, int mapHeight)
        {
            return pos.x >= 0 && pos.y >= 0 && 
                   pos.x < mapWidth && pos.y < mapHeight;
        }

        public static bool CantDisplay(TilePos p1, TilePos p2, float z)
        {
            if ((p1.x == 0 && p1.y == 0) || (p2.x == 0 && p2.y == 0)) return true;

            if (p1.x == 0 || p2.x == 0)
            {
                var t = p1.y;
                p1.y = p1.x;
                p1.x = t;

                t = p2.y;
                p2.y = p2.x;
                p2.x = t;
            }

            float k1 = (float)p1.y / p1.x;
            float k2 = (float)p2.y / p2.x;

            int dist = p1.Distance(p2);
            if (dist > 0)
            {
                return Angle(k1, k2) < z;
            }
            else
            {
                return false;
            }

        }

        public static float Angle(float k1, float k2)
        {
            return Mathf.Abs((k2 - k1) / (1 + k1 * k2));
        }
    }
}