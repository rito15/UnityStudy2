using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-26 PM 6:42:13
// 작성자 : Rito

namespace Rito.VoxelSystem
{
    public static class VoxelData
    {
        /***********************************************************************
        *                               Lookup Tables
        ***********************************************************************/
        #region .

        /* 
                7 ──── 6    
              / │       / │
            3 ──── 2   │
            │  │     │  │
            │  4───│─5  
            │/        │/
            0 ──── 1
        */
        /// <summary> 큐브의 8개 버텍스의 상대 위치 </summary>
        public static readonly Vector3[] voxelVerts = new Vector3[8]
        {
            // Front
            new Vector3(0.0f, 0.0f, 0.0f), // LB
            new Vector3(1.0f, 0.0f, 0.0f), // RB
            new Vector3(1.0f, 1.0f, 0.0f), // RT
            new Vector3(0.0f, 1.0f, 0.0f), // LT

            // Back
            new Vector3(0.0f, 0.0f, 1.0f), // LB
            new Vector3(1.0f, 0.0f, 1.0f), // RB
            new Vector3(1.0f, 1.0f, 1.0f), // RT
            new Vector3(0.0f, 1.0f, 1.0f), // LT
        };

        // 한 면을 이루는 삼각형은 2개
        // 버텍스 인덱스는 시계방향으로 배치(전면으로 그려지도록)
        // 각 면의 버텍스 순서는 해당 면을 기준으로 LB-LT-RB, RB-LT-RT
        /*
            LB-LT-RB   RB-LT-RT

            1          1 ㅡ 2
            | ＼         ＼ |
            0 ㅡ 2          0
        */
        /// <summary> 큐브의 각 면을 이루는 삼각형들의 버텍스 인덱스 데이터 </summary>
        public static readonly int[,] voxelTris = new int[6, 6]
        {
            {0, 3, 1, 1, 3, 2 }, // Back Face   (-Z)
            {5, 6, 4, 4, 6, 7 }, // Front Face  (+Z)
            {3, 7, 2, 2, 7, 6 }, // Top Face    (+Y)
            {1, 5, 0, 0, 5, 4 }, // Bottom Face (-Y)
            {4, 7, 0, 0, 7, 3 }, // Left Face   (-X)
            {1, 2, 5, 5, 2, 6 }, // RIght Face  (+X)
        };

        /// <summary> voxelTris의 버텍스 인덱스 순서에 따라 정의된 UV 좌표 데이터 </summary>
        public static readonly Vector2[] voxelUvs = new Vector2[6]
        {
            new Vector2(0.0f, 0.0f), // LB
            new Vector2(0.0f, 1.0f), // LT
            new Vector2(1.0f, 0.0f), // RB

            new Vector2(1.0f, 0.0f), // RB
            new Vector2(0.0f, 1.0f), // LT
            new Vector2(1.0f, 1.0f), // RT
        };

        #endregion
    }
}