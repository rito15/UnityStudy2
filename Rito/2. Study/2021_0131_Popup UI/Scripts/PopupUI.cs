using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.UI;

// 날짜 : 2021-01-31 PM 9:44:58
// 작성자 : Rito

namespace Rito.PopupUIManagement
{
    public class PopupUI : MonoBehaviour
    {
        /***********************************************************************
        *                          Public Fields, Properties
        ***********************************************************************/
        #region .
        public RectTransform _headerRect;
        public Button _closeButton;

        public bool IsFocused { get; private set; }

        #endregion
        /***********************************************************************
        *                          Private Fields
        ***********************************************************************/
        #region .
        private RectTransform _rect;
        private SortingGroup _sortGroup;

        

        #endregion
        /***********************************************************************
        *                          Unity Callbacks
        ***********************************************************************/
        #region .
        private void Awake()
        {
            TryGetComponent(out _rect);
            _sortGroup = gameObject.AddComponent<SortingGroup>();
        }

        

        #endregion
        /***********************************************************************
        *                          Public Methods
        ***********************************************************************/
        #region .
        public void Focus() => IsFocused = true;
        public void Release() => IsFocused = false;

        public void SetSortingOrder(in int order)
            => _sortGroup.sortingOrder = order;

        public bool IsFocusedOnHeader()
        {
            return true;/////
        }

        public void Move(Vector2 offset)
        {

        }

        #endregion
        /***********************************************************************
        *                          Private Methods
        ***********************************************************************/
        #region .


        #endregion
    }
}