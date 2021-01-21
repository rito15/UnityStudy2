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
        public int type;
        public TilePos pos;
        public int X => pos.x;
        public int Y => pos.y;

        public FowTile(int type, int x, int y)
        {
            this.type = type;
            pos.x = x;
            pos.y = y;
        }

        public bool CantDisplay(FowTile ob, TilePos pos)
        {
            return FogHelper.CantDisplay
            (
                ob.X - pos.x, ob.Y - pos.x, 
                X - pos.x, Y - pos.y,
                Mathf.PI / (6 + ob.Distance(pos) / 1.2f)
            );
        }

        public List<FowTile> RayCast(TilePos pos, List<FowTile> tiles)
        {
            var fogTile = new List<FowTile>();
            var startIndex = tiles.IndexOf(this);

            for (int i = startIndex + 1; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile.CantDisplay(this, pos))
                {
                    fogTile.Add(tile);
                }
            }
            return fogTile;
        }

        public int Distance(TilePos other)
        {
            int distX = other.x - X;
            int distY = other.y - Y;
            return (distX * distX) + (distY * distY);
        }
    }
}