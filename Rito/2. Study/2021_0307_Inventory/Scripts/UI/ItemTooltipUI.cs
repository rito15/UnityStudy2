using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-01 PM 8:33:22
// 작성자 : Rito

namespace Rito.InventorySystem
{
    /// <summary> 슬롯 내의 아이템 아이콘에 마우스를 올렸을 때 보이는 툴팁 </summary>
    public class ItemTooltipUI : MonoBehaviour
    {
        /***********************************************************************
        *                           Inspector Option Fields
        ***********************************************************************/
        #region .
        [SerializeField]
        private Text _titleText;   // 아이템 이름 텍스트

        [SerializeField]
        private Text _contentText; // 아이템 설명 텍스트

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        private RectTransform _rt;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            TryGetComponent(out _rt);

            // NOTE : 게임 시작 시 스스로 Hide()하지 않고, 인벤토리 UI에서 Hide()
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 툴팁 UI에 아이템 정보 등록 </summary>
        public void SetItemInfo(ItemData data)
        {
            _titleText.text = data.Name;
            _contentText.text = data.Tooltip;
        }

        /// <summary> 툴팁의 위치 지정 </summary>
        public void SetRectPosition()
        {
            // 갸악
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        #endregion
    }
}