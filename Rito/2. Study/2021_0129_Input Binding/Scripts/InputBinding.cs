using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/*
  * 날짜 : 2021-01-29 PM 5:25:39
  * 작성자 : Rito

  * 설명 : 게임 내 키보드, 마우스 바인딩 설정 객체

  * 프로퍼티
    - Mouse : 딕셔너리를 통해 사용자 마우스 액션과 마우스 버튼 인덱스(0, 1, 2) 연결
    - Key : 딕셔너리를 통해 사용자 키보드 액션과 KeyCode 연결

  * 메소드
    - ResetAll() : 키보드, 마우스 바인딩을 초기 설정 상태로 되돌리기
    - SetBinding() : 새로운 바인딩 페어 등록 또는 기존 바인딩 변경
    - Save() : 필드를 통해 지정한 경로에 바인딩 설정 저장
    - Load() : 지정한 경로로부터 바인딩 설정 로드
*/
namespace Rito.InputBindings
{
    /// <summary> 유저 키, 마우스 바인딩 옵션 </summary>
    [Serializable]
    public class InputBinding
    {
        // 저장, 불러오기 시 폴더명, 파일명, 확장자, 고유번호
        public string localDirectoryPath = "Settings"; // "Assets/Settings"
        public string fileName = "InputBindingPreset";
        public string extName = "txt";
        public string id = "001";

        public Dictionary<UserMouseAction, int> Mouse => _mouse;
        public Dictionary<UserKeyAction, KeyCode> Keyboard => _keyboard;

        private Dictionary<UserMouseAction, int> _mouse;
        private Dictionary<UserKeyAction, KeyCode> _keyboard;

        public bool showDebug = false;

        /***********************************************************************
        *                               Constructors
        ***********************************************************************/
        #region .
        public InputBinding(bool initalize = true)
        {
            _mouse = new Dictionary<UserMouseAction, int>();
            _keyboard = new Dictionary<UserKeyAction, KeyCode>();

            if (initalize)
            {
                ResetAll();
            }
        }

        public InputBinding(SerializableInputBinding sib)
        {
            _mouse = new Dictionary<UserMouseAction, int>();
            _keyboard = new Dictionary<UserKeyAction, KeyCode>();

            foreach (var pair in sib.mouseBindPairs)
            {
                _mouse[pair.key] = pair.value;
            }

            foreach (var pair in sib.keyBindPairs)
            {
                _keyboard[pair.key] = pair.value;
            }
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DebugLog(object msg)
        {
            if (!showDebug) return;
            UnityEngine.Debug.Log(msg);
        }

        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .
        /// <summary> 새로운 바인딩 설정 적용 </summary>
        public void ApplyNewBindings(InputBinding newBinding)
        {
            _mouse = new Dictionary<UserMouseAction, int>(newBinding._mouse);
            _keyboard = new Dictionary<UserKeyAction, KeyCode>(newBinding._keyboard);
        }
        /// <summary> 새로운 바인딩 설정 적용 </summary>
        public void ApplyNewBindings(SerializableInputBinding newBinding)
        {
            _mouse.Clear();
            _keyboard.Clear();

            foreach (var pair in newBinding.mouseBindPairs)
            {
                _mouse[pair.key] = pair.value;
            }

            foreach (var pair in newBinding.keyBindPairs)
            {
                _keyboard[pair.key] = pair.value;
            }
        }

        /// <summary> 초기 상태로 설정 </summary>
        public void ResetAll()
        {
            SetBinding(UserMouseAction.Attack, MouseCode.Left);
            SetBinding(UserMouseAction.Move, MouseCode.Right);

            SetBinding(UserKeyAction.MoveForward, KeyCode.W);
            SetBinding(UserKeyAction.MoveBackward, KeyCode.S);
            SetBinding(UserKeyAction.MoveLeft, KeyCode.A);
            SetBinding(UserKeyAction.MoveRight, KeyCode.D);

            SetBinding(UserKeyAction.Run, KeyCode.LeftControl);
            SetBinding(UserKeyAction.Jump, KeyCode.Space);

            SetBinding(UserKeyAction.UI_Inventory, KeyCode.I);
            SetBinding(UserKeyAction.UI_Status, KeyCode.P);
            SetBinding(UserKeyAction.UI_Skill, KeyCode.K);
        }

        /// <summary> 바인딩 등록 또는 변경
        /// <para/> - allowOverlap이 false이면 기존 해당 키에 연결된 동작의 키가 존재할 경우 none으로 설정
        /// <para/> - allowOverlap이 true이면 바인딩이 중복되도록 유지
        /// </summary>
        public void SetBinding(in UserMouseAction mAction, in MouseCode mCode, bool allowOverlap = false)
        {
            int mIntCode = (int)mCode;

            if (!allowOverlap && _mouse.ContainsValue(mIntCode))
            {
                var copy = new Dictionary<UserMouseAction, int>(_mouse);

                foreach (var pair in copy)
                {
                    if (pair.Value.Equals(mCode))
                    {
                        _mouse[pair.Key] = -1;
#if UNITY_EDITOR
                        DebugLog($"기존 마우스 바인딩 제거 : [{pair.Key} - {pair.Value}]");
#endif
                    }
                }
            }

#if UNITY_EDITOR
            if (_mouse.ContainsKey(mAction))
            {
                MouseCode prevCode = (MouseCode)_mouse[mAction];
                DebugLog($"마우스 바인딩 변경 : [{mAction}] {prevCode} -> {mCode}");
            }
            else
            {
                DebugLog($"마우스 바인딩 등록 : [{mAction} - {mCode}]");
            }
#endif

            _mouse[mAction] = mIntCode;
        }

        /// <summary> 바인딩 등록 또는 변경
        /// <para/> - allowOverlap이 false이면 기존 해당 키에 연결된 동작의 키가 존재할 경우 none으로 설정
        /// <para/> - allowOverlap이 true이면 바인딩이 중복되도록 유지
        /// </summary>
        public void SetBinding(in UserKeyAction mAction, in KeyCode mCode, bool allowOverlap = false)
        {
            if (!allowOverlap && _keyboard.ContainsValue(mCode))
            {
                var copy = new Dictionary<UserKeyAction, KeyCode>(_keyboard);

                foreach (var pair in copy)
                {
                    if (pair.Value.Equals(mCode))
                    {
                        _keyboard[pair.Key] = KeyCode.None;
#if UNITY_EDITOR
                        DebugLog($"기존 키보드 바인딩 제거 : [{pair.Key} - {pair.Value}]");
#endif
                    }
                }
            }

#if UNITY_EDITOR
            if (_keyboard.ContainsKey(mAction))
            {
                KeyCode prevCode = _keyboard[mAction];
                DebugLog($"키보드 바인딩 변경 : [{mAction}] {prevCode} -> {mCode}");
            }
            else
            {
                DebugLog($"키보드 바인딩 등록 : [{mAction} - {mCode}]");
            }
#endif

            _keyboard[mAction] = mCode;
        }

        public override string ToString()
        {
            StringBuilder Sb = new StringBuilder("");

            Sb.Append("1. Mouse \n");
            foreach (var pair in _mouse)
            {
                Sb.Append($"{pair.Key} : {(MouseCode)pair.Value}\n");
            }

            Sb.Append("\n2. Keyboard \n");
            foreach (var pair in _keyboard)
            {
                Sb.Append($"{pair.Key} : {pair.Value}\n");
            }

            return Sb.ToString();
        }

        #endregion
        /***********************************************************************
        *                               File IO Methods
        ***********************************************************************/
        #region .
        /// <summary> 파일에 저장 </summary>
        public void SaveToFile()
        {
            SerializableInputBinding sib = new SerializableInputBinding(this);
            string jsonStr = JsonUtility.ToJson(sib);

            if (jsonStr.Length < 3)
            {
                Debug.Log("JSON Serialization Error");
                return;
            }

            DebugLog($"Save : Assets/{localDirectoryPath}/{fileName}_{id}.{extName}\n\n{this}");
            LocalFileIOHandler.Save(jsonStr, localDirectoryPath, $"{fileName}_{id}", extName);
        }

        public void LoadFromFile()
        {
            string jsonStr = LocalFileIOHandler.Load(localDirectoryPath, $"{fileName}_{id}", extName);

            if (jsonStr == null)
            {
                Debug.Log("File Load Error");
                return;
            }

            var sib = JsonUtility.FromJson<SerializableInputBinding>(jsonStr);
            ApplyNewBindings(sib);

            DebugLog($"Load : Assets/{localDirectoryPath}/{fileName}_{id}.{extName}\n\n{this}");
        }

        #endregion
    }
}