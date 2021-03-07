using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-07 PM 7:33:52
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        /***********************************************************************
        *                               Public Properties
        ***********************************************************************/
        #region .
        public int Capacity => _capacity;
        public int Count => _itemList.Count;

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        [SerializeField]
        private List<Item> _itemList = new List<Item>();

        private InventoryUI _connectedUI;
        private int _capacity = 24; // 아이템 수용 한도

        #endregion

        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 인벤토리 UI 연결 </summary>
        public void ConnectUI(InventoryUI inventoryUI)
        {
            _connectedUI = inventoryUI;
        }

        /// <summary> 인벤토리에 아이템 추가
        /// <para/> - 수용량 한계인 경우, 추가하지 않고 false 리턴
        /// </summary>
        public bool Add(Item item)
        {
            if(Count >= Capacity) return false;

            _itemList.Add(item);
            return true;
        }

        /// <summary> 인벤토리에서 아이템 제거
        /// <para/> - 잘못된 인덱스를 참조한 경우 false 리턴
        /// </summary>
        public bool Remove(int index)
        {
            if(!IsValidateIndex(index)) return false;
            if(_itemList[index] == null) return false;

            _itemList.RemoveAt(index);
            return true;
        }

        /// <summary> 두 인덱스의 아이템 위치를 서로 교체 </summary>
        public void Swap(int indexA, int indexB)
        {
            if(!IsValidateIndex(indexA)) return;
            if(!IsValidateIndex(indexB)) return;

            // Swap
            Item temp = _itemList[indexA];
            _itemList[indexA] = _itemList[indexB];
            _itemList[indexB] = temp;
        }

        #endregion
        /***********************************************************************
        *                               Check Methods
        ***********************************************************************/
        #region .
        private bool IsValidateIndex(int index)
        {
            return index > 0 && index < Count;
        }

        #endregion
    }
}