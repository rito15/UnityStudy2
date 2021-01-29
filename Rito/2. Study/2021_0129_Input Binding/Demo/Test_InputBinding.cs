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
        public InputBinding _binding = new InputBinding();

        private void Start()
        {
            _binding.SetBinding(UserMouseAction.Attack, MouseCode.Middle);
            _binding.SetBinding(UserMouseAction.Move, MouseCode.Middle, true);
            Debug.Log(_binding);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.UI_Inventory]))
            {
                Debug.Log("Keydown : UI_Inventory");
            }
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.Jump]))
            {
                Debug.Log("Keydown : Jump");
            }
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.MoveForward]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.MoveBackward]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.MoveLeft]))
            {
                Debug.Log("Keydown : MoveForward");
            }
            if (Input.GetKeyDown(_binding.Keyboard[UserKeyAction.MoveRight]))
            {
                Debug.Log("Keydown : MoveForward");
            }

            if (Input.GetMouseButtonDown(_binding.Mouse[UserMouseAction.Attack]))
            {
                Debug.Log("Mousedown : Attack");
            }
            if (Input.GetMouseButtonDown(_binding.Mouse[UserMouseAction.Move]))
            {
                Debug.Log("Mousedown : Move");
            }
        }
    }
}