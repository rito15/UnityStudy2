#define DEBUG_RANGE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.FogOfWar
{
    [System.Serializable]
    public class FowMap
    {
        private List<FowTile> map = new List<FowTile>();
        private int mapWidth;
        private int mapHeight;

        // 배열들은 타일 개수만큼 크기 생성
        private Visit[] visit;

        //public Color32[] colorBuffer;
        private Color32[] fogBuffer;
        private Material blurMat;

        private Texture2D texBuffer;
        private RenderTexture renderBuffer; // 렌더텍스쳐 여러장 쓰는 이유 : 안개를 더 부드럽게 보이게 하기 위해
        private RenderTexture renderBuffer2;
        private RenderTexture nextTexture;
        private RenderTexture curTexture;

        public Texture FogTexture => curTexture;

        /***********************************************************************
        *                               Init, Release
        ***********************************************************************/
        #region .
        public void InitMap(float[,] heightData)
        {
            map.Clear();
            mapWidth = heightData.GetLength(0);
            mapHeight = heightData.GetLength(1);

            visit = new Visit[mapWidth * mapHeight];
            fogBuffer = new Color32[mapWidth * mapHeight];

            //colorInfo = new Color32[mapWidth * mapHeight];
            blurMat = new Material(Shader.Find("FogOfWar/AverageBlur"));
            texBuffer = new Texture2D(mapWidth, mapHeight, TextureFormat.ARGB32, false);
            texBuffer.wrapMode = TextureWrapMode.Clamp;

            int width  = (int)(mapWidth * 1.5f);
            int height = (int)(mapHeight * 1.5f);

            renderBuffer  = RenderTexture.GetTemporary(width,height, 0);
            renderBuffer2 = RenderTexture.GetTemporary(width, height, 0);

            nextTexture = RenderTexture.GetTemporary(width, height, 0);
            curTexture  = RenderTexture.GetTemporary(width, height, 0);

            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    // 타일정보 : 장애물여부, X좌표, Y좌표, 너비
                    map.Add(new FowTile(heightData[i, j], i, j, mapWidth));
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

        #endregion
        /***********************************************************************
        *                               Getter, Calculators
        ***********************************************************************/
        #region .
        /// <summary> 해당 위치에 있는 타일 얻어내기 </summary>
        private FowTile GetTile(in int x, in int y)
        {
            if (InMapRange(x, y))
            {
                return map[GetTileIndex(x, y)];
            }
            else
            {
                return null;
            }
        }

        /// <summary> 해당 좌표가 맵 내에 있는지 검사 </summary>
        public bool InMapRange(in int x, in int y)
        {
            return x >= 0 && y >= 0 &&
                   x < mapWidth && y < mapHeight;
        }

        /// <summary> (x, y) 타일좌표를 배열인덱스로 변환 </summary>
        public int GetTileIndex(in int x, in int y)
        {
            return x + y * mapWidth;
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 이전 프레임의 안개를 현재 프레임에 부드럽게 보간 </summary>
        public void LerpBlur()
        {
            Graphics.Blit(curTexture, renderBuffer);
            blurMat.SetTexture("_LastTex", renderBuffer);
            Graphics.Blit(nextTexture, curTexture, blurMat, 1);
        }

        /// <summary> 지난 번 시행에 유닛이 존재해서 밝게 나타냈던 부분을 다시 안개로 가려줌 </summary>
        public void RefreshFog()
        {
            foreach (FowTile tile in map)
            {
                int index = tile.index;
                visit[index].current = false;
            }
        }

        /// <summary> 타일 위치로부터 시야만큼 안개 계산 </summary>
        public void ComputeFog(TilePos pos, in float sightXZ, in float sightY
#if DEBUG_RANGE
            , out List<FowTile> visibles
#endif
            )
        {
            int sightRangeInt = (int)sightXZ;
            int rangeSquare = sightRangeInt * sightRangeInt;

            // 현재 시야(원형 범위)만큼의 타일들 목록
            // x^2 + y^2 <= range^2
            var tilesInSight = new List<FowTile>();
            for (int i = -sightRangeInt; i <= sightRangeInt; i++)
            {
                for (int j = -sightRangeInt; j <= sightRangeInt; j++)
                {
                    if (i * i + j * j <= rangeSquare)
                    {
                        var tile = GetTile(pos.x + i, pos.y + j);
                        if (tile != null)
                        {
                            tilesInSight.Add(tile);
                        }
                    }
                }
            }

            // 시야를 밝힐 수 있는 타일들 목록 가져오기
            List<FowTile> visibleTileList =
                GetVisibleTilesInRange(tilesInSight, pos, sightY);
#if DEBUG_RANGE
            visibles = visibleTileList;
#endif
            // 현재 방문, 과거 방문 여부 true
            foreach (FowTile visibleTile in visibleTileList)
            {
                visit[visibleTile.index].current = true;
                visit[visibleTile.index].ever = true;
            }

            ApplyFogAlpha();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 시야 내 타일들 중 중심으로부터 밝힐 수 있는 타일들 가져오기 </summary>
        private List<FowTile> GetVisibleTilesInRange(List<FowTile> tilesInSight, in TilePos centerPos, in float sightHeight)
        {
            List<FowTile> visibleTiles = new List<FowTile>();

            foreach (FowTile tile in tilesInSight)
            {
                // 1. 타일의 높이가 유닛 가시 높이보다 높은 경우 불가능
                if (tile.Height > centerPos.height + sightHeight)
                    continue;

                // 2. 유닛과 해당 타일 사이에 유닛보다 더 높은 높이의 타일이 있는 경우 불가능
                if (TileCast(centerPos, tile, sightHeight))
                    continue;

                visibleTiles.Add(tile);
            }

            return visibleTiles;
        }

        /// <summary> 특정 위치의 타일로부터 대상 타일까지 가려지지 않았는지 검사 </summary>
        private bool TileCast(in TilePos origin, FowTile dest, in float sightHeight)
        {
            // 동일 위치
            if (origin.x == dest.X && origin.y == dest.Y) return false;

            float exceededHeight = origin.height + sightHeight;

            int destX = dest.X;
            int destY = dest.Y;
            int xLen = destX - origin.x;
            int yLen = destY - origin.y;

            int xSign = System.Math.Sign(xLen);
            int ySign = System.Math.Sign(yLen);

            xLen = System.Math.Abs(xLen);
            yLen = System.Math.Abs(yLen);

            int x = origin.x;
            int y = origin.y;

            // 가로 전진
            if (yLen == 0)
            {
                if (xSign > 0)
                {
                    for (; x <= destX; x++)
                    {
                        if (isBlocked(x, y))
                            return true;
                    }
                }
                else
                {
                    for (; x >= destX; x--)
                    {
                        if (isBlocked(x, y))
                            return true;
                    }
                }
            }
            // 세로 전진
            if (xLen == 0)
            {
                if (ySign > 0)
                {
                    for (; y <= destY; y++)
                    {
                        if (isBlocked(x, y))
                            return true;
                    }
                }
                else
                {
                    for (; y >= destY; y--)
                    {
                        if (isBlocked(x, y))
                            return true;
                    }
                }
            }

            float xyRatio = (float)xLen / yLen;
            float yxRatio = (float)yLen / xLen;
            int xMove = 0;
            int yMove = 0;

            // 우상향
            if (xSign > 0 && ySign > 0)
            {

                if (xyRatio > yxRatio)
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
                        else yMove++;

                        if (isBlocked(x + xMove, y + yMove))
                            return true;
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (isBlocked(x + xMove, y + yMove))
                            return true;
                    }
                }
            }
            // 좌상향
            if (xSign < 0 && ySign > 0)
            {
                if (xyRatio > yxRatio)
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
                        else yMove++;

                        if (isBlocked(x - xMove, y + yMove))
                            return true;
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (isBlocked(x - xMove, y + yMove))
                            return true;
                    }
                }
            }
            // 좌하향
            if (xSign < 0 && ySign < 0)
            {
                if (xyRatio > yxRatio)
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
                        else yMove++;

                        if (isBlocked(x - xMove, y - yMove))
                            return true;
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (isBlocked(x - xMove, y - yMove))
                            return true;
                    }
                }
            }
            // 우하향
            if (xSign > 0 && ySign < 0)
            {
                if (xyRatio > yxRatio)
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
                        else yMove++;

                        if (isBlocked(x + xMove, y - yMove))
                            return true;
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (isBlocked(x + xMove, y - yMove))
                            return true;
                    }
                }
            }

            return false;

            bool isBlocked(int a, int b)
            {
                int index = GetTileIndex(a, b);
                if (index > map.Count || index < 0) return true;
                return map[index].Height > exceededHeight;
            }
        }

        /// <summary> 방문 정보를 텍스처의 알파 정보로 변환하고 가우시안 블러 적용 </summary>
        private void ApplyFogAlpha()
        {
            foreach (var tile in map)
            {
                int index = tile.index;
                byte alpha;

                if (visit[index].current) alpha = 0;     // 현재 위치한 경우 알파 제로
                else if (visit[index].ever) alpha = 200; // 과거 방문 이력
                else alpha = 255;                        // 한 번도 방문한 적 없음

                fogBuffer[index].a = alpha;
            }

            texBuffer.SetPixels32(fogBuffer);
            texBuffer.Apply();

            Graphics.Blit(texBuffer, renderBuffer, blurMat, 0);

            Graphics.Blit(renderBuffer, renderBuffer2, blurMat, 0);
            Graphics.Blit(renderBuffer2, renderBuffer, blurMat, 0);

            Graphics.Blit(renderBuffer, nextTexture);
        }

        #endregion
    }
}