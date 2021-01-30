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
        public BindPair[] bindPairs;

        public SerializableInputBinding(InputBinding binding)
        {
            int len = binding.Bindings.Count;
            int index = 0;

            bindPairs = new BindPair[len];

            foreach (var pair in binding.Bindings)
            {
                bindPairs[index++] =
                    new BindPair(pair.Key, pair.Value);
            }
        }
    }

    [Serializable]
    public class BindPair
    {
        public UserAction key;
        public KeyCode value;

        public BindPair(UserAction key, KeyCode value)
        {
            this.key = key;
            this.value = value;
        }
    }
}