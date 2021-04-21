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
        private RectTransform _rt;
        private Vector2 _beginPoint;
        private Vector2 _beginAnPoint;

        private void Start()
        {
            TryGetComponent(out _rt);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _beginPoint = eventData.position;
            _beginAnPoint = _rt.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 offset = eventData.position - _beginPoint;
            _rt.anchoredPosition = _beginAnPoint + offset;
        }
    }
}