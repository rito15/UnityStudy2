using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    - Item의 상속구조
        - Item : abstract Use()
            - CountableItem
                - PortionItem : Use() -> 사용
            - EquipmentItem : Use() -> 착용
                - WeaponItem
                - ArmorItem

    - ItemData의 상속구조
      (ItemData는 해당 아이템이 공통으로 가질 데이터 필드 모음)
      (개체마다 달라져야 하는 현재 내구도, 강화도 등은 Item 클래스에서)

        - ItemData
            - CountableItemData
                - PortionItemData : 효과량(Value : 회복량, 공격력 등에 사용)
            - EquipmentItemData : 최대 내구도
                - WeaponItemData : 기본 공격력
                - ArmorItemData : 기본 방어력


*/


/*
    [기능]
    - [o] 사용 가능/불가능 슬롯 관리
    - [o] 셀 수 있는 아이템은 텍스트로 개수 표시
    - [o] 아이템 이미지 드래그 앤 드롭
    - [o] 아이템 습득
        - 아이템 컨테이너 프리팹으로부터 습득
    - [o] 아이템 버리기
        - 버리려고 할 때 팝업으로 물어보기
        - 버리는데 성공하면 아이템 컨테이너 프리팹 생성
    - [o] 아이템끼리 슬롯 교체
    - [o] 마우스 올린 슬롯에 하이라이트 표시
    - 마우스 올린 슬롯에 아이템이 있으면 툴팁 UI 표시
    - 동일한 셀 수 있는 아이템끼리 드래그 앤 드롭할 경우 개수 합치기
    - 셀 수 있는 아이템 개수 나누기(Shift 클릭, 나눌 개수 팝업으로 지정)
    - 아이템 타입에 따라 필터링하기(전체(기본값), 장비, 소비)
    - 빈칸 없이 정렬하기(타입에 따라)
*/

/*

    [TODO]

    - ADD 구현 [완료]

    - Unaccessible Slot 구현 [완료]

    - Drag & Drop 구현 [완료]

    - 아이템 습득 구현 [완료]

    - 아이템 버리기 구현 [완료]

    - 드래그 앤 드롭 시 소비 아이템은 개수 합치도록 구현(Max 넘치면 Max까지만)

    - 아이템 정렬 구현

    - 아이템 분리 구현

    - 인스펙터에서 가시적으로 아이템 슬롯 확인할 수 있도록(null인지 아닌지 여부) 커스텀 에디터 작성

    - 인벤토리 아이템 아이콘에 마우스 올리면 뜨는 툴팁 구현

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
        private InventoryUI _inventoryUI; // 연결된 인벤토리 UI

        /// <summary> 아이템 목록 </summary>
        [SerializeField]
        private Item[] _items;

        /// <summary> UI 갱신이 필요한 인덱스 목록 </summary>
        private List<int> _waitForUpdateList = new List<int>();

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            _items = new Item[_maxCapacity];
            Capacity = _initalCapacity;

            _inventoryUI.SetInventoryReference(this);
            UpdateAccessibleStatesAll();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 인덱스가 수용 범위 내에 있는지 검사 </summary>
        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < Capacity;
        }

        /// <summary> 앞에서부터 비어있는 슬롯 인덱스 탐색 </summary>
        private int FindEmptySlotIndex(int startIndex = 0)
        {
            for(int i = startIndex; i < Capacity; i++)
                if(_items[i] == null)
                    return i;
            return -1;
        }

        /// <summary> 앞에서부터 개수 여유가 있는 Countable 아이템의 슬롯 인덱스 탐색 </summary>
        private int FindCountableItemSlotIndex(CountableItemData target, int startIndex = 0)
        {
            for (int i = startIndex; i < Capacity; i++)
            {
                var current = _items[i];
                if(current == null)
                    continue;

                // 아이템 종류 일치, 개수 여유 확인
                if (current.Data == target && current is CountableItem ci)
                {
                    if(!ci.IsMax)
                        return i;
                }
            }

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
            _inventoryUI = inventoryUI;
            _inventoryUI.SetInventoryReference(this);
        }

        /// <summary> 인벤토리에 아이템 추가
        /// <para/> 넣는 데 실패한 잉여 아이템 개수 리턴
        /// <para/> 리턴이 0이면 넣는데 모두 성공했다는 의미
        /// </summary>
        public int Add(ItemData itemData, int amount = 1)
        {
            int index;

            // 1. 수량이 있는 아이템
            if (itemData is CountableItemData ciData)
            {
                bool findNextCountable = true;
                index = -1;

                while (amount > 0)
                {
                    // 1-1. 이미 해당 아이템이 인벤토리 내에 존재하고, 개수 여유 있는지 검사
                    if (findNextCountable)
                    {
                        index = FindCountableItemSlotIndex(ciData, index + 1);

                        // 개수 여유있는 기존재 슬롯이 더이상 없다고 판단될 경우, 빈 슬롯부터 탐색 시작
                        if (index == -1)
                        {
                            findNextCountable = false;
                            index = -1;
                        }
                        // 기존재 슬롯을 찾은 경우, 양 증가시키고 초과량 존재 시 amount에 초기화
                        else
                        {
                            CountableItem ci = _items[index] as CountableItem;
                            amount = ci.AddAmountAndGetExcess(amount);

                            UpdateUI(index);
                        }
                    }
                    // 1-2. 빈 슬롯 탐색
                    else
                    {
                        index = FindEmptySlotIndex(index + 1);

                        // 빈 슬롯조차 없는 경우 종료
                        if (index == -1)
                        {
                            break;
                        }
                        // 빈 슬롯 발견 시, 슬롯에 아이템 추가 및 잉여량 계산
                        else
                        {
                            _items[index] = new CountableItem(ciData, amount);
                            amount = (amount > ciData.MaxAmount) ? (amount - ciData.MaxAmount) : 0;

                            UpdateUI(index);
                        }
                    }
                }
            }
            // 2. 수량이 없는 아이템
            else
            {
                // 2-1. 1개만 넣는 경우, 간단히 수행
                if (amount == 1)
                {
                    index = FindEmptySlotIndex();
                    if (index != -1)
                    {
                        // 아이템을 슬롯에 추가
                        _items[index] = new Item(itemData);
                        amount = 0;

                        UpdateUI(index);
                    }
                }

                // 2-2. 2개 이상의 수량 없는 아이템을 동시에 추가하는 경우
                index = -1;
                for (; amount > 0; amount--)
                {
                    // 아이템 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                    index = FindEmptySlotIndex(index + 1);

                    // 다 넣지 못한 경우 루프 종료
                    if (index == -1)
                    {
                        break;
                    }

                    // 아이템을 슬롯에 추가
                    _items[index] = new Item(itemData);

                    UpdateUI(index);
                }
            }

            return amount;
        }

        /// <summary> 인벤토리에서 아이템 제거
        /// <para/> - 잘못된 인덱스를 참조한 경우 false 리턴
        /// </summary>
        public bool Remove(int index)
        {
            if(!IsValidIndex(index)) return false;

            _items[index] = null;
            _inventoryUI.RemoveItem(index);

            return true;
        }

        /// <summary> 두 인덱스의 아이템 위치를 서로 교체 </summary>
        public void Swap(int indexA, int indexB)
        {
            if(!IsValidIndex(indexA)) return;
            if(!IsValidIndex(indexB)) return;

            // Swap
            Item temp = _items[indexA];
            _items[indexA] = _items[indexB];
            _items[indexB] = temp;

            // Update Both
            UpdateUI(indexA);
            UpdateUI(indexB);
        }

        /// <summary> 해당 슬롯의 아이템 사용 </summary>
        public void Use(int index)
        {
            if(!IsValidIndex(index)) return;

            // 아이템 사용


            // UI 제거
            _inventoryUI.RemoveItem(index);
        }

        /// <summary> 해당하는 인덱스의 슬롯 상태를 UI에 갱신 </summary>
        public void UpdateUI(int index)
        {
            if(!IsValidIndex(index)) return;

            Item item = _items[index];

            // 아이템이 슬롯에 존재하는 경우 : 아이콘 등록
            if (item != null)
            {
                _inventoryUI.SetItem(index, item.Data.IconSprite);

                // 셀 수 있는 아이템이면 수량 텍스트 표시
                if (item is CountableItem ci)
                {
                    _inventoryUI.SetItemAmount(index, ci.Amount);
                }
                // 셀 수 없는 아이템인 경우 수량 : 1 (제거)
                else
                {
                    _inventoryUI.SetItemAmount(index, 1);
                }
            }
            // 빈 슬롯인 경우 : 아이콘 제거
            else
            {
                _inventoryUI.RemoveItem(index);
                _inventoryUI.SetItemAmount(index, -1); // 수량 텍스트 제거
            }
        }

        /// <summary> 해당하는 인덱스의 슬롯들을 UI에 갱신 </summary>
        public void UpdateUI(params int[] indices)
        {
            foreach (var i in indices)
            {
                UpdateUI(i);
            }
        }

        /// <summary> 모든 슬롯 UI에 접근 가능 여부 설정 </summary>
        public void UpdateAccessibleStatesAll()
        {
            _inventoryUI.SetAccessibleSlotRange(Capacity);
        }

        /// <summary> 해당 슬롯의 아이템 정보 넘겨주기 </summary>
        public ItemData GetItemData(int slotIndex)
        {
            if(!IsValidIndex(slotIndex)) return null;
            if(_items[slotIndex] == null) return null;

            return _items[slotIndex].Data;
        }

        #endregion
    }
}