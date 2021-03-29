using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-29 PM 3:59:51
// 작성자 : Rito

namespace Rito.VoxelSystem
{
    public readonly struct ChunkCoord
    {
        readonly public int x;
        readonly public int z;

        public ChunkCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public bool Equals(in ChunkCoord other)
        {
            return this.x == other.x && this.z == other.z;
        }
        public static bool operator ==(in ChunkCoord @this, in ChunkCoord other)
        {
            return @this.x == other.x && @this.z == other.z;
        }
        public static bool operator !=(in ChunkCoord @this, in ChunkCoord other)
        {
            return @this.x != other.x || @this.z != other.z;
        }
    }
}