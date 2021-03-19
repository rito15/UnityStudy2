using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-03-07 PM 10:20:05
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class ItemSlotUI : MonoBehaviour, 
        IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        /***********************************************************************
        *                               Option Fields
        ***********************************************************************/
        #region .
        [Tooltip("슬롯 내에서 아이콘과 슬롯 사이의 여백")]
        public float _padding = 1f;

        #endregion
        /***********************************************************************
        *                               Properties
        ***********************************************************************/
        #region .
        /// <summary> 슬롯의 인덱스 </summary>
        public int Index { get; private set; }

        /// <summary> 슬롯이 아이템을 보유하고 있는지 여부 </summary>
        public bool HasItem => _iconImage.sprite != null;

        /// <summary> 성공적으로 드래그를 하고 있는지 여부 </summary>
        public bool IsDragging { get; private set; }

        #endregion
        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .
        private InventoryUI _inventoryUI;
        private GameObject _iconGo;
        private RectTransform _iconRect;
        private Image _iconImage;

        private Vector2 _startingPoint;
        private Vector2 _moveBegin;
        private int _sbIndex;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            Init();
        }

        #endregion
        /***********************************************************************
        *                               Pointer Events
        ***********************************************************************/
        #region .
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if(HasItem == false) return;

            _startingPoint = _iconRect.position;
            _moveBegin = eventData.position;

            _sbIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling(); // 맨 위로 올려주기

            _inventoryUI.BeginDrag(this);
            IsDragging = true;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if(!IsDragging) return;

            Vector2 _moveOffset = eventData.position - _moveBegin;
            _iconRect.position = _startingPoint + _moveOffset;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragging) return;

            _iconRect.position = _startingPoint;
            transform.SetSiblingIndex(_sbIndex);

            _inventoryUI.EndDrag();
            IsDragging = false;
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void Init()
        {
            _inventoryUI = GetComponentInParent<InventoryUI>();
            _iconRect = transform.GetChild(0).GetComponent<RectTransform>();
            _iconGo = _iconRect.gameObject;
            _iconImage = _iconRect.GetComponent<Image>();

            // 1. Item Rect
            _iconRect.pivot = new Vector2(0.5f, 0.5f);
            _iconRect.anchorMin = Vector2.zero;
            _iconRect.anchorMax = Vector2.one;

            // 패딩 조절
            _iconRect.offsetMin = Vector2.one * (_padding);
            _iconRect.offsetMax = Vector2.one * (-_padding);

            // 2. Image
            _iconImage.raycastTarget = false;

            // 3. Deactivate Icon
            HideIcon();
        }

        private void ShowIcon() => _iconGo.SetActive(true);
        private void HideIcon() => _iconGo.SetActive(false);

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        public void SetSlotIndex(int index) => Index = index;

        /// <summary> 다른 슬롯과 아이템 아이콘 교환 </summary>
        public void ExchangeOrMoveIcon(ItemSlotUI other)
        {
            if (other == null) return;

            // 자기 자신과 교환 불가
            if (other == this) return;

            var temp = _iconImage.sprite;

            // 1. 대상에 아이템이 있는 경우 : 교환
            if (other.HasItem) SetItem(other._iconImage.sprite);

            // 2. 없는 경우 : 이동
            else RemoveItem();

            other.SetItem(temp);
        }

        /// <summary> 슬롯에 아이템 등록 </summary>
        public void SetItem(Sprite itemSprite)
        {
            _iconImage.sprite = itemSprite;
            ShowIcon();
        }

        /// <summary> 슬롯에서 아이템 제거 </summary>
        public void RemoveItem()
        {
            _iconImage.sprite = null;
            HideIcon();
        }

        /// <summary> 이미지 투명도 설정 </summary>
        public void SetIconAlpha(float alpha)
        {
            _iconImage.color = new Color(
                _iconImage.color.r, _iconImage.color.g, _iconImage.color.b, alpha
            );
        }

        #endregion
    }
}