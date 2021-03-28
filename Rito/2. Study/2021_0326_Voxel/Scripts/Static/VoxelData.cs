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
        *                               Const Fields
        ***********************************************************************/
        #region .
        public const int BackFace   = 0;
        public const int FrontFace  = 1;
        public const int TopFace    = 2;
        public const int BottomFace = 3;
        public const int LeftFace   = 4;
        public const int RightFace  = 5;

        #endregion
        /***********************************************************************
        *                               Static Fields
        ***********************************************************************/
        #region .
        public static readonly int ChunkWidth = 5;
        public static readonly int ChunkHeight = 5;

        // 텍스쳐 아틀라스의 가로, 세로 텍스쳐 개수
        public static readonly int TextureAtlasWidth = 9;
        public static readonly int TextureAtlasHeight = 10;

        // 텍스쳐 아틀라스 내에서 각 행, 열마다 텍스쳐가 갖는 크기 비율
        public static float NormalizedTextureAtlasWidth
            => 1f / TextureAtlasWidth;
        public static float NormalizedTextureAtlasHeight
            => 1f / TextureAtlasHeight;

        #endregion

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
            // Back (-Z)
            new Vector3(0.0f, 0.0f, 0.0f), // LB
            new Vector3(1.0f, 0.0f, 0.0f), // RB
            new Vector3(1.0f, 1.0f, 0.0f), // RT
            new Vector3(0.0f, 1.0f, 0.0f), // LT

            // Front (+Z)
            new Vector3(0.0f, 0.0f, 1.0f), // LB
            new Vector3(1.0f, 0.0f, 1.0f), // RB
            new Vector3(1.0f, 1.0f, 1.0f), // RT
            new Vector3(0.0f, 1.0f, 1.0f), // LT
        };

        /// <summary> 면이 바라보고 있는 방향의 방향벡터 값 </summary>
        public static readonly Vector3[] faceChecks = new Vector3[6]
        {
            new Vector3( 0.0f,  0.0f, -1.0f), // Back Face   (-Z)
            new Vector3( 0.0f,  0.0f, +1.0f), // Front Face  (+Z)
            new Vector3( 0.0f, +1.0f,  0.0f), // Top Face    (+Y)
            new Vector3( 0.0f, -1.0f,  0.0f), // Bottom Face (-Y)
            new Vector3(-1.0f,  0.0f,  0.0f), // Left Face   (-X)
            new Vector3(+1.0f,  0.0f,  0.0f), // RIght Face  (+X)
        };

        /*  한 면을 이루는 삼각형은 2개
            버텍스 인덱스는 시계방향으로 배치(전면으로 그려지도록)
            각 면의 버텍스 순서는 해당 면을 기준으로 LB-LT-RB, RB-LT-RT
        
            LB-LT-RB   RB-LT-RT

            1          1 ㅡ 2
            | ＼         ＼ |
            0 ㅡ 2          0

            * 원래는 6개의 정점 인덱스가 차례대로 나열되었지만,
            * 2개의 버텍스가 중복되므로 중복 제거하여 메모리 절약

            * 각 면의 정점 4개 순서는 {LB, LT, RB, RT}
            * 실제 한 면을 조립할 때는 0-1-2, 2-1-3 으로 사용
        */
        /// <summary> 큐브의 각 면을 이루는 삼각형들의 버텍스 인덱스 데이터 </summary>
        public static readonly int[,] voxelTris = new int[6, 4]
        {
            {0, 3, 1, 2 }, // Back Face   (-Z)
            {5, 6, 4, 7 }, // Front Face  (+Z)
            {3, 7, 2, 6 }, // Top Face    (+Y)
            {1, 5, 0, 4 }, // Bottom Face (-Y)
            {4, 7, 0, 3 }, // Left Face   (-X)
            {1, 2, 5, 6 }, // RIght Face  (+X)
        };

        // UV의 정점 순서 역시 LB-LT-RB-RT로 voxelTris와 일치
        /// <summary> 정점 인덱스 순서에 해당하는 UV 좌표 데이터 </summary>
        public static readonly Vector2[] voxelUvs = new Vector2[4]
        {
            new Vector2(0.0f, 0.0f), // LB
            new Vector2(0.0f, 1.0f), // LT
            new Vector2(1.0f, 0.0f), // RB
            new Vector2(1.0f, 1.0f), // RT
        };

        #endregion
    }
}