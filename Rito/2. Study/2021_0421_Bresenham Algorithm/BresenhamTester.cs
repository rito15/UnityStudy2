using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-21 PM 9:38:12
// 작성자 : Rito

namespace Rito.BresenhamAlgorithm
{
    // 브레젠햄(픽셀 선 그리기) 알고리즘
    public class BresenhamTester : MonoBehaviour
    {
        public bool _showLine = true;
        public bool _showPoints = true;
        [Range(0.01f, 1f)]
        public float _updateCycle = 0.1f;

        [Space(12)]
        public GameObject _rectPrefab;
        public GameObject _pointPrefab;
        public Transform _pointsParent;
        public Image _lineImage;

        [Space(12)]
        public int _gridCount = 10;
        public RectTransform _pointA;
        public RectTransform _pointB;

        [Space(12)]
        public Point _gridPosA;
        public Point _gridPosB;

        private Point _prevPosA;
        private Point _prevPosB;

        public const float TileSize = 50f;

        private Image[,] _grid;

        private void Awake()
        {
            InitGrid();
            _prevPosA = new Point(-1, -1);
            _prevPosB = new Point(-1, -1);
        }

        private void OnEnable()
        {
            StartCoroutine(nameof(UpdateRoutine));
        }

        private void InitGrid()
        {
            _grid = new Image[_gridCount, _gridCount];
            Vector2 pos = new Vector2(0f, 0f);

            for (int y = 0; y < _gridCount; y++)
            {
                for (int x = 0; x < _gridCount; x++)
                {
                    GameObject go = Instantiate(_rectPrefab, transform);
                    go.SetActive(true);
                    go.name = $"[{x}, {y}]";

                    go.TryGetComponent(out RectTransform rt);
                    rt.anchoredPosition = pos;

                    _grid[x, y] = go.GetComponent<Image>();

                    // Next X
                    pos.x += TileSize;
                }

                // Next Y
                pos.y += TileSize;
                pos.x = 0f;
            }
        }

        private Point GetGridPosFrom3d(in Vector2 point)
        {
            Vector2 LB = _grid[0, 0].rectTransform.position;
            Vector2 posOffset = point - LB;

            int x = (int)(posOffset.x / TileSize);
            int y = (int)(posOffset.y / TileSize);

            return new Point(x, y);
        }

        /// <summary> Anchored Position으로부터 그리드 좌표 얻기 </summary>
        private Point GetGridPosFrom2d(in Vector2 point)
        {
            int x = (int)(point.x / TileSize);
            int y = (int)(point.y / TileSize);

            return new Point(x, y);
        }

        /// <summary> 각 사각형의 중간 좌표값 구하기 </summary>
        private float GetRectMidValue(float value)
        {
            float res = value - (value % TileSize) + TileSize * 0.5f;
            return res;
        }

        private void PaintSlotColor(in Point gridPos, in Color color)
        {
            int x = gridPos.x;
            int y = gridPos.y;

            if (x < 0 || y < 0) return;
            if(x >= _grid.GetLength(0)) return;
            if(y >= _grid.GetLength(1)) return;

            _grid[x, y].color = color;
        }

        private IEnumerator UpdateRoutine()
        {
            while (true)
            {
                Point pA = GetGridPosFrom2d(_pointA.anchoredPosition);
                Point pB = GetGridPosFrom2d(_pointB.anchoredPosition);

                Bresenham bh = new Bresenham(pA, pB);
                foreach (Point p in bh)
                {
                    PaintSlotColor(p, Color.green);
                }
                
                yield return new WaitForSeconds(_updateCycle);

                foreach (Point p in bh)
                {
                    PaintSlotColor(p, Color.white);
                }

                _lineImage.enabled = _showLine;
            }
        }

        private IEnumerator UpdateRoutine_OLD()
        {
            List<RectTransform> pointList = new List<RectTransform>();
            int infLoopCount;

            while (true)
            {
                infLoopCount = 0;

                Vector2 posA = _pointA.anchoredPosition;
                Vector2 posB = _pointB.anchoredPosition;

                float lineWidth = posB.x - posA.x;
                float lineHeight = posB.y - posA.y;
                float slope = lineHeight / lineWidth; // 기울기

                // A -> B 방향벡터가 좌하향/좌상향일 경우, 두 점을 교환
                float sum = lineWidth + lineHeight;
                if (sum < 0 || (sum == 0 && lineWidth > 0))
                {
                    var temp = posA;
                    posA = posB;
                    posB = temp;
                }

                if (Mathf.Abs(slope) < 1f)
                {
                    float x = GetRectMidValue(posA.x);
                    float y = posA.y + (x - posA.x) * slope;
                    float endX = GetRectMidValue(posB.x);

                    while (Mathf.Abs(x) < Mathf.Abs(endX))
                    {
                        RectTransform rt = L_DrawPoint(x, y);

                        // Paint Green
                        PaintSlotColor(GetGridPosFrom2d(rt.anchoredPosition), Color.green);

                        y += TileSize * slope;
                        x = GetRectMidValue(x + TileSize);

                        CheckInfiniteLoop();
                    }
                }
                else
                {
                    float y = GetRectMidValue(posA.y);
                    float x = posA.x + (y - posA.y) / slope;
                    float endY = GetRectMidValue(posB.y);

                    while (Mathf.Abs(y) < Mathf.Abs(endY))
                    {
                        RectTransform rt = L_DrawPoint(x, y);

                        // Paint Green
                        PaintSlotColor(GetGridPosFrom2d(rt.anchoredPosition), Color.green);

                        x += TileSize / slope;
                        y = GetRectMidValue(y + TileSize);

                        CheckInfiniteLoop();
                    }
                }
                
                yield return new WaitForSeconds(_updateCycle);

                pointList.ForEach(rt =>
                {
                    PaintSlotColor(GetGridPosFrom2d(rt.anchoredPosition), Color.white);
                    Destroy(rt.gameObject);
                });
                pointList.Clear();

                //UpdateEndPoints();

                _lineImage.enabled = _showLine;
            }

            RectTransform L_DrawPoint(float x, float y)
            {
                GameObject pointGo = Instantiate(_pointPrefab, _pointsParent);
                pointGo.SetActive(_showPoints);
                pointGo.name = $"{x}, {y}";
                pointGo.TryGetComponent(out RectTransform rt);
                rt.anchoredPosition = new Vector2(x, y);
                pointList.Add(rt);

                return rt;
            }

            void CheckInfiniteLoop()
            {
                if(++infLoopCount > 10000)
                    throw new System.Exception("Infinite Loop");
            }
        }

        private void UpdateEndPoints()
        {
            _gridPosA = GetGridPosFrom2d(_pointA.anchoredPosition);
            _gridPosB = GetGridPosFrom2d(_pointB.anchoredPosition);

            if (_gridPosA != _prevPosA)
            {
                PaintSlotColor(_gridPosA, Color.green);
                PaintSlotColor(_prevPosA, Color.white);
            }
            if (_gridPosB != _prevPosB)
            {
                PaintSlotColor(_gridPosB, Color.green);
                PaintSlotColor(_prevPosB, Color.white);
            }

            _prevPosA = _gridPosA;
            _prevPosB = _gridPosB;
        }

        /***********************************************************************
        *                               Definitions
        ***********************************************************************/
        #region .

        public struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public static implicit operator Point((int x, int y) p) => new Point(p.x, p.y);
            public static bool operator ==(Point a, Point b) => a.x == b.x && a.y == b.y;
            public static bool operator !=(Point a, Point b) => !(a.x == b.x && a.y == b.y);
            public override string ToString() => $"({x}, {y})";
        }

        internal class Bresenham : IEnumerable
        {
            private readonly List<Point> points;

            public int Count { get; private set; }

            public Point this[int index]
            {
                get => points[index];
            }

            public Bresenham(Point p1, Point p2)
            {
                int w = Math.Abs(p2.x - p1.x);
                int h = Math.Abs(p2.y - p1.y);
                points = new List<Point>(w + h);

                SetPoints(p1, p2);
                Count = points.Count;
            }

            private void SetPoints(in Point p1, in Point p2)
            {
                int W = p2.x - p1.x; // width
                int H = p2.y - p1.y; // height;
                int absW = Math.Abs(W);
                int absH = Math.Abs(H);

                int xSign = Math.Sign(W);
                int ySign = Math.Sign(H);

                // 기울기 절댓값
                float absM = (W == 0) ? float.MaxValue : (float)absH / absW;

                int k;  // 판별값
                int kA; // p가 0 이상일 때 p에 더할 값
                int kB; // p가 0 미만일 때 p에 더할 값

                int x = p1.x;
                int y = p1.y;

                // 1. 기울기 절댓값이 1 미만인 경우 => x 기준
                if (absM < 1f)
                {
                    k = 2 * absH - absW; // p의 초깃값
                    kA = 2 * absH;
                    kB = 2 * (absH - absW);

                    for (; W >= 0 ? x <= p2.x : x >= p2.x; x += xSign)
                    {
                        points.Add((x, y));

                        if (k < 0)
                        {
                            k += kA;
                        }
                        else
                        {
                            k += kB;
                            y += ySign;
                        }
                    }
                }
                // 기울기 절댓값이 1 이상인 경우 => y 기준
                else
                {
                    k = 2 * absW - absH; // p의 초깃값
                    kA = 2 * absW;
                    kB = 2 * (absW - absH);

                    for (; H >= 0 ? y <= p2.y : y >= p2.y; y += ySign)
                    {
                        points.Add((x, y));

                        if (k < 0)
                        {
                            k += kA;
                        }
                        else
                        {
                            k += kB;
                            x += xSign;
                        }
                    }
                }
            }

            public IEnumerator GetEnumerator()
            {
                return points.GetEnumerator();
            }
        }

        #endregion
    }
}