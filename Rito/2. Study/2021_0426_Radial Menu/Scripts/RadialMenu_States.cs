using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-26 PM 5:23:19
// 작성자 : Rito

namespace Rito
{
    public partial class RadialMenu : MonoBehaviour
    {
        /***********************************************************************
        *                               State Definitions
        ***********************************************************************/
        #region .
        private abstract class MenuState
        {
            protected readonly RadialMenu menu;

            public MenuState(RadialMenu menu)
                => this.menu = menu;

            public abstract void OnEnter();
            public abstract void Update();
            public abstract void OnExit();
        }

        private class MainState : MenuState
        {
            public MainState(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                for (int i = 0; i < menu._pieceCount; i++)
                {
                    menu._pieceImages[i].color = NotSelectedPieceColor;
                    menu._pieceRects[i].anchoredPosition = menu._pieceDirections[i] * menu._pieceDist;
                }
            }
            public override void Update()
            {
                bool showArrow = false;

                // 마우스의 스크린 내 좌표
                var mViewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                // 스크린의 중앙을 (0, 0)으로 하는 마우스 좌표
                var mPos = new Vector2(mViewportPos.x - 0.5f, mViewportPos.y - 0.5f);

                // 중심에서 마우스까지의 거리
                var mDist = new Vector2(mPos.x * Screen.width / Screen.height, mPos.y).magnitude;

                if (mDist < menu._centerDistThreshold)
                {
                    menu._selectedIndex = -1;
                }
                else
                {
                    ClockwisePolarCoord mousePC = ClockwisePolarCoord.FromVector2(mPos);

                    // Arrow 회전 설정
                    menu._arrowRotationZ = -mousePC.Angle;
                    showArrow = true;

                    // 각도로부터 배열 인덱스 계산
                    float fIndex = (mousePC.Angle / 360f) * menu._pieceCount;
                    menu._selectedIndex = Mathf.RoundToInt(fIndex) % menu._pieceCount;
                }

                menu.SetSelectedPieceColors();
                menu.SetArrowRotation(showArrow);
            }
            public override void OnExit()
            {
            }
        }

        #endregion

    }
}