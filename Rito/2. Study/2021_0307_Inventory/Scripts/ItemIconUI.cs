using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-03-18 PM 7:46:09
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class ItemIconUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        private Vector2 _startingPoint;
        private Vector2 _moveBegin;
        private int _sbIndex;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _startingPoint = transform.position;
            _moveBegin = eventData.position;

            _sbIndex = transform.parent.GetSiblingIndex();
            transform.parent.SetAsLastSibling(); // 맨 위로 올려주기
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Vector2 _moveOffset = eventData.position - _moveBegin;
            transform.position = _startingPoint + _moveOffset;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            transform.position = _startingPoint;

            transform.parent.SetSiblingIndex(_sbIndex);
        }
    }
}