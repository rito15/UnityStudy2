using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-26 PM 6:47:20
// 작성자 : Rito

namespace Rito.VoxelSystem
{
    /// <summary> 메시 청크 컴포넌트 </summary>
    public class Chunk : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        private void Start()
        {
            int vertexIndex = 0;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            // 6방향의 면 그리기
            for (int p = 0; p < 6; p++)
            {
                // 각 면의 삼각형 2개 그리기
                for (int i = 0; i < 6; i++)
                {
                    int triangleIndex = VoxelData.voxelTris[p, i];

                    vertices.Add(VoxelData.voxelVerts[triangleIndex]);
                    triangles.Add(vertexIndex);
                    uvs.Add(VoxelData.voxelUvs[i]);

                    vertexIndex++;
                }
            }

            // 메시에 데이터들 초기화
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray()
            };

            mesh.RecalculateNormals(); // 필수

            meshFilter.mesh = mesh;
        }
    }
}