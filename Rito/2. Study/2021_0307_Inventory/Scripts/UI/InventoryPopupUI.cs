using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-13 PM 7:47:35
// 작성자 : Rito

namespace Rito
{
    /// <summary> 인벤토리 UI 위에 띄울 작은 팝업들 관리 </summary>
    public class InventoryPopupUI : MonoBehaviour
    {
        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .
        // 확인창(확인/취소) 관련 필드
        [Header("Confirmation Popup")]
        [SerializeField] private GameObject _confirmationPopupObject;
        [SerializeField] private Text _confirmationText;
        [SerializeField] private Button _confirmationOkButton;     // Ok
        [SerializeField] private Button _confirmationCancelButton; // Cancel

        // 수량 입력 팝업
        [Header("Amount Input Popup")]
        [SerializeField] private GameObject _amountInputPopupObject;
        [SerializeField] private InputField _amountInputField;
        [SerializeField] private Button _amountPlusButton;        // +
        [SerializeField] private Button _amountMinusButton;       // -
        [SerializeField] private Button _amountInputOkButton;     // Ok
        [SerializeField] private Button _amountInputCancelButton; // Cancel

        // 확인 버튼 눌렀을 때 동작할 이벤트
        private event Action OnConfirmationOK;
        private event Action<int> OnAmountInputOK;

        private int _maxAmount;

        #endregion
        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .
        private void Awake()
        {
            InitUIEvents();
            HidePanel();
            HideConfirmationPopup();
            HideAmountInputPopup();
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 확인/취소 팝업 띄우기 </summary>
        public void OpenConfirmationPopup(Action okCallback)
        {
            ShowPanel();
            ShowConfirmationPopup();
            SetConfirmationOKEvent(okCallback);
        }
        /// <summary> 수량 입력 팝업 띄우기 </summary>
        public void OpenAmountInputPopup(Action<int> okCallback, int currentAmount)
        {
            _maxAmount = currentAmount - 1;
            _amountInputField.text = "1";

            ShowPanel();
            ShowAmountInputPopup();
            SetAmountInputOKEvent(okCallback);
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        private void InitUIEvents()
        {
            // 1. 확인 취소 팝업
            _confirmationOkButton.onClick.AddListener(HidePanel);
            _confirmationOkButton.onClick.AddListener(HideConfirmationPopup);
            _confirmationOkButton.onClick.AddListener(() => OnConfirmationOK?.Invoke());

            _confirmationCancelButton.onClick.AddListener(HidePanel);
            _confirmationCancelButton.onClick.AddListener(HideConfirmationPopup);

            // 2. 수량 입력 팝업
            _amountInputOkButton.onClick.AddListener(HidePanel);
            _amountInputOkButton.onClick.AddListener(HideAmountInputPopup);
            _amountInputOkButton.onClick.AddListener(() => OnAmountInputOK?.Invoke(int.Parse(_amountInputField.text)));

            _amountInputCancelButton.onClick.AddListener(HidePanel);
            _amountInputCancelButton.onClick.AddListener(HideAmountInputPopup);

            // [-], [+] 버튼 이벤트
            _amountMinusButton.onClick.AddListener(() =>
            {
                int amount = int.Parse(_amountInputField.text);
                if (amount - 1 > 0)
                    _amountInputField.text = (amount - 1).ToString();
            });

            _amountPlusButton.onClick.AddListener(() =>
            {
                int amount = int.Parse(_amountInputField.text);
                if(amount + 1 < _maxAmount)
                    _amountInputField.text = (amount + 1).ToString();
            });

            // 입력 값 범위 제한
            _amountInputField.onValueChanged.AddListener(str =>
            {
                int amount = int.Parse(str);
                bool flag = false;

                if (amount < 1)
                {
                    flag = true;
                    amount = 1;
                }
                else if (amount > _maxAmount)
                {
                    flag = true;
                    amount = _maxAmount;
                }

                if(flag)
                    _amountInputField.text = amount.ToString();
            });
        }

        private void ShowPanel() => gameObject.SetActive(true);
        private void HidePanel() => gameObject.SetActive(false);

        private void ShowConfirmationPopup() => _confirmationPopupObject.SetActive(true);
        private void HideConfirmationPopup() => _confirmationPopupObject.SetActive(false);

        private void ShowAmountInputPopup() => _amountInputPopupObject.SetActive(true);
        private void HideAmountInputPopup() => _amountInputPopupObject.SetActive(false);

        private void SetConfirmationOKEvent(Action handler) => OnConfirmationOK = handler;
        private void SetAmountInputOKEvent(Action<int> handler) => OnAmountInputOK = handler;


        #endregion

    }
}