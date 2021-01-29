using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-29 PM 5:30:00
// 작성자 : Rito

namespace Rito.InputBindings
{
    /// <summary> InputBinding의 JSON 직렬화 가능한 형태 </summary>
    [Serializable]
    public class SerializableInputBinding
    {
        [SerializeField]
        public KeyBindPair[] keyBindPairs;
        [SerializeField]
        public MouseBindPair[] mouseBindPairs;

        public SerializableInputBinding(InputBinding binding)
        {
            var kBind = binding.Keyboard;
            var mBind = binding.Mouse;

            int kLen = kBind.Count;
            int mLen = mBind.Count;

            int kIndex = 0;
            int mIndex = 0;

            keyBindPairs = new KeyBindPair[kLen];
            mouseBindPairs = new MouseBindPair[mLen];

            foreach (var pair in kBind)
            {
                keyBindPairs[kIndex++] =
                    new KeyBindPair(pair.Key, pair.Value);
            }

            foreach (var pair in mBind)
            {
                mouseBindPairs[mIndex++] =
                    new MouseBindPair(pair.Key, pair.Value);
            }
        }
    }

    [Serializable]
    public class KeyBindPair
    {
        public UserKeyAction key;
        public KeyCode value;

        public KeyBindPair(UserKeyAction key, KeyCode value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class MouseBindPair
    {
        public UserMouseAction key;
        public int value;

        public MouseBindPair(UserMouseAction key, int value)
        {
            this.key = key;
            this.value = value;
        }
    }
}