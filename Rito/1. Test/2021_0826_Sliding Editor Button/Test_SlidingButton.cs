using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 날짜 : 2021-08-26 AM 4:31:03
// 작성자 : Rito

namespace Rito
{
    /// <summary> 
    /// 
    /// </summary>
    public class Test_SlidingButton : MonoBehaviour
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(Test_SlidingButton))]
        private class CE : UnityEditor.Editor
        {
            private Test_SlidingButton m;

            private void OnEnable()
            {
                m = target as Test_SlidingButton;
            }

            public override void OnInspectorGUI()
            {
                EditorGUILayout.Space(40f);
                DrawMovingButton();
                EditorGUILayout.Space(40f);
            }

            bool isMoving = false;
            bool isOn = true;
            float onOffPos = 0f;
            string strOnOff = "On";
            private void DrawMovingButton()
            {
                const float LEFT = 200f;
                const float RIGHT = 36f + LEFT;
                const float WIDTH = 40f;
                const float HEIGHT = 20f;
                const float MOVE_SPEED = 1f;

                Rect rect = GUILayoutUtility.GetRect(1f, HEIGHT);

                Rect bgRect = new Rect(rect);
                bgRect.x = LEFT + 1f;
                bgRect.xMax = RIGHT + WIDTH - 2f;
                EditorGUI.DrawRect(bgRect, new Color(0.15f, 0.15f, 0.15f));

                rect.width = WIDTH;
                rect.x = onOffPos;

                Color col = GUI.backgroundColor;
                GUI.backgroundColor = Color.black;

                if (GUI.Button(rect, strOnOff))
                {
                    isMoving = true;
                }

                if (!isMoving)
                {
                    if (isOn)
                    {
                        onOffPos = LEFT;
                        strOnOff = "On";
                    }
                    else
                    {
                        onOffPos = RIGHT;
                        strOnOff = "Off";
                    }
                }
                else
                {
                    if (isOn)
                    {
                        if (onOffPos < RIGHT)
                        {
                            onOffPos += MOVE_SPEED;
                            Repaint();

                            if (onOffPos >= RIGHT)
                            {
                                isMoving = false;
                                isOn = false;
                            }
                        }
                    }
                    else
                    {
                        if (onOffPos > LEFT)
                        {
                            onOffPos -= MOVE_SPEED;
                            Repaint();

                            if (onOffPos <= LEFT)
                            {
                                isMoving = false;
                                isOn = true;
                            }
                        }
                    }
                }

                GUI.backgroundColor = col;
            }
        }
#endif
    }
}