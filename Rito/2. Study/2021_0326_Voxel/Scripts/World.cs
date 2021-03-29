using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-28 PM 4:46:01
// 작성자 : Rito

namespace Rito.VoxelSystem
{
    public class World : MonoBehaviour
    {
        /***********************************************************************
        *                               Public Const Fields
        ***********************************************************************/
        #region .
        public const int Air = 0;
        public const int Grass = 1;
        public const int Ground = 2;
        public const int Stone = 3;

        #endregion
        /***********************************************************************
        *                               Public Fields
        ***********************************************************************/
        #region .
        [Space]
        public Transform player;
        public Vector3 spawnPosition;

        [Space(12f)]
        public Material material;
        public BlockType[] blockTypes;

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

        // 이전 프레임에 활성화 되었던 청크 목록
        private List<Chunk> prevActiveChunkList = new List<Chunk>();

        // 현재 프레임에 활성화된 청크 목록
        private List<Chunk> currentActiveChunkList = new List<Chunk>();

        // 플레이어의 이전 프레임 위치
        private ChunkCoord prevPlayerCoord;

        // 플레이어의 현재 프레임 위치
        private ChunkCoord currentPlayerCoord;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Start()
        {
            InitPositions();
            //GenerateWorld(); // 필요 X (UpdateChunksInViewRange()에서 수행)
        }

        private void Update()
        {
            currentPlayerCoord = GetChunkCoordFromWorldPos(player.position);

            // 플레이어가 청크 위치를 이동한 경우, 시야 범위 갱신
            if(!prevPlayerCoord.Equals(currentPlayerCoord))
                UpdateChunksInViewRange();

            prevPlayerCoord = currentPlayerCoord;
        }

        #endregion
        /***********************************************************************
        *                               Get / Check Methods
        ***********************************************************************/
        #region .
        /// <summary> 해당 청크가 월드 XZ 범위 내에 있는지 검사 </summary>
        private bool IsChunkInWorld(in ChunkCoord coord)
        {
            return coord.x >= 0 && coord.x < VoxelData.WorldSizeInChunks &&
                   coord.z >= 0 && coord.z < VoxelData.WorldSizeInChunks;
        }

        /// <summary> 해당 청크 좌표가 월드 XZ 범위 내에 있는지 검사 </summary>
        private bool IsChunkPosInWorld(int x, int z)
        {
            return x >= 0 && x < VoxelData.WorldSizeInChunks &&
                   z >= 0 && z < VoxelData.WorldSizeInChunks;
        }

        // 복셀의 좌표는 각각의 복셀마다 좌하단 기준
        /// <summary> 해당 위치의 복셀이 월드 내에 있는지 검사 </summary>
        private bool IsBlockInWorld(in Vector3 worldPos)
        {
            return
                worldPos.x >= 0 && worldPos.x < VoxelData.WorldSizeInVoxels &&
                worldPos.z >= 0 && worldPos.z < VoxelData.WorldSizeInVoxels &&
                worldPos.y >= 0 && worldPos.y < VoxelData.ChunkHeight;
        }

        /// <summary> 월드 위치의 청크 좌표 리턴 </summary>
        private ChunkCoord GetChunkCoordFromWorldPos(in Vector3 worldPos)
        {
            int x = (int)(worldPos.x / VoxelData.ChunkWidth);
            int z = (int)(worldPos.z / VoxelData.ChunkWidth);
            return new ChunkCoord(x, z);
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void InitPositions()
        {
            spawnPosition = new Vector3(
                VoxelData.WorldSizeInVoxels * 0.5f,
                VoxelData.ChunkHeight,
                VoxelData.WorldSizeInVoxels * 0.5f
            );
            player.position = spawnPosition;

            prevPlayerCoord = new ChunkCoord(-1, -1);
            currentPlayerCoord = GetChunkCoordFromWorldPos(player.position);
        }

        private void GenerateWorld()
        {
            int center = VoxelData.WorldSizeInChunks / 2;
            int viewMin = center - VoxelData.ViewDistanceInChunks;
            int viewMax = center + VoxelData.ViewDistanceInChunks;

            for (int x = viewMin; x < viewMax; x++)
            {
                for (int z = viewMin; z < viewMax; z++)
                {
                    CreateNewChunk(x, z);
                }
            }
        }

        /// <summary> 해당 X, Z 청크 좌표에 새로운 청크 생성 </summary>
        private void CreateNewChunk(int x, int z)
        {
            chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        }

        /// <summary> 시야범위 내의 청크들만 유지 </summary>
        private void UpdateChunksInViewRange()
        {
            ChunkCoord coord = GetChunkCoordFromWorldPos(player.position);
            int viewDist = VoxelData.ViewDistanceInChunks;
            (int x, int z) viewMin = (coord.x - viewDist, coord.z - viewDist);
            (int x, int z) viewMax = (coord.x + viewDist, coord.z + viewDist);

            // 활성 목록 : 현재 -> 이전으로 이동
            prevActiveChunkList = currentActiveChunkList;
            currentActiveChunkList = new List<Chunk>();

            for (int x = viewMin.x; x < viewMax.x; x++)
            {
                for (int z = viewMin.z; z < viewMax.z; z++)
                {
                    // 청크 좌표가 월드 범위 내에 있는지 검사
                    if (IsChunkPosInWorld(x, z) == false)
                        continue;

                    Chunk currentChunk = chunks[x, z];

                    // 시야 범위 내에 청크가 생성되지 않은 영역이 있을 경우, 새로 생성
                    if (chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                        currentChunk = chunks[x, z]; // 참조 갱신
                    }
                    // 비활성화 되어있던 경우에는 활성화
                    else if(chunks[x, z].IsActive == false)
                    {
                        chunks[x, z].IsActive = true;
                    }

                    // 현재 활성 목록에 추가
                    currentActiveChunkList.Add(currentChunk);

                    // 이전 활성 목록에서 제거
                    if (prevActiveChunkList.Contains(currentChunk))
                        prevActiveChunkList.Remove(currentChunk);
                }
            }

            // 차집합으로 남은 청크들 비활성화
            foreach (var chunk in prevActiveChunkList)
            {
                chunk.IsActive = false;
            }
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 해당 위치의 블록 타입 검사</summary>
        public byte GetBlockType(in Vector3 worldPos)
        {
            if(!IsBlockInWorld(worldPos))
                return Air;

            if(worldPos.y >= VoxelData.ChunkHeight - 1)
                return Grass;
            else
                return Ground;
        }

        /// <summary> 해당 위치의 블록이 단단한지 검사</summary>
        public bool IsBlockSolid(in Vector3 worldPos)
        {
            return blockTypes[GetBlockType(worldPos)].isSolid;
        }

        #endregion
    }
}