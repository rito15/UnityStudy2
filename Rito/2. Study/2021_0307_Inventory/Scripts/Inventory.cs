using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    [TODO]

    - ADD 구현

    - 현재 존재하는 타입들의 아이템 데이터 모두 구현

    - 아이템 습득, 버리기, 사용, 정렬 등등 구현


*/


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
        /// <summary> 아이템 수용 한도 </summary>
        public int Capacity { get; private set; }

        // /// <summary> 현재 아이템 개수 </summary>
        //public int ItemCount => _itemArray.Count;

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        
        // 초기 수용 한도
        [SerializeField]
        private int _initalCapacity = 32;

        // 최대 수용 한도(리스트 크기)
        [SerializeField]
        private int _maxCapacity = 64;

        [SerializeField]
        private InventoryUI _connectedUI; // 연결된 인벤토리 UI

        /// <summary> 아이템 목록 배열 </summary>
        [SerializeField]
        private Item[] _itemArray;

        /// <summary> 아이템 - 인덱스 테이블 </summary>
        private Dictionary<ItemData, int> _itemIndexDict;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            _itemArray = new Item[_maxCapacity];
            _itemIndexDict = new Dictionary<ItemData, int>();
            Capacity = _initalCapacity;
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 인덱스가 수용 범위 내에 있는지 검사 </summary>
        private bool IsValidIndex(int index)
        {
            return index > 0 && index < Capacity;
        }

        /// <summary> 앞에서부터 비어있는 슬롯 인덱스 확인 </summary>
        private int GetEmptySlotIndex()
        {
            for(int i = 0; i < Capacity; i++)
                if(_itemArray[i] == null)
                    return i;
            return -1;
        }

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
        /// <para/> - 넣을 수 없는 경우, 추가하지 않고 false 리턴
        /// </summary>
        public bool Add(ItemData itemData)
        {
            int currentIndex = GetEmptySlotIndex(); // 비어있는 슬롯 인덱스
            bool isCountable = itemData.Type != ItemType.Equipment;


            // 수량이 없는 아이템
            if (!isCountable)
            {
                // 인벤토리가 가득 찬 경우
                if (currentIndex == -1)
                {
                    return false;
                }
                // 넣을 수 있는 경우
                else
                {
                    Item item = new Item(itemData);
                    _itemArray[currentIndex] = item;
                }
            }
            // 수량이 있는 아이템
            else
            {
                // 넣을 수 있는 경우, 없는 경우 판단하여 처리

                // 수용량 초과여도 MaxAmount 도달하지 않았으면 습득 가능
                // 대신 추가적인 처리 필요 (현재 80개인데 추가로 50개를 넣어서 maxAmount 31개 초과 등)
            }

            return true;
        }

        /// <summary> 인벤토리에서 아이템 제거
        /// <para/> - 잘못된 인덱스를 참조한 경우 false 리턴
        /// </summary>
        public bool Remove(int index)
        {
            if(!IsValidIndex(index)) return false;
            if(_itemArray[index] == null) return false;

            _itemArray[index] = null;
            return true;
        }

        /// <summary> 두 인덱스의 아이템 위치를 서로 교체 </summary>
        public void Swap(int indexA, int indexB)
        {
            if(!IsValidIndex(indexA)) return;
            if(!IsValidIndex(indexB)) return;

            // Swap
            Item temp = _itemArray[indexA];
            _itemArray[indexA] = _itemArray[indexB];
            _itemArray[indexB] = temp;
        }

        #endregion
    }
}