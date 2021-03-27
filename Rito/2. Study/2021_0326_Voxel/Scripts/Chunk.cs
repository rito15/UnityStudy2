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
        /***********************************************************************
        *                               Public Fields
        ***********************************************************************/
        #region .
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        private int vertexIndex = 0;
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

        /// <summary> Solid 여부(해당 좌표에 복셀 큐브가 존재해야 하는지 여부(true/false))를 갖는 맵 데이터 </summary>
        private bool [,,] voxelMap = new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Start()
        {
            PopulateVoxelMap();
            CreateMeshData();
            CreateMesh();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 복셀 맵의 Solid 정보 데이터 생성 </summary>
        private void PopulateVoxelMap()
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.ChunkWidth; z++)
                    {
                        voxelMap[x, y, z] = true;
                    }
                }
            }
        }

        /// <summary> 복셀 면 그려내기 </summary>
        private void CreateMeshData()
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.ChunkWidth; z++)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
                }
            }
        }

        /// <summary> 해당 좌표가 Solid인지 여부 검사(해당 좌표에 복셀 큐브가 존재하는지 여부) </summary>
        private bool CheckVoxel(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);

            // 맵 범위를 벗어나는 경우
            if(x < 0 || x > VoxelData.ChunkWidth - 1 || 
               y < 0 || y > VoxelData.ChunkHeight - 1 || 
               z < 0 || z > VoxelData.ChunkWidth - 1)
                return false;

            return voxelMap[x, y, z];
        }

        /// <summary> 해당 좌표에서 복셀 큐브의 메시 데이터 추가 </summary>
        private void AddVoxelDataToChunk(Vector3 pos)
        {
            // 6방향의 면 그리기
            // p : -Z, +Z, +Y, -Y, -X, +X 순서로 이루어진, 큐브의 각 면에 대한 인덱스
            for (int p = 0; p < 6; p++)
            {
                // Face Check(면이 바라보는 방향으로 +1 이동하여 확인)를 했을 때 
                // Solid가 아닌 경우에만 큐브의 면이 그려지도록 하기
                // => 청크의 외곽 부분만 면이 그려지고, 내부에는 면이 그려지지 않도록
                if (CheckVoxel(pos) && !CheckVoxel(pos + VoxelData.faceChecks[p]))
                {
                    // 각 면(삼각형 2개) 그리기

                    // 1. Vertex, UV 4개 추가
                    for (int i = 0; i <= 3; i++)
                    {
                        vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, i]] + pos);
                        uvs.Add(VoxelData.voxelUvs[i]);
                    }

                    // 2. Triangle의 버텍스 인덱스 6개 추가
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);

                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);

                    vertexIndex += 4;
                }
            }
        }

        /// <summary> vertex, triangle, uv 데이터 이용해 메시 생성  </summary>
        private void CreateMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray()
            };

            mesh.RecalculateNormals(); // 필수

            meshFilter.mesh = mesh;
        }

        #endregion
    }
}