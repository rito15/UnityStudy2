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
        public GameObject _rectPrefab;
        public int _gridCount = 10;
        public RectTransform _pointA;
        public RectTransform _pointB;

        public Vector2Int _gridPosA;
        public Vector2Int _gridPosB;

        private Vector2Int _prevPosA;
        private Vector2Int _prevPosB;

        private const float RectSize = 50f;

        private Image[,] _grid;

        private void Start()
        {
            InitGrid();
            _prevPosA = new Vector2Int(-1, -1);
            _prevPosB = new Vector2Int(-1, -1);
        }

        private void Update()
        {
            _gridPosA = GetGridPos(_pointA.position);
            _gridPosB = GetGridPos(_pointB.position);

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
                    pos.x += RectSize;
                }

                // Next Y
                pos.y += RectSize;
                pos.x = 0f;
            }
        }

        private Vector2Int GetGridPos(in Vector2 point)
        {
            Vector2 LB = _grid[0, 0].rectTransform.position;
            Vector2 posOffset = point - LB;

            int x = (int)(posOffset.x / RectSize);
            int y = (int)(posOffset.y / RectSize);

            return new Vector2Int(x, y);
        }

        private void PaintSlotColor(in Vector2Int gridPos, in Color color)
        {
            int x = gridPos.x;
            int y = gridPos.y;

            if(x < 0 || y < 0) return;
            if(x >= _grid.GetLength(0)) return;
            if(y >= _grid.GetLength(1)) return;

            _grid[x, y].color = color;
        }
    }
}