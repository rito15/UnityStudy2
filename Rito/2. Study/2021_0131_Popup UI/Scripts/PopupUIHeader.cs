using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-02-01 AM 2:52:15
// 작성자 : Rito

namespace Rito
{
    /// <summary> 헤더 드래그 앤 드롭에 의한 UI 이동 구현 </summary>
    public class PopupUIHeader : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        private RectTransform _parentRect;

        private Vector2 _rectBegin;
        private Vector2 _moveBegin;
        private Vector2 _moveOffset;

        private void Awake()
        {
            _parentRect = transform.parent.GetComponent<RectTransform>();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"Focus {name}");
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _rectBegin = _parentRect.anchoredPosition;
            _moveBegin = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            _moveOffset = eventData.position - _moveBegin;
            _parentRect.anchoredPosition = _rectBegin + _moveOffset;
        }
    }
}