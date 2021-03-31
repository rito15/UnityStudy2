using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-31 PM 7:53:18
// 작성자 : Rito

namespace Rito.VoxelSystem
{
    /// <summary> 지형 분포 데이터 </summary>
    [CreateAssetMenu(fileName = "BiomeData", menuName = "Voxel System/Biome Attribute")]
    public class BiomeData : ScriptableObject
    {
        public string biomeName;

        // 이 값 이하의 높이는 모두 solid
        public int solidGroundHeight;

        // solidGroundHeight로부터 증가할 수 있는 최대 높이값
        public int terrainHeightRange;

        public float terrainScale;

        /*
            예시

            solidGroundHeight  = 40;
            terrainHeightRange = 30;

            => 지형의 최소 높이 : 40, 지형의 최대 높이(고지) : 70
        */

        public Lode[] lodes;
    }

    /// <summary> 광맥 </summary>
    [System.Serializable]
    public class Lode
    {
        public string loadName;
        public byte blockID;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public float noiseOffset;
    }
}