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

        /// <summary> 블록 타입의 인덱스를 참조하는 맵 데이터 </summary>
        private byte [,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
        private World world;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Start()
        {
            Init();
            PopulateVoxelMap();
            CreateMeshData();
            CreateMesh();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void Init()
        {
            world = FindObjectOfType<World>();
        }

        /// <summary> 복셀 맵의 Solid 정보 데이터 생성 </summary>
        private void PopulateVoxelMap()
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.ChunkWidth; z++)
                    {
                        voxelMap[x, y, z] = (byte)(y >= VoxelData.ChunkHeight - 1 ? 0 : 1);
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

        /// <summary> voxelMap으로부터 특정 위치에 해당하는 블록 ID 가져오기 </summary>
        private byte GetBlockID(in Vector3 pos)
        {
            return voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        }

        /// <summary> 해당 좌표가 Solid인지 여부 검사(해당 좌표에 복셀 큐브가 존재하는지 여부) </summary>
        private bool CheckVoxel(in Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);

            // 맵 범위를 벗어나는 경우
            if(x < 0 || x > VoxelData.ChunkWidth - 1 || 
               y < 0 || y > VoxelData.ChunkHeight - 1 || 
               z < 0 || z > VoxelData.ChunkWidth - 1)
                return false;

            return world.blockTypes[voxelMap[x, y, z]].isSolid;
        }

        /// <summary> 해당 좌표에서 복셀 큐브의 메시 데이터 추가 </summary>
        private void AddVoxelDataToChunk(in Vector3 pos)
        {
            // 6방향의 면 그리기
            // face : -Z, +Z, +Y, -Y, -X, +X 순서로 이루어진, 큐브의 각 면에 대한 인덱스
            for (int face = 0; face < 6; face++)
            {
                // Face Check(면이 바라보는 방향으로 +1 이동하여 확인)를 했을 때 
                // Solid가 아닌 경우에만 큐브의 면이 그려지도록 하기
                // => 청크의 외곽 부분만 면이 그려지고, 내부에는 면이 그려지지 않도록

                // 각 면(삼각형 2개) 그리기
                if (CheckVoxel(pos) && !CheckVoxel(pos + VoxelData.faceChecks[face]))
                {
                    byte blockID = GetBlockID(pos);

                    // 1. Vertex 4개 추가
                    for (int i = 0; i <= 3; i++)
                    {
                        vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[face, i]] + pos);
                    }

                    // 2. 텍스쳐에 해당하는 UV 추가
                    AddTextureUV(world.blockTypes[blockID].GetTextureID(face));

                    // 3. Triangle의 버텍스 인덱스 6개 추가
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

        /*
            textureID : 텍스쳐 아틀라스 내에서 해당하는 각 텍스쳐의 ID (좌상단부터 인덱스 0)

            예시 : 4x4 -> 16개의 텍스쳐로 이루어진 아틀라스

            0  1  2  3
            
            4  5  6  7

            8  9  10 11
         
            12 13 14 15
        */
        /// <summary> 텍스쳐 아틀라스 내에서 해당하는 ID의 텍스쳐가 위치한 UV를 uvs 리스트에 추가 </summary>
        private void AddTextureUV(int textureID)
        {
            // 아틀라스 내의 텍스쳐 가로, 세로 개수
            (int w, int h) = (VoxelData.TextureAtlasWidth, VoxelData.TextureAtlasHeight);

            int x = textureID % w;
            int y = h - (textureID / w) - 1;

            AddTextureUV(x, y);
        }

        // (x, y) : (0, 0) 기준은 좌하단
        /// <summary> 텍스쳐 아틀라스 내에서 (x, y) 위치의 텍스쳐 UV를 uvs 리스트에 추가 </summary>
        private void AddTextureUV(int x, int y)
        {
            // 텍스쳐 내에서 의도치 않게 들어가는 부분 잘라내기
            const float uvXBeginOffset = 0.005f;
            const float uvXEndOffset   = 0.005f;
            const float uvYBeginOffset = 0.005f;
            const float uvYEndOffset   = 0.005f;

            if (x < 0 || y < 0 || x >= VoxelData.TextureAtlasWidth || y >= VoxelData.TextureAtlasHeight)
                throw new IndexOutOfRangeException($"텍스쳐 아틀라스의 범위를 벗어났습니다 : [x = {x}, y = {y}]");

            float nw = VoxelData.NormalizedTextureAtlasWidth;
            float nh = VoxelData.NormalizedTextureAtlasHeight;

            float uvX = x * nw;
            float uvY = y * nh;

            // 해당 텍스쳐의 uv를 LB-LT-RB-RT 순서로 추가
            uvs.Add(new Vector2(uvX + uvXBeginOffset, uvY + uvYBeginOffset));
            uvs.Add(new Vector2(uvX + uvXBeginOffset, uvY + nh - uvYEndOffset));
            uvs.Add(new Vector2(uvX + nw - uvXEndOffset, uvY + uvYBeginOffset));
            uvs.Add(new Vector2(uvX + nw - uvXEndOffset, uvY + nh - uvYEndOffset));
        }

        #endregion
    }
}