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

        private static readonly Vector2 LeftTop = new Vector2(0f, 1f);
        private static readonly Vector2 LeftBottom = new Vector2(0f, 0f);
        private static readonly Vector2 RightTop = new Vector2(1f, 1f);
        private static readonly Vector2 RightBottom = new Vector2(1f, 0f);

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        // Awake()가 InventoryUI.Awake()보다 늦게 호출되면 미리 게임오브젝트가 꺼져서
        // 호출되지 않을 수 있음

        //private void Awake()
        //{
        //    TryGetComponent(out _rt);

        //    // NOTE : 게임 시작 시 스스로 Hide()하지 않고, 인벤토리 UI에서 Hide()
        //}

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 모든 자식 UI에 레이캐스트 타겟 해제 </summary>
        private void SetAllChildrenDisableRaycastTarget(Transform tr)
        {
            // 본인이 Graphic(UI)를 상속하면 레이캐스트 타겟 해제
            tr.TryGetComponent(out Graphic gr);
            if(gr != null)
                gr.raycastTarget = false;

            // 자식이 없으면 종료
            int childCount = tr.childCount;
            if (childCount == 0) return;

            for (int i = 0; i < childCount; i++)
            {
                SetAllChildrenDisableRaycastTarget(tr.GetChild(i));
            }
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> Awake() 대용 </summary>
        public void Init()
        {
            TryGetComponent(out _rt);
            _rt.pivot = LeftTop;

            SetAllChildrenDisableRaycastTarget(transform);
        }

        /// <summary> 툴팁 UI에 아이템 정보 등록 </summary>
        public void SetItemInfo(ItemData data)
        {
            _titleText.text = data.Name;
            _contentText.text = data.Tooltip;
        }

        /// <summary> 툴팁의 위치 조정 </summary>
        public void SetRectPosition(RectTransform slotRect)
        {
            // 해상도에 유동적으로 대응 필요!!!!!!!!


            _rt.position = slotRect.position; 
            //Debug.Log($"offsetMin : {slotRect.offsetMin}, offsetMax : {slotRect.offsetMax}");
            //Debug.Log($"sizeDelta : {slotRect.sizeDelta}");
            //Debug.Log($"width : {slotRect.rect.width}");
            //Debug.Log($"anchoredPosition : {slotRect.anchoredPosition}");
            Debug.Log($"position : {slotRect.position}");

            //return;


            _rt.position = slotRect.position + new Vector3(slotRect.rect.width, -slotRect.rect.height);

            float slotWidth = slotRect.rect.width;
            float width = _rt.rect.width;
            float height = _rt.rect.height;
            Vector2 pos = _rt.position;

            // 우측, 하단이 잘렸는지 여부
            bool rightTruncated = pos.x + width > Screen.width;
            bool bottomTruncated = pos.y - height < 0f;

            ref bool R = ref rightTruncated;
            ref bool B = ref bottomTruncated;

            // 오른쪽만 잘림 => 슬롯의 Left Bottom 방향으로 표시
            if (R && !B)
            {
                _rt.position = new Vector2(pos.x - width - slotWidth, pos.y);
            }
            // 아래쪽만 잘림 => 슬롯의 Right Top 방향으로 표시
            else if (!R && B)
            {
                _rt.position = new Vector2(pos.x, pos.y + height);
            }
            // 모두 잘림 => 슬롯의 Left Top 방향으로 표시
            else if (R && B)
            {
                _rt.position = new Vector2(pos.x - width - slotWidth, pos.y + height);
            }
            // 잘리지 않음 => 슬롯의 Right Bottom 방향으로 표시
            // Do Nothing
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        #endregion
    }
}