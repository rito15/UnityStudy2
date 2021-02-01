using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.UI;
using UnityEngine.EventSystems;

// 날짜 : 2021-01-31 PM 9:44:58
// 작성자 : Rito

namespace Rito.PopupUIManagement
{
    public class PopupUI : MonoBehaviour, IPointerDownHandler
    {
        public Button _closeButton;
        public event Action OnFocus;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            OnFocus();
        }
    }
}