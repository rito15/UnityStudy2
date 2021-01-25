using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-21 PM 5:04:10
// 작성자 : Rito
namespace Rito.FogOfWar
{
    public class FowTile
    {
        /// <summary> 타일 (x,y) 좌표 </summary>
        public TilePos pos;

        /// <summary> (x,y) 좌표, width를 이용해 계산한 일차원 배열 내 인덱스 </summary>
        public int index;

        public int X => pos.x;
        public int Y => pos.y;
        /// <summary> 해당 타일이 위치한 곳의 지형 높이 </summary>
        public float Height => pos.height;

        public FowTile(float height, int x, int y, int width)
        {
            pos.x = x;
            pos.y = y;
            pos.height = height;

            index = x + y * width;
        }

        public int Distance(FowTile other)
        {
            int distX = other.X - X;
            int distY = other.Y - Y;
            return (distX * distX) + (distY * distY);
        }
    }
}