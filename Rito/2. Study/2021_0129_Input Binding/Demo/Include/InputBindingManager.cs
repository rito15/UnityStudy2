using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;

// 날짜 : 2021-01-30 PM 7:58:41
// 작성자 : Rito

namespace Rito.InputBindings
{
    public class InputBindingManager : MonoBehaviour
    {
        /***********************************************************************
        *                               Enum Definition
        ***********************************************************************/
        #region .
        private enum InputListening
        {
            None,
            Mouse,
            Keyboard
        }

        #endregion

        /***********************************************************************
        *                               Fields
        ***********************************************************************/
        #region .
        public InputBinding _binding = new InputBinding(false)
        {
            localDirectoryPath = @"Rito\2. Study\2021_0129_Input Binding\Presets",
            fileName = "BindingPreset",
            extName = "txt",
            id = "1"
        };

        public Button[] _presetButtons;
        public Button _saveButton;

        public GameObject _waitingInputGo;
        public Transform _verticalLayoutTr;
        public GameObject _bindingPairPrefab;

        private List<GameObject> _bindingPairGoList;
        private Dictionary<UserAction, BindingPairUI> _bindingPairDict;

        private bool _isListening;
        private UserAction _curKeyAction;

        #endregion
        /***********************************************************************
        *                               Unity Callbacks
        ***********************************************************************/
        #region .
        private void Start()
        {
            Init();
            InitButtonListeners();

            LoadPreset();
            LoadInputBindings();
        }

        private void Update()
        {
            if (_isListening)
            {
                if (ListenInput(out var keyCode))
                {
                    SetKeyBinding(_curKeyAction, keyCode);
                    _isListening = false;
                }
            }

            _waitingInputGo.SetActive(_isListening);
        }
        #endregion
        /***********************************************************************
        *                               Update Methods
        ***********************************************************************/
        #region .

        private bool ListenInput(out KeyCode code)
        {
            IEnumerable<KeyCode> keyCodeList =
                Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

            foreach (var curCode in keyCodeList)
            {
                if (Input.GetKeyDown(curCode))
                {
                    code = curCode;
                    return true;
                }
            }

            code = KeyCode.None;
            return false;
        }

        #endregion
        /***********************************************************************
        *                               Init Methods
        ***********************************************************************/
        #region .
        private void Init()
        {
            _isListening = false;
            _waitingInputGo.SetActive(false);

            _bindingPairGoList = new List<GameObject>();
            _bindingPairDict = new Dictionary<UserAction, BindingPairUI>();
        }

        private void InitButtonListeners()
        {
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                Button curButton = _presetButtons[i];

                int index = i + 1;
                curButton.onClick.AddListener(() =>
                {
                    ResetAllPresetButtons();
                    curButton.TryGetComponent(out Image image);
                    if (image)
                    {
                        image.color = Color.green;
                    }

                    _binding.id = $"{index}";
                    LoadPreset();
                    LoadInputBindings();
                });
            }

            _saveButton.onClick.AddListener(() =>
            {
                _binding.SaveToFile();
            });
        }

        #endregion
        /***********************************************************************
        *                               Methods
        ***********************************************************************/
        #region .
        private void LoadPreset()
        {
            if (_binding.LoadFromFile() == false)
            {
                _binding.ResetAll();
                _binding.SaveToFile();
            }
        }

        private void LoadInputBindings()
        {
            int count = 0;

            // 1. Reset
            foreach (var go in _bindingPairGoList)
            {
                Destroy(go);
            }
            _bindingPairDict.Clear();
            _bindingPairGoList.Clear();


            // 2. Load Pairs
            foreach (var pair in _binding.Bindings)
            {
                var pairGo = Instantiate(_bindingPairPrefab, _verticalLayoutTr);
                var pairUI = pairGo.GetComponent<BindingPairUI>();

                pairUI.InitLabels($"{pair.Key}", $"{pair.Value}");
                pairUI.AddButtonListener(() =>
                {
                    pairUI.Select();
                    _isListening = true;
                    _curKeyAction = pair.Key;
                });

                _bindingPairDict.Add(pair.Key, pairUI);
                _bindingPairGoList.Add(pairGo);
                count++;
            }

            // Resize Vertical Layout Height
            _verticalLayoutTr.TryGetComponent(out RectTransform rt);
            if (rt)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x,
                    40 +
                    count * 60 +
                    (count - 1) * 10
                );
            }
        }

        private void SetKeyBinding(UserAction action, KeyCode code)
        {
            _binding.Bind(action, code);
            RefreshAllBindingUIs();
        }

        private void RefreshAllBindingUIs()
        {
            foreach (var pair in _binding.Bindings)
            {
                _bindingPairDict[pair.Key].SetCodeLabel($"{pair.Value}");
                _bindingPairDict[pair.Key].Deselect();
            }
        }

        private void ResetAllPresetButtons()
        {
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].TryGetComponent(out Image image);
                if (image)
                {
                    image.color = Color.white;
                }
            }
        }

        #endregion
    }
}