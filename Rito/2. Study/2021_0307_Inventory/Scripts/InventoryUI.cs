using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-03-07 PM 7:34:31
// 작성자 : Rito

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// TODO : 에디터모드에서 기즈모로 슬롯들 배치될 영역 미리 계산해서 보여주기

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

namespace Rito.InventorySystem
{
    public class InventoryUI : MonoBehaviour
    {
        /***********************************************************************
        *                               Public Fields
        ***********************************************************************/
        #region .
        public int _slotCount = 20;
        public int _slotMargin = 8;
        public int _contentMargin = 20;

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        [Space]
        [SerializeField] private RectTransform _areaRectTransform; // 슬롯들이 위치할 영역
        [SerializeField] private GameObject _slotUiPrefab;

        private GraphicRaycaster _gr;
        private float _slotSize;

        #endregion

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            Init();
            InitSlots();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void Init()
        {
            TryGetComponent(out _gr);
            if(_gr == null)
                _gr = gameObject.AddComponent<GraphicRaycaster>();

            _slotUiPrefab.TryGetComponent(out RectTransform rt);
            _slotSize = rt.sizeDelta.x;
        }

        /// <summary> 지정된 개수만큼 슬롯 영역 내에 슬롯들 동적 생성 </summary>
        private void InitSlots()
        {
            Vector2 areaSize = _areaRectTransform.rect.size;
            Vector2 beginPos = new Vector2(_contentMargin, -_contentMargin);
            Vector2 curPos = beginPos;

            Debug.Log(areaSize);

            for (int i = 0; i < _slotCount; i++)
            {
                var slot = CloneSlot();
                slot.anchoredPosition = curPos;
                slot.gameObject.SetActive(true);

                // 다음 생성 위치 계산
                curPos.x += (_slotMargin + _slotSize);

                // 다음 줄로 넘어가기
                if (curPos.x + _slotMargin * 2 + _slotSize >= areaSize.x)
                {
                    curPos.x = beginPos.x;
                    curPos.y = curPos.y - (_slotMargin + _slotSize);
                }
            }
        }

        private RectTransform CloneSlot()
        {
            GameObject slotGo = Instantiate(_slotUiPrefab);
            RectTransform rt = slotGo.GetComponent<RectTransform>();
            rt.transform.SetParent(_areaRectTransform.transform);

            return rt;
        }

        #endregion
    }
}