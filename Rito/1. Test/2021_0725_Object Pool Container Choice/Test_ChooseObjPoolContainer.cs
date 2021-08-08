using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// 날짜 : 2021-07-25 AM 1:22:23
// 작성자 : Rito

namespace Rito.Tests
{
    /// <summary> 
    /// 
    /// </summary>
    public class Test_ChooseObjPoolContainer : MonoBehaviour
    {
        public Transform _parentOfTarget;
        public GameObject _target;

        [Space]
        public InputField _countInputField;

        [Space]
        public Button _listButton;
        public Text   _listText;

        [Space]
        public Button _stackButton;
        public Text   _stackText;

        [Space]
        public Button _queueButton;
        public Text   _queueText;

        [Space]
        public Button _createButton;
        public Button _resetButton;

        [Space]
        public Text _createdTargetCountText;
        private GameObject[] _targets;

        private List<GameObject> _list = new List<GameObject>(400);
        private Stack<GameObject> _stack = new Stack<GameObject>(400);
        private Queue<GameObject> _queue = new Queue<GameObject>(400);

        [Space]
        public long _tick;
        public int _count;

        private void Awake()
        {
            InitInputFields();
            InitButtons();
        }

        private void InitInputFields()
        {
            int.TryParse(_countInputField.text, out _count);

            _countInputField.onEndEdit.AddListener(str =>
            {
                int.TryParse(_countInputField.text, out _count);
            });
        }

        private void InitButtons()
        {
            // List
            _listButton.onClick.AddListener(DoList);

            // Stack
            _stackButton.onClick.AddListener(DoStack);

            // Queue
            _queueButton.onClick.AddListener(DoQueue);

            // Create
            _createButton.onClick.AddListener(CreateTargets);

            // Reset
            _resetButton.onClick.AddListener(ResetAll);
        }

        private void DoList()
        {
            BeginRecord();

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].SetActive(false);
                _list.Add(_targets[i]);
            }
            for (int i = 0; i < _targets.Length; i++)
            {
                _list.Remove(_targets[i]);
                _targets[i].SetActive(true);
                _targets[i].transform.position = Vector3.up * 6f;
            }

            EndRecord(_listText);
        }

        private void DoStack()
        {
            BeginRecord();

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].SetActive(false);
                _stack.Push(_targets[i]);
            }
            for (int i = 0; i < _targets.Length; i++)
            {
                var current = _stack.Pop();
                current.SetActive(true);
                current.transform.position = Vector3.up * 6f;
            }

            EndRecord(_stackText);
        }

        private void DoQueue()
        {
            BeginRecord();

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].SetActive(false);
                _queue.Enqueue(_targets[i]);
            }
            for (int i = 0; i < _targets.Length; i++)
            {
                var current = _queue.Dequeue();
                current.SetActive(true);
                current.transform.position = Vector3.up * 6f;
            }

            EndRecord(_queueText);
        }

        private void CreateTargets()
        {
            // Destroy And Reset All
            if (_targets != null)
            {
                for (int i = 0; i < _targets.Length; i++)
                {
                    Destroy(_targets[i]);
                }
                _list.Clear();
                _stack.Clear();
                _queue.Clear();
            }

            // Create Targets
            _targets = new GameObject[_count];
            for (int i = 0; i < _count; i++)
            {
                _targets[i] = Instantiate(_target);
                _targets[i].SetActive(false);
            }

            _createdTargetCountText.text = _targets.Length.ToString();
        }

        private void ResetAll()
        {
            CreateTargets();

            _listText.text = "0000";
            _stackText.text = "0000";
            _queueText.text = "0000";
        }

        private void BeginRecord()
        {
            _tick = DateTime.Now.Ticks;
        }

        private void EndRecord(Text resultText)
        {
            _tick = DateTime.Now.Ticks - _tick;
            resultText.text = _tick.ToString();
        }
    }
}