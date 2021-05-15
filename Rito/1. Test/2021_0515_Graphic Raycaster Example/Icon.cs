using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-05-15 PM 7:49:11
// 작성자 : Rito

namespace Rito.Tests.GR
{
    public class Icon : MonoBehaviour
    {
        private Image image;
        private int siblingIndex;

        private static readonly Color DefaultColor = Color.white;
        private static readonly Color FocusedColor = Color.red;
        private static readonly Color DownColor = Color.green;

        private void Awake()
        {
            TryGetComponent(out image);
        }

        public void SetOnTop(bool isTrue)
        {
            // 맨 위에 보이기
            if (isTrue)
            {
                siblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
            }
            // 원상복귀
            else
            {
                transform.SetSiblingIndex(siblingIndex);
            }
        }

        public void Focus(bool isFocused)
        {
            if(isFocused)
                image.color = FocusedColor;
            else
                image.color = DefaultColor;
        }

        public void Down()
        {
            image.color = DownColor;
        }

        public void Up()
        {
            image.color = FocusedColor;
        }
    }
}