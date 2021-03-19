using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/*

   TODO

   - 그래픽 레이캐스터 이용해서 드래그앤 드롭으로 놓았을 때, 놓은 지점 아이콘 2개 이용해서
     서로의 이미지 바꾸기 (추후 : 아이템 정보도 교환)

   - 놓았을 때 놓은 지점에 아이콘이 1개라면 허공에 놓았다고 간주하여 버리도록

*/

// 날짜 : 2021-03-07 PM 7:34:31
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class InventoryUI : MonoBehaviour
    {
        /***********************************************************************
        *                               Public Fields
        ***********************************************************************/
        #region .
        [Range(0, 10)]
        [SerializeField] private int _slotCountPerLine = 8;  // 한 줄 당 슬롯 개수
        [Range(0, 10)]
        [SerializeField] private int _slotLineCount = 8;     // 슬롯 줄 수
        [SerializeField] private float _slotMargin = 8f;     // 한 슬롯의 상하좌우 여백
        [SerializeField] private float _contentAreaPadding = 20f; // 인벤토리 영역의 내부 여백
        [Range(32, 64)]
        [SerializeField] private float _slotSize = 64f;      // 각 슬롯의 크기

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .
        [Space]
        [SerializeField] private RectTransform _contentAreaRT; // 슬롯들이 위치할 영역
        [SerializeField] private GameObject _slotUiPrefab;

        private List<ItemSlotUI> _slotUIList = new List<ItemSlotUI>();
        private GraphicRaycaster _gr;
        private PointerEventData _ped;
        private List<RaycastResult> _rrList;

        private ItemSlotUI _beginDragSlot; // 현재 드래그를 시작한 슬롯
        private ItemSlotUI _endDragSlot; // 현재 드래그 마치는 지점의 슬롯


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
            if (_gr == null)
                _gr = gameObject.AddComponent<GraphicRaycaster>();

            // 1. Rect Transform
            _slotUiPrefab.TryGetComponent(out RectTransform slotRT);
            slotRT.sizeDelta = new Vector2(_slotSize, _slotSize);

            // 2. Slot UI
            _slotUiPrefab.TryGetComponent(out ItemSlotUI slotUI);
            if (slotUI == null)
                _slotUiPrefab.AddComponent<ItemSlotUI>();

            _slotUiPrefab.SetActive(false);

            // 3. Graphic Raycaster
            _ped = new PointerEventData(EventSystem.current);
            _rrList = new List<RaycastResult>(10);
        }

        /// <summary> 지정된 개수만큼 슬롯 영역 내에 슬롯들 동적 생성 </summary>
        private void InitSlots()
        {
            Vector2 beginPos = new Vector2(_contentAreaPadding, -_contentAreaPadding);
            Vector2 curPos = beginPos;

            _slotUIList = new List<ItemSlotUI>(_slotLineCount * _slotCountPerLine);

            // Init Slot Grid
            for (int j = 0; j < _slotLineCount; j++)
            {
                for (int i = 0; i < _slotCountPerLine; i++)
                {
                    int slotIndex = (_slotCountPerLine * j) + i;

                    var slotRT = CloneSlot();
                    slotRT.anchoredPosition = curPos;
                    slotRT.gameObject.SetActive(true);
                    slotRT.gameObject.name = $"Item Slot [{slotIndex}]";

                    var slotUI = slotRT.GetComponent<ItemSlotUI>();
                    slotUI.SetSlotIndex(slotIndex);
                    _slotUIList.Add(slotUI);

                    // Next X
                    curPos.x += (_slotMargin + _slotSize);
                }

                // Next Line
                curPos.x = beginPos.x;
                curPos.y -= (_slotMargin + _slotSize);
            }
        }

        private RectTransform CloneSlot()
        {
            GameObject slotGo = Instantiate(_slotUiPrefab);
            RectTransform rt = slotGo.GetComponent<RectTransform>();
            rt.transform.SetParent(_contentAreaRT.transform);

            return rt;
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        public void BeginDrag(ItemSlotUI beginDragSlot)
        {
            _beginDragSlot = beginDragSlot;
        }

        public void EndDrag()
        {
            bool isHandled = false;
            _ped.position = Input.mousePosition;
            _rrList.Clear();

            _gr.Raycast(_ped, _rrList);
            int resultCount = _rrList.Count;

            // 아이템 슬롯끼리 아이콘 교환 또는 전이
            if (resultCount > 0)
            {
                ItemSlotUI endDragSlot = _rrList[0].gameObject.GetComponent<ItemSlotUI>();

                if (endDragSlot != null)
                {
                    _beginDragSlot.ExchangeOrMoveIcon(endDragSlot);
                    isHandled = true;
                }
            }

            // 4. 기타 : 버릴까요?
            if (!isHandled)
            {

            }

            _beginDragSlot = null;
        }

        public void Test_AddItemIcon(int index, Sprite icon)
        {
            _slotUIList[index].SetItem(icon);
        }

        #endregion
        /***********************************************************************
        *                               Editor Preview
        ***********************************************************************/
        #region .
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private bool __showPreview = false;

        [Range(0.01f, 1f)]
        [SerializeField] private float __previewAlpha = 0.1f;

        private List<GameObject> __previewSlotGoList = new List<GameObject>();
        private int __prevSlotCountPerLine;
        private int __prevSlotLineCount;
        private float __prevSlotSize;
        private float __prevSlotMargin;
        private float __prevContentPadding;
        private float __prevAlpha;
        private bool __prevShow = false;

        private void OnValidate()
        {
            if(Application.isPlaying) return;

            if (__showPreview && !__prevShow)
            {
                CreateSlots();
            }
            __prevShow = __showPreview;

            if (Unavailable())
            {
                ClearAll();
                return;
            }
            if (CountChanged())
            {
                ClearAll();
                CreateSlots();
                __prevSlotCountPerLine = _slotCountPerLine;
                __prevSlotLineCount = _slotLineCount;
            }
            if (ValueChanged())
            {
                DrawGrid();
                __prevSlotSize = _slotSize;
                __prevSlotMargin = _slotMargin;
                __prevContentPadding = _contentAreaPadding;
            }
            if (AlphaChanged())
            {
                SetImageAlpha();
                __prevAlpha = __previewAlpha;
            }

            bool Unavailable()
            {
                return !__showPreview ||
                        _slotCountPerLine < 1 ||
                        _slotLineCount < 1 ||
                        _slotSize <= 0f ||
                        _contentAreaRT == null ||
                        _slotUiPrefab == null;
            }
            bool CountChanged()
            {
                return _slotCountPerLine != __prevSlotCountPerLine ||
                       _slotLineCount != __prevSlotLineCount;
            }
            bool ValueChanged()
            {
                return _slotSize != __prevSlotSize ||
                       _slotMargin != __prevSlotMargin ||
                       _contentAreaPadding != __prevContentPadding;
            }
            bool AlphaChanged()
            {
                return __previewAlpha != __prevAlpha;
            }
            void ClearAll()
            {
                foreach (var go in __previewSlotGoList)
                {
                    Destroyer.Destroy(go);
                }
                __previewSlotGoList.Clear();
            }
            void CreateSlots()
            {
                int count = _slotCountPerLine * _slotLineCount;
                __previewSlotGoList.Capacity = count;

                for (int i = 0; i < count; i++)
                {
                    GameObject slotGo = Instantiate(_slotUiPrefab);
                    slotGo.transform.SetParent(_contentAreaRT.transform);
                    slotGo.SetActive(true);
                    slotGo.AddComponent<PreviewItemSlot>();

                    HideGameObject(slotGo);

                    __previewSlotGoList.Add(slotGo);
                }

                DrawGrid();
                SetImageAlpha();
            }
            void DrawGrid()
            {
                Vector2 beginPos = new Vector2(_contentAreaPadding, -_contentAreaPadding);
                Vector2 curPos = beginPos;

                // Draw Slot Grid
                int index = 0;
                for (int j = 0; j < _slotLineCount; j++)
                {
                    for (int i = 0; i < _slotCountPerLine; i++)
                    {
                        GameObject slotGo = __previewSlotGoList[index++];
                        RectTransform slotRT = slotGo.GetComponent<RectTransform>();

                        slotRT.anchoredPosition = curPos;
                        slotRT.sizeDelta = new Vector2(_slotSize, _slotSize);
                        __previewSlotGoList.Add(slotGo);

                        // Next X
                        curPos.x += (_slotMargin + _slotSize);
                    }

                    // Next Line
                    curPos.x = beginPos.x;
                    curPos.y -= (_slotMargin + _slotSize);
                }
            }
            void HideGameObject(GameObject go)
            {
                go.hideFlags = HideFlags.HideAndDontSave;

                Transform tr = go.transform;
                for (int i = 0; i < tr.childCount; i++)
                {
                    tr.GetChild(i).gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            void SetImageAlpha()
            {
                foreach (var go in __previewSlotGoList)
                {
                    var images = go.GetComponentsInChildren<Image>();
                    foreach (var img in images)
                    {
                        img.color = new Color(img.color.r, img.color.g, img.color.b, __previewAlpha);
                        var outline = img.GetComponent<Outline>();
                        if (outline)
                            outline.effectColor = new Color(outline.effectColor.r, outline.effectColor.g, outline.effectColor.b, __previewAlpha);
                    }
                }
            }
        }

        private class PreviewItemSlot : MonoBehaviour { }

        [UnityEditor.InitializeOnLoad]
        private static class Destroyer
        {
            private static Queue<GameObject> targetQueue = new Queue<GameObject>();

            static Destroyer()
            { 
                UnityEditor.EditorApplication.update += () =>
                {
                    for (int i = 0; targetQueue.Count > 0 && i < 100000; i++)
                    {
                        var next = targetQueue.Dequeue();
                        DestroyImmediate(next);
                    }
                };
            }
            public static void Destroy(GameObject go) => targetQueue.Enqueue(go);
        }
#endif

        #endregion
    }
}