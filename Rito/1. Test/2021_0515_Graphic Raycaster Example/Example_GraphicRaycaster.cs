using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-05-15 PM 6:42:15
// 작성자 : Rito

namespace Rito.Tests.GR
{
    public class Example_GraphicRaycaster : MonoBehaviour
    {
        private GraphicRaycaster gr;
        private PointerEventData ped;
        private List<RaycastResult> rrList;

        [SerializeField] private bool gridMove; // 격자 이동 여부
        [Range(1f, 100f)]
        [SerializeField] private float gridUnit = 20f; // 격자 이동 단위
        [SerializeField] private Icon currentIcon; // 현재 마우스 커서가 위치한 곳의 아이콘
        [SerializeField] private Icon dragIcon;    // 드래그 대상 아이콘

        private Transform dragIconTransform;
        private Vector3 beginDragIconPosition;  // 드래그 시작 시 아이콘의 위치
        private Vector3 beginDragMousePosition; // 드래그 시작 시 마우스 커서 위치
        private Vector3 dragOffset;

        private void Awake()
        {
            gr = GetComponentInParent<GraphicRaycaster>();
            ped = new PointerEventData(EventSystem.current);
            rrList = new List<RaycastResult>(4);
        }

        private void Update()
        {
            ped.position = Input.mousePosition;

            OnPointerEnterAndExit();
            OnPointerDown();
            OnPointerDrag();
            OnPointerUp();
        }

        private void OnPointerEnterAndExit()
        {
            // 드래그 상태일 경우 동작 X
            if(dragIcon != null) return;

            var prevIcon = currentIcon;      // 이전 프레임의 아이콘
            currentIcon = RaycastUI<Icon>(); // 현재 프레임의 아이콘

            if (prevIcon == null)
            {
                // 1. Enter
                if (currentIcon != null)
                    OnCurrentEnter();
            }
            else
            {
                // 2. Exit
                if (currentIcon == null)
                    OnPrevExit();

                // 3. Change
                else if (prevIcon != currentIcon)
                {
                    OnPrevExit();
                    OnCurrentEnter();
                }
            }

            // ==================== Local Methods ===================
            void OnPrevExit()
            {
                prevIcon.Focus(false);
            }
            void OnCurrentEnter()
            {
                currentIcon.Focus(true);
            }
        }

        /// <summary> 마우스를 누르는 순간 </summary>
        private void OnPointerDown()
        {
            if (currentIcon == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                // 클릭 이벤트
                currentIcon.Down();
                currentIcon.SetOnTop(true);

                // 드래그 설정
                dragIcon = currentIcon;
                dragIconTransform = dragIcon.transform;

                // 드래그 - 시작 위치 기록
                beginDragIconPosition = dragIconTransform.position;
                beginDragMousePosition = Input.mousePosition;
            }
        }

        /// <summary> 마우스 클릭을 유지할 경우 </summary>
        private void OnPointerDrag()
        {
            if(dragIcon == null) return;

            if (Input.GetMouseButton(0))
            {
                dragOffset = Input.mousePosition - beginDragMousePosition;

                // 미동도 하지 않은 경우
                if(dragOffset.sqrMagnitude < 0.1f)
                    return;

                // 일반 이동
                if (!gridMove)
                    dragIconTransform.position = beginDragIconPosition + dragOffset;

                // 격자 이동
                else
                {
                    Vector3 nextPos = beginDragIconPosition + dragOffset;
                    nextPos.x = Mathf.Round(nextPos.x / gridUnit) * gridUnit;
                    nextPos.y = Mathf.Round(nextPos.y / gridUnit) * gridUnit;

                    dragIconTransform.position = nextPos;
                }
            }
        }

        /// <summary> 마우스를 뗄 경우 </summary>
        private void OnPointerUp()
        {
            if (currentIcon == null) return;

            if (Input.GetMouseButtonUp(0))
            {
                currentIcon.Up();
                dragIcon = null;
            }
        }

        /// <summary> UI에 레이캐스트하여 가장 위에 있는 대상 가져오기 </summary>
        private T RaycastUI<T>() where T : Component
        {
            rrList.Clear();
            gr.Raycast(ped, rrList);

            if(rrList.Count == 0)
                return null;

            return rrList[0].gameObject.GetComponent<T>();
        }
    }
}