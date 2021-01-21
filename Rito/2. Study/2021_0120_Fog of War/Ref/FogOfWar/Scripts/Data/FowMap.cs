using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    public class FowMap
    {
        protected List<FowTile> map = new List<FowTile>();
        protected int mapWidth;
        protected int mapHeight;

        public Color[] colorBuffer;
        public Color[] blurBuffer;
        public Material blurMat;

        private Texture2D texBuffer;
        private RenderTexture renderBuffer;
        private RenderTexture renderBuffer2;
        private RenderTexture nextTexture;
        private RenderTexture curTexture;

        public Texture FogTexture => curTexture;

        /// <summary> 해당 위치에 있는 타일 얻어내기 </summary>
        public FowTile GetTile(TilePos pos)
        {
            if (FowHelper.InMap(pos, mapWidth, mapHeight))
            {
                return map[GetIndex(pos)];
            }
            else
            {
                return null;
            }
        }
        public int GetIndex(TilePos pos)
        {
            if (FowHelper.InMap(pos, mapWidth, mapHeight))
            {
                return pos.ConvertToTileIndex(mapWidth);
            }
            else
            {
                return -1;
            }
        }
        public int GetIndex(FowTile tile) => GetIndex(tile.pos);

        public bool InMapRange(TilePos pos)
        {
            return GetIndex(pos) != -1;
        }

        public void Lerp()
        {
            Graphics.Blit(curTexture, renderBuffer);
            blurMat.SetTexture("_LastTex", renderBuffer);
            Graphics.Blit(nextTexture, curTexture, blurMat, 1);
        }

        protected void Blur()
        {
          
            foreach (var tile in map)
            {
                var color = colorBuffer[GetIndex(tile)];
                if (color.r == 0)
                {
                    blurBuffer[GetIndex(tile)].a = color.b == 255 ? (byte)120 : (byte)255;

                }
                else
                {
                    blurBuffer[GetIndex(tile)].a = (byte)(255 - color.r);
                }
            }
            texBuffer.SetPixels(blurBuffer);
            texBuffer.Apply();

            Graphics.Blit(texBuffer, renderBuffer, blurMat,0);

            Graphics.Blit(renderBuffer, renderBuffer2, blurMat, 0);
            Graphics.Blit(renderBuffer2, renderBuffer, blurMat, 0);

            Graphics.Blit(renderBuffer, nextTexture);
          
        }

        public void ComputeFog(TilePos pos, float range)
        {
            int rangeS =(int) (range * range);

            var tiles = new List<FowTile>();
            for (int i = (int)-range; i <= range; i++)
            {
                for (int j = (int)-range; j <= range; j++)
                {
                    if (i * i + j * j <= rangeS)
                    {
                        var tile = GetTile(pos + (i, j));
                        if (tile != null)
                        {
                            colorBuffer[GetIndex(pos + (i, j))].r = 255;
                            tiles.Add(tile);
                        }
                    }
                }
            }

            tiles.Sort(
                (a, b) =>
                a.Distance(pos) - b.Distance(pos)
            );

            var obs = GetObstacle(pos, range);

            while (obs.Count > 0)
            {
                var ob = obs[0];
                var fogList = ob.RayCast(pos, tiles);
                foreach (FowTile tile in fogList)
                {
                    colorBuffer[GetIndex(tile)].r = 0;
                }
                obs.Remove(ob);
            }
            foreach (var tile in tiles)
            {
                if (colorBuffer[GetIndex(tile)].r == 255)
                {
                    colorBuffer[GetIndex(tile)].b = 255;
                }
            }
        
            Blur();
        }
       
        public List<FowTile> GetObstacle(TilePos pos, float range)
        {
            var obs = new List<FowTile>();
            var rangeS = (int)range * range;
            for (int i = (int)-range; i <= range; i++)
            {
                for (int j = (int)-range; j <= range; j++)
                {
                    if (i == 0 && i == j) continue;
                    if (i * i + j * j <= rangeS)
                    {
                        var tile = GetTile(pos + (i, j));
                        if (tile != null)
                        {
                            if (tile.type == 1)
                            {
                                obs.Add(tile);
                            }
                        }
                    }
                }
            }
            obs.Sort((a, b) =>
                a.Distance(pos) - b.Distance(pos)
            );

            return obs;
        }

        /// <summary> 지난 번 시행에 유닛이 존재해서 밝게 나타냈던 부분을 다시 안개로 가려줌 </summary>
        public void RefreshFog()
        {
            foreach (FowTile tile in map)
            {
                Color c = colorBuffer[GetIndex(tile)];

                if (c.r == 255)
                {
                    colorBuffer[GetIndex(tile)].r = 0;
                }
            }
        }

        public void InitMap(int[,] mapData)
        {
            map.Clear();
            mapWidth  = mapData.GetLength(0);
            mapHeight = mapData.GetLength(1);

            colorBuffer = new Color[mapWidth * mapHeight];
            blurBuffer  = new Color[mapWidth * mapHeight];

            //colorInfo = new Color32[mapWidth * mapHeight];
            blurMat = new Material(Shader.Find("ImageEffect/AverageBlur"));
            texBuffer = new Texture2D(mapWidth, mapHeight, TextureFormat.ARGB32, false);
            texBuffer.wrapMode = TextureWrapMode.Clamp;

            renderBuffer  = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
            renderBuffer2 = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);

            nextTexture = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
            curTexture  = RenderTexture.GetTemporary((int)(mapWidth * 1.5f), (int)(mapHeight * 1.5f), 0);
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    map.Add(new FowTile(mapData[i, j], i, j));
                    colorBuffer[i] = new Color(0, 0, 0, 1f);
                }

            }
        }

        public void Release()
        {
            RenderTexture.ReleaseTemporary(renderBuffer);
            RenderTexture.ReleaseTemporary(renderBuffer2);
            RenderTexture.ReleaseTemporary(nextTexture);
            RenderTexture.ReleaseTemporary(curTexture);
        }
    }

    

    public static class FogHelper
    {
        public static bool InMap(int x, int y, int mapWidth, int mapHeight)
        {
            return (x >= 0 && y >= 0 && x < mapWidth && y < mapHeight);
        }

        public static bool CantDisplay(int x1, int y1, int x2, int y2, float z)
        {
            if ((x1 == 0 && y1 == 0) || (x2 == 0 && y2 == 0)) return true;

            //if (x1 == 0 && y1 == 0) return true;
            if (x1 == 0 || x2 == 0)
            {
                var t = y1;
                y1 = x1;
                x1 = t;
                t = y2;
                y2 = x2;
                x2 = t;
            }
            var k1 = y1 * 1f / x1;
            var k2 = y2 * 1f / x2;
            var dot = x1 * x2 + y1 * y2;
            if (dot > 0)
            {
                return Angle(k1, k2) < z;
            }
            else
            {
                return false;
            }

        }

        public static float Angle(float k1, float k2)
        {
            return Mathf.Abs((k2 - k1) / (1 + k1 * k2));
        }
    }

}