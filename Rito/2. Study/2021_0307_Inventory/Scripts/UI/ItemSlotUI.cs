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
    public class ItemSlotUI : MonoBehaviour
    {
        /***********************************************************************
        *                               Option Fields
        ***********************************************************************/
        #region .
        [Tooltip("슬롯 내에서 아이콘과 슬롯 사이의 여백")]
        public float _padding = 1f;

        [Tooltip("아이템 아이콘 이미지")]
        [SerializeField] private Image _iconImage;

        [Tooltip("아이템 개수 텍스트")]
        [SerializeField] private Text _amountText;

        [Tooltip("슬롯이 포커스될 때 나타나는 하이라이트 이미지")]
        [SerializeField] private Image _highlightImage;

        [Space]
        [Tooltip("하이라이트 이미지 알파 값")]
        [SerializeField] private float _highlightAlpha = 0.5f;

        [Tooltip("하이라이트 소요 시간")]
        [SerializeField] private float _highlightFadeDuration = 0.2f;

        #endregion
        /***********************************************************************
        *                               Properties
        ***********************************************************************/
        #region .
        /// <summary> 슬롯의 인덱스 </summary>
        public int Index { get; private set; }

        /// <summary> 슬롯이 아이템을 보유하고 있는지 여부 </summary>
        public bool HasItem => _iconImage.sprite != null;

        public RectTransform IconRect => _iconRect;

        #endregion
        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .
        private InventoryUI _inventoryUI;

        private RectTransform _iconRect;
        private RectTransform _highlightRect;

        private GameObject _iconGo;
        private GameObject _textGo;
        private GameObject _highlightGo;

        private float _currentHLAlpha = 0f;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            InitComponents();
            InitValues();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void InitComponents()
        {
            _inventoryUI = GetComponentInParent<InventoryUI>();

            // Rects
            _iconRect = _iconImage.rectTransform;
            _highlightRect = _highlightImage.rectTransform;

            // Game Objects
            _iconGo = _iconRect.gameObject;
            _textGo = _amountText.gameObject;
            _highlightGo = _highlightImage.gameObject;
        }
        private void InitValues()
        {
            // 1. Item Icon, Highlight Rect
            _iconRect.pivot = new Vector2(0.5f, 0.5f);
            _iconRect.anchorMin = Vector2.zero;
            _iconRect.anchorMax = Vector2.one;

            // 패딩 조절
            _iconRect.offsetMin = Vector2.one * (_padding);
            _iconRect.offsetMax = Vector2.one * (-_padding);

            // 아이콘과 하이라이트 크기가 동일하도록
            _highlightRect.pivot = _iconRect.pivot;
            _highlightRect.anchorMin = _iconRect.anchorMin;
            _highlightRect.anchorMax = _iconRect.anchorMax;
            _highlightRect.offsetMin = _iconRect.offsetMin;
            _highlightRect.offsetMax = _iconRect.offsetMax;

            // 2. Image
            _iconImage.raycastTarget = false;
            _highlightImage.raycastTarget = false;

            // 3. Deactivate Icon
            HideIcon();
            _highlightGo.SetActive(false);
        }

        private void ShowIcon() => _iconGo.SetActive(true);
        private void HideIcon() => _iconGo.SetActive(false);

        private void ShowText() => _textGo.SetActive(true);
        private void HideText() => _textGo.SetActive(false);

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        public void SetSlotIndex(int index) => Index = index;

        /// <summary> 다른 슬롯과 아이템 아이콘 교환 </summary>
        public void SwapOrMoveIcon(ItemSlotUI other)
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

        /// <summary> 아이템 개수 텍스트 설정 </summary>
        public void SetItemAmount(int amount)
        {
            if(HasItem && amount > 1)
                ShowText();
            else
                HideText();

            _amountText.text = amount.ToString();
        }

        public void Highlight(bool value)
        {
            if (value)
                StartCoroutine("HighlightFadeInRoutine");
            else
                StartCoroutine("HighlightFadeOutRoutine");
        }

        #endregion
        /***********************************************************************
        *                               Coroutines
        ***********************************************************************/
        #region .
        /// <summary> 하이라이트 알파값 0f => 1f 서서히 증가 </summary>
        private IEnumerator HighlightFadeInRoutine()
        {
            StopCoroutine("HighlightFadeOutRoutine");
            _highlightGo.SetActive(true);

            float unit = _highlightAlpha / _highlightFadeDuration;

            for (; _currentHLAlpha <= _highlightAlpha; _currentHLAlpha += unit * Time.deltaTime)
            {
                _highlightImage.color = new Color(
                    _highlightImage.color.r,
                    _highlightImage.color.g,
                    _highlightImage.color.b,
                    _currentHLAlpha
                );

                yield return null;
            }
        }

        private IEnumerator HighlightFadeOutRoutine()
        {
            StopCoroutine("HighlightFadeInRoutine");

            float unit = _highlightAlpha / _highlightFadeDuration;

            for (; _currentHLAlpha >= 0f; _currentHLAlpha -= unit * Time.deltaTime)
            {
                _highlightImage.color = new Color(
                    _highlightImage.color.r,
                    _highlightImage.color.g,
                    _highlightImage.color.b,
                    _currentHLAlpha
                );

                yield return null;
            }

            _highlightGo.SetActive(false);
        }

        #endregion
    }
}