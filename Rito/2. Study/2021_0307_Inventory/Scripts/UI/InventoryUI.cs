using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/*
    [기능]
    - 에디터모드에서 슬롯 미리보기
    - 아이템 드래그 앤 드롭으로 이동, 교환, 버리기
    - 슬롯에 아이템 등록
    - 슬롯에서 아이템 제거
    - 개수 수정
*/

/*
    TODO

    - 
*/

// 날짜 : 2021-03-07 PM 7:34:31
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public class InventoryUI : MonoBehaviour
    {
        /***********************************************************************
        *                               Option Fields
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

        [Space]
        [SerializeField] private RectTransform _contentAreaRT; // 슬롯들이 위치할 영역
        [SerializeField] private GameObject _slotUiPrefab;     // 슬롯의 원본 프리팹
        [SerializeField] private ItemTooltipUI _itemTooltip;   // 아이템 정보를 보여줄 툴팁 UI
        [SerializeField] private GameObject _barrierGO;        // 클릭 방지 UI

        [Space]
        [SerializeField] private bool _mouseReversed = false; // 마우스 클릭 반전 여부

        #endregion
        /***********************************************************************
        *                               Private Fields
        ***********************************************************************/
        #region .

        /// <summary> 연결된 인벤토리 </summary>
        private Inventory _inventory;

        private List<ItemSlotUI> _slotUIList = new List<ItemSlotUI>();
        private GraphicRaycaster _gr;
        private PointerEventData _ped;
        private List<RaycastResult> _rrList;

        private ItemSlotUI _pointerOverSlot; // 현재 포인터가 위치한 곳의 슬롯
        private ItemSlotUI _beginDragSlot; // 현재 드래그를 시작한 슬롯
        private Transform _beginDragIconTransform; // 해당 슬롯의 아이콘 트랜스폼

        private int _leftClick = 0;
        private int _rightClick = 1;

        private Vector3 _beginDragIconPoint;   // 드래그 시작 시 슬롯의 위치
        private Vector3 _beginDragCursorPoint; // 드래그 시작 시 커서의 위치
        private int _beginDragSlotSiblingIndex;

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

        private void Update()
        {
            _ped.position = Input.mousePosition;

            OnPointerEnterAndExit();
            ShowOrHideItemTooltip();
            OnPointerDown();
            OnPointerDrag();
            OnPointerUp();
        }

        #endregion
        /***********************************************************************
        *                               Init Methods
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

            // 4. Item Tooltip UI
            if (_itemTooltip == null)
            {
                _itemTooltip = GetComponentInChildren<ItemTooltipUI>();
                EditorLog("인스펙터에서 아이템 툴팁 UI를 직접 지정하지 않아 자식에서 발견하여 초기화하였습니다.");
            }
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
                    slotRT.pivot = new Vector2(0f, 1f); // Left Top
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

            RectTransform CloneSlot()
            {
                GameObject slotGo = Instantiate(_slotUiPrefab);
                RectTransform rt = slotGo.GetComponent<RectTransform>();
                rt.transform.SetParent(_contentAreaRT.transform);

                return rt;
            }
        }

        #endregion
        /***********************************************************************
        *                               Mouse Event Methods
        ***********************************************************************/
        #region .
        private bool IsOverUI()
            => EventSystem.current.IsPointerOverGameObject();

        /// <summary> 레이캐스트하여 얻은 첫 번째 UI에서 컴포넌트 찾아 리턴 </summary>
        private T RaycastAndGetFirstComponent<T>() where T : Component
        {
            _rrList.Clear();

            _gr.Raycast(_ped, _rrList);
            
            if(_rrList.Count == 0)
                return null;

            T component = _rrList[0].gameObject.GetComponent<T>();
            return (component != null) ? component : null;
        }
        /// <summary> 슬롯에 포인터가 올라가는 경우, 슬롯에서 포인터가 빠져나가는 경우 </summary>
        private void OnPointerEnterAndExit()
        {
            // 이전 프레임의 슬롯
            var prevSlot = _pointerOverSlot;

            // 현재 프레임의 슬롯
            var curSlot = _pointerOverSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

            if (prevSlot == null)
            {
                // Enter
                if (curSlot != null)
                {
                    //EditorLog($"Mouse Over : Slot [{curSlot.Index}]");
                    OnCurrentEnter();
                }
            }
            else
            {
                // Exit
                if (curSlot == null)
                {
                    //EditorLog($"Mouse Exit : Slot [{prevSlot.Index}]");
                    OnPrevExit();
                }

                // Change
                else if (prevSlot != curSlot)
                {
                    //EditorLog($"Focus Changed : Slot [{prevSlot.Index}] -> [{curSlot.Index}]");
                    OnPrevExit();
                    OnCurrentEnter();
                }
            }

            // ===================== Local Methods ===============================
            void OnCurrentEnter()
            {
                curSlot.Highlight(true);

                if (curSlot.HasItem)
                {
                    EditorLog($"Mouse Over : Slot [{curSlot.Index}]");

                    // 툴팁 정보 갱신
                    UpdateTooltipUI(curSlot);
                }
            }
            void OnPrevExit()
            {
                prevSlot.Highlight(false);

                //if (prevSlot.HasItem)
                //{
                //    EditorLog($"Mouse Exit : Slot [{prevSlot.Index}]");
                //}
            }
        }
        /// <summary> 아이템 정보 툴팁 보여주거나 감추기 </summary>
        private void ShowOrHideItemTooltip()
        {
            // 마우스가 유효한 아이템 아이콘 위에 올라와 있다면 툴팁 보여주기
            bool isValid =
                _pointerOverSlot != null && _pointerOverSlot.HasItem && _pointerOverSlot.IsAccessible
                && (_pointerOverSlot != _beginDragSlot); // 드래그 시작한 슬롯이면 보여주지 않기

            if (isValid)
                _itemTooltip.Show();
            else
                _itemTooltip.Hide();
        }
        /// <summary> 슬롯에 클릭하는 경우 </summary>
        private void OnPointerDown()
        {
            // Left Click : Begin Drag
            if (Input.GetMouseButtonDown(_leftClick))
            {
                _beginDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

                // 아이템을 갖고 있는 슬롯만 해당
                if (_beginDragSlot != null && _beginDragSlot.HasItem)
                {
                    EditorLog($"Drag Begin : Slot [{_beginDragSlot.Index}]");

                    // 위치 기억, 참조 등록
                    _beginDragIconTransform = _beginDragSlot.IconRect.transform;
                    _beginDragIconPoint = _beginDragIconTransform.position;
                    _beginDragCursorPoint = Input.mousePosition;

                    // 맨 위에 보이기
                    _beginDragSlotSiblingIndex = _beginDragSlot.transform.GetSiblingIndex();
                    _beginDragSlot.transform.SetAsLastSibling();
                }
                else
                {
                    _beginDragSlot = null;
                }
            }

            // Right Click : Use Item
            else if (Input.GetMouseButtonDown(_rightClick))
            {
                ItemSlotUI slot = RaycastAndGetFirstComponent<ItemSlotUI>();

                if (slot != null && slot.HasItem)
                {
                    TryUseItem(slot.Index);
                }
            }
        }
        private void OnPointerDrag()
        {
            if(_beginDragSlot == null) return;

            if (Input.GetMouseButton(_leftClick))
            {
                // 위치 이동
                _beginDragIconTransform.position =
                    _beginDragIconPoint + (Input.mousePosition - _beginDragCursorPoint);
            }
        }
        private void OnPointerUp()
        {
            if (Input.GetMouseButtonUp(_leftClick))
            {
                // End Drag
                if (_beginDragSlot != null)
                {
                    // 위치 복원
                    _beginDragIconTransform.position = _beginDragIconPoint;

                    // UI 순서 복원
                    _beginDragSlot.transform.SetSiblingIndex(_beginDragSlotSiblingIndex);

                    // 드래그 완료 처리
                    EndDrag();

                    // 참조 제거
                    _beginDragSlot = null;
                    _beginDragIconTransform = null;
                }
            }
        }

        private void EndDrag()
        {
            ItemSlotUI endDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

            // 아이템 슬롯끼리 아이콘 교환 또는 이동
            if (endDragSlot != null)
            {
                //EditorLog((_beginDragSlot == endDragSlot) ?
                //    $"Drag End(Same Slot) : [{_beginDragSlot.Index}]" : 
                //    $"Drag End({(endDragSlot.HasItem ? "Swap" : "Move")}) : Slot [{_beginDragSlot.Index} -> {endDragSlot.Index}]");

                TrySwapItems(_beginDragSlot, endDragSlot);

                // 툴팁 갱신
                UpdateTooltipUI(endDragSlot);
                return;
            }

            // 버리기(커서가 UI 레이캐스트 타겟 위에 있지 않은 경우)
            if (!IsOverUI())
            {
                //EditorLog($"Drag End(Remove) : Slot [{_beginDragSlot.Index}]");

                TryRemoveItem(_beginDragSlot.Index);
            }
            // 슬롯이 아닌 다른 UI 위에 놓은 경우
            else
            {
                EditorLog($"Drag End(Do Nothing)");
            }
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .

        /// <summary> UI 및 인벤토리에서 아이템 제거 </summary>
        private void TryRemoveItem(int index)
        {
            EditorLog($"UI - Try Remove Item : Slot [{index}]");

            _inventory.Remove(index);
        }

        /// <summary> 아이템 사용 </summary>
        private void TryUseItem(int index)
        {
            EditorLog($"UI - Try Use Item : Slot [{index}]");

            _inventory.Use(index);
        }

        /// <summary> 두 슬롯의 아이템 교환 </summary>
        private void TrySwapItems(ItemSlotUI from, ItemSlotUI to)
        {
            if (from == to)
            {
                EditorLog($"UI - Try Swap Items: Same Slot [{from.Index}]");
                return;
            }

            EditorLog($"UI - Try Swap Items: Slot [{from.Index} -> {to.Index}]");

            from.SwapOrMoveIcon(to);
            _inventory.Swap(from.Index, to.Index);
        }

        /// <summary> 툴팁 UI의 슬롯 데이터 갱신 </summary>
        private void UpdateTooltipUI(ItemSlotUI slot)
        {
            if(!slot.IsAccessible || !slot.HasItem)
                return;

            // 툴팁 정보 갱신
            _itemTooltip.SetItemInfo(_inventory.GetItemData(slot.Index));

            // 툴팁 위치 조정
            _itemTooltip.SetRectPosition(slot.SlotRect);
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        /// <summary> 인벤토리 참조 등록 (인벤토리에서 직접 호출) </summary>
        public void SetInventoryReference(Inventory inventory)
        {
            _inventory = inventory;
        }

        /// <summary> 마우스 클릭 좌우 반전시키기 (true : 반전) </summary>
        public void InvertMouse(bool value)
        {
            _leftClick = value ? 1 : 0;
            _rightClick = value ? 0 : 1;

            _mouseReversed = value;
        }

        /// <summary> 슬롯에 아이템 아이콘 등록 </summary>
        public void SetItemIcon(int index, Sprite icon)
        {
            EditorLog($"Set Item Icon : Slot [{index}]");

            _slotUIList[index].SetItem(icon);
        }

        /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
        public void SetItemAmountText(int index, int amount)
        {
            EditorLog($"Set Item Amount Text : Slot [{index}], Amount [{amount}]");

            // NOTE : amount가 1 이하일 경우 텍스트 미표시
            _slotUIList[index].SetItemAmount(amount);
        }

        /// <summary> 해당 슬롯의 아이템 개수 텍스트 지정 </summary>
        public void HideItemAmountText(int index)
        {
            EditorLog($"Hide Item Amount Text : Slot [{index}]");

            _slotUIList[index].SetItemAmount(1);
        }

        /// <summary> 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기 </summary>
        public void RemoveItem(int index)
        {
            EditorLog($"Remove Item : Slot [{index}]");

            _slotUIList[index].RemoveItem();
        }

        /// <summary> 접근 가능한 슬롯 범위 설정 </summary>
        public void SetAccessibleSlotRange(int accessibleSlotCount)
        {
            for (int i = 0; i < _slotUIList.Count; i++)
            {
                _slotUIList[i].SetAccessibleState(i < accessibleSlotCount);
            }
        }

        #endregion
        /***********************************************************************
        *                               Editor Only Debug
        ***********************************************************************/
        #region .
#if UNITY_EDITOR
        [Space]
        [SerializeField] private bool _showDebug = true;
#endif
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void EditorLog(object message)
        {
            if (!_showDebug) return;
            UnityEngine.Debug.Log($"[InventoryUI] {message}");
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
        private bool __prevMouseReversed = false;

        private void OnValidate()
        {
            if (__prevMouseReversed != _mouseReversed)
            {
                __prevMouseReversed = _mouseReversed;
                InvertMouse(_mouseReversed);

                EditorLog($"Mouse Reversed : {_mouseReversed}");
            }

            if (Application.isPlaying) return;

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

                // 슬롯의 피벗은 Left Top으로 고정
                RectTransform slotPrefabRT = _slotUiPrefab.GetComponent<RectTransform>();
                slotPrefabRT.pivot = new Vector2(0f, 1f);

                for (int i = 0; i < count; i++)
                {
                    GameObject slotGo = Instantiate(_slotUiPrefab);
                    slotGo.transform.SetParent(_contentAreaRT.transform);
                    slotGo.SetActive(true);
                    slotGo.AddComponent<PreviewItemSlot>();

                    slotGo.transform.localScale = Vector3.one; // 버그 해결

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

                // Draw Slots
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