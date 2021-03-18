using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-03-07 PM 10:20:05
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class ItemSlotUI : MonoBehaviour
    {
        private RectTransform _itemRect;
        private Image _itemImage;

        private void Awake()
        {
            _itemRect = transform.GetChild(0).GetComponent<RectTransform>();
            _itemImage = _itemRect.GetComponent<Image>();
        }
    }
}