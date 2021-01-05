using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /******************************************************************
     *                        Condition 필드 모음
     ******************************************************************/
    public partial class CharacterCore : MonoBehaviour, ICore
    {
        Func<bool> WasdInput = new Func<bool>(() =>
        {
            bool check =
                    Input.GetKey(KeyCode.W) ||
                    Input.GetKey(KeyCode.A) ||
                    Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.D);
            Debug.Log($"Condition : Wasd Input ({check})");

            return check;
        });

        Func<bool> MouseInput = new Func<bool>(() =>
        {
            bool check = Input.GetMouseButton(1);
            Debug.Log($"Condition : Mouse Input ({check})");

            return check;
        });
    }
}