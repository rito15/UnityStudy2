using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-21 PM 11:01:27
// 작성자 : Rito

namespace Rito.InventorySystem
{
    /// <summary> 수량을 셀 수 있는 아이템 </summary>
    public class CountableItem : Item
    {
        /// <summary> 현재 아이템 개수 </summary>
        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value > 1 ? value : 1;
            }
        }

        /// <summary> 하나의 슬롯이 가질 수 있는 최대 개수(기본 99) </summary>
        public int MaxAmount => _maxAmount;

        private int _amount;
        private int _maxAmount;

        public CountableItem(ItemData data, int amount = 1, int maxAmount = 99) : base(data)
        {
            _amount = amount;
            _maxAmount = maxAmount;
        }
    }
}