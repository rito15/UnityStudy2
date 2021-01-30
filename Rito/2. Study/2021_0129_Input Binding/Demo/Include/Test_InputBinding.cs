using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-01-29 PM 6:18:55
// 작성자 : Rito

namespace Rito.InputBindings
{
    public class Test_InputBinding : MonoBehaviour
    {
        public InputBinding _binding = new InputBinding()
        {
            localDirectoryPath = @"Rito\2. Study\2021_0129_Input Binding\Presets",
            fileName = "InputBindingPreset",
            extName = "txt",
            id = "001"
        };

        private void Start()
        {
            _binding.SetBinding(UserAction.Attack, KeyCode.Q);
            _binding.SetBinding(UserAction.Attack, KeyCode.E, true);
            Debug.Log(_binding);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_binding.Bindings[UserAction.UI_Inventory]))
            {
                Debug.Log("Keydown : UI_Inventory");
            }
            if (Input.GetKeyDown(_binding.Bindings[UserAction.Jump]))
            {
                Debug.Log("Keydown : Jump");
            }
            if (Input.GetKeyDown(_binding.Bindings[UserAction.MoveForward]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Bindings[UserAction.MoveBackward]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Bindings[UserAction.MoveLeft]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Bindings[UserAction.MoveRight]))
            {
                Debug.Log("Keydown : MoveForward");
            }

            if (Input.GetKeyDown(_binding.Bindings[UserAction.Attack]))
            {
                Debug.Log("Keydown : Attack");
            }
        }
    }
}