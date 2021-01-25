using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

// 날짜 : 2021-01-24 PM 5:23:49
// 작성자 : Rito

namespace Rito.FogOfWarJob
{
    //[BurstCompile]
    public struct FowJob : IJobParallelFor
    {
        public TilePos origin;
        [ReadOnly] public NativeArray<float> heightArray;
        [ReadOnly] public NativeArray<TilePos> destArray;
        [WriteOnly] public NativeArray<bool> resultArray;

        public float sightHeight; // origin의 시야 높이
        public int mapWidth;
        public int mapLength;

        public void Execute(int i)
        {
            TilePos dest = destArray[i];

            // 1. 동일 위치 - 시야 가능
            if (origin.x == dest.x && origin.y == dest.y)
            {
                resultArray[i] = false;
                return;
            }

            // 2. 타일의 높이가 애초에 높은 경우 - 시야 불가능
            if (dest.height > origin.height + sightHeight)
            {
                resultArray[i] = true;
                return;
            }

            int destX = dest.x;
            int destY = dest.y;
            int xLen = dest.x - origin.x;
            int yLen = dest.y - origin.y;

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
                        if (IsBlocked(x, y))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    for (; x >= destX; x--)
                    {
                        if (IsBlocked(x, y))
                        {
                            resultArray[i] = true;
                            return;
                        }
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
                        if (IsBlocked(x, y))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    for (; y >= destY; y--)
                    {
                        if (IsBlocked(x, y))
                        {
                            resultArray[i] = true;
                            return;
                        }
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

                        if (IsBlocked(x + xMove, y + yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (IsBlocked(x + xMove, y + yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
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

                        if (IsBlocked(x - xMove, y + yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (IsBlocked(x - xMove, y + yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
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

                        if (IsBlocked(x - xMove, y - yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (IsBlocked(x - xMove, y - yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
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

                        if (IsBlocked(x + xMove, y - yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
                else
                {
                    while (xMove < xLen && yMove < yLen)
                    {
                        if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
                        else xMove++;

                        if (IsBlocked(x + xMove, y - yMove))
                        {
                            resultArray[i] = true;
                            return;
                        }
                    }
                }
            }

            resultArray[i] = false;
        }

        private bool IsBlocked(int a, int b)
        {
            int index = a + b * mapWidth;
            if (index >= mapLength || index < 0)
                return true;
            return heightArray[index] > origin.height + sightHeight;
        }
    }
}