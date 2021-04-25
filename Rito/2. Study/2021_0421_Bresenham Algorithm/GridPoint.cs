using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-04-21 PM 9:58:22
// 작성자 : Rito

namespace Rito.BresenhamAlgorithm
{
    public class GridPoint : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private float TileSize => BresenhamTester.TileSize;
        private Vector2 HalfTile;

        private RectTransform _rt;
        private Vector2 _beginPoint;
        private Vector2 _beginAnPoint;

        private void Awake()
        {
            TryGetComponent(out _rt);
            HalfTile = Vector2.one * (TileSize * 0.5f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _beginPoint = eventData.position;
            _beginAnPoint = _rt.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 offset = eventData.position - _beginPoint - HalfTile;
            _rt.anchoredPosition = GetGridPoint(_beginAnPoint + offset);
        }

        private Vector2 GetGridPoint(Vector2 point)
        {
            point /= TileSize;
            point.x = Mathf.RoundToInt(point.x);
            point.y = Mathf.RoundToInt(point.y);

            return point * TileSize + HalfTile;
        }
    }
}