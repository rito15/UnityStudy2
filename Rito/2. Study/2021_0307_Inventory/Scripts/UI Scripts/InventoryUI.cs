using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

            // 3. Icon UI
            Transform iconTr = slotRT.transform.GetChild(0);
            if (iconTr != null)
            {
                iconTr.TryGetComponent(out ItemIconUI iconUI);
                if(iconUI == null)
                    iconTr.gameObject.AddComponent<ItemIconUI>();
            }

            _slotUiPrefab.SetActive(false);
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
                    var slotRT = CloneSlot();
                    slotRT.anchoredPosition = curPos;
                    slotRT.gameObject.SetActive(true);
                    slotRT.gameObject.name = $"Item Slot [{(_slotCountPerLine * j) + i}]";

                    _slotUIList.Add(slotRT.GetComponent<ItemSlotUI>());

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
        *                               Editor Preview
        ***********************************************************************/
        #region .
#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private bool _showPreview = false;

        [Range(0.01f, 1f)]
        [SerializeField] private float _previewAlpha = 0.1f;

        private List<GameObject> _previewSlotGoList = new List<GameObject>();
        private int _prevSlotCountPerLine;
        private int _prevSlotLineCount;
        private float _prevSlotSize;
        private float _prevSlotMargin;
        private float _prevContentPadding;
        private float _prevAlpha;
        private bool _prevShow = false;

        private void OnValidate()
        {
            if(Application.isPlaying) return;

            if (_showPreview && !_prevShow)
            {
                CreateSlots();
            }
            _prevShow = _showPreview;

            if (Unavailable())
            {
                ClearAll();
                return;
            }
            if (CountChanged())
            {
                ClearAll();
                CreateSlots();
                _prevSlotCountPerLine = _slotCountPerLine;
                _prevSlotLineCount = _slotLineCount;
            }
            if (ValueChanged())
            {
                DrawGrid();
                _prevSlotSize = _slotSize;
                _prevSlotMargin = _slotMargin;
                _prevContentPadding = _contentAreaPadding;
            }
            if (AlphaChanged())
            {
                SetImageAlpha();
                _prevAlpha = _previewAlpha;
            }

            bool Unavailable()
            {
                return !_showPreview ||
                        _slotCountPerLine < 1 ||
                        _slotLineCount < 1 ||
                        _slotSize <= 0f ||
                        _contentAreaRT == null ||
                        _slotUiPrefab == null;
            }
            bool CountChanged()
            {
                return _slotCountPerLine != _prevSlotCountPerLine ||
                       _slotLineCount != _prevSlotLineCount;
            }
            bool ValueChanged()
            {
                return _slotSize != _prevSlotSize ||
                       _slotMargin != _prevSlotMargin ||
                       _contentAreaPadding != _prevContentPadding;
            }
            bool AlphaChanged()
            {
                return _previewAlpha != _prevAlpha;
            }
            void ClearAll()
            {
                foreach (var go in _previewSlotGoList)
                {
                    Destroyer.Destroy(go);
                }
                _previewSlotGoList.Clear();
            }
            void CreateSlots()
            {
                int count = _slotCountPerLine * _slotLineCount;
                _previewSlotGoList.Capacity = count;

                for (int i = 0; i < count; i++)
                {
                    GameObject slotGo = Instantiate(_slotUiPrefab);
                    slotGo.transform.SetParent(_contentAreaRT.transform);
                    slotGo.SetActive(true);
                    slotGo.AddComponent<PreviewItemSlot>();

                    HideGameObject(slotGo);

                    _previewSlotGoList.Add(slotGo);
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
                        GameObject slotGo = _previewSlotGoList[index++];
                        RectTransform slotRT = slotGo.GetComponent<RectTransform>();

                        slotRT.anchoredPosition = curPos;
                        slotRT.sizeDelta = new Vector2(_slotSize, _slotSize);
                        _previewSlotGoList.Add(slotGo);

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
                foreach (var go in _previewSlotGoList)
                {
                    var images = go.GetComponentsInChildren<Image>();
                    foreach (var img in images)
                    {
                        img.color = new Color(img.color.r, img.color.g, img.color.b, _previewAlpha);
                        var outline = img.GetComponent<Outline>();
                        if (outline)
                            outline.effectColor = new Color(outline.effectColor.r, outline.effectColor.g, outline.effectColor.b, _previewAlpha);
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