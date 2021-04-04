using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

// 날짜 : 2021-04-04 PM 8:52:03
// 작성자 : Rito

namespace Rito.JobTest
{
    [BurstCompile]
    public struct TerrainJob : IJob
    {
        public NativeArray<Vector3> verts;
        public NativeArray<int> tris;
        public int resolution;
        public float width, maxHeight, noiseScale;
        public Vector3 startPos;

        public TerrainJob(int resolution, float width, float maxHeight, float noiseScale, Vector3 startPos)
        {
            verts = new NativeArray<Vector3>(resolution * resolution, Allocator.TempJob);
            tris = new NativeArray<int>((resolution - 1) * (resolution - 1) * 6, Allocator.TempJob);
            this.width = width;
            this.resolution = resolution;
            this.maxHeight = maxHeight;
            this.noiseScale = noiseScale;
            this.startPos = startPos;
        }

        public void Execute()
        {
            float xzUnit = width / resolution;

            Vector3 curVertPos = startPos;

            // 1. 버텍스 생성
            int vertIndex = 0;
            for (int z = 0; z < resolution; z++)
            {
                curVertPos.x = startPos.x;

                for (int x = 0; x < resolution; x++)
                {
                    curVertPos.y = GetPerlinHeight(curVertPos.x, curVertPos.z, noiseScale) * maxHeight;
                    verts[vertIndex++] = curVertPos;

                    curVertPos.x += xzUnit;
                }

                curVertPos.z += xzUnit;
            }

            // 2. 폴리곤 조립
            int triIndex = 0;
            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int LB = x + (z * resolution); // LB Index

                    tris[triIndex    ] = LB;
                    tris[triIndex + 1] = LB + resolution;
                    tris[triIndex + 2] = LB + 1;

                    tris[triIndex + 3] = LB + 1;
                    tris[triIndex + 4] = LB + resolution;
                    tris[triIndex + 5] = LB + resolution + 1;

                    triIndex += 6;
                }
            }
        }

        public (Vector3[] verts, int[] tris) GetResults()
        {
            var result = (verts.ToArray(), tris.ToArray());
            verts.Dispose();
            tris.Dispose();

            return result;
        }

        private float GetPerlinHeight(float x, float y, float scale)
        {
            return Mathf.PerlinNoise(x / scale + 0.1f, y / scale + 0.1f);
        }
    }
}