using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-29 PM 5:26:41
// 작성자 : Rito

namespace Rito.InputBindings
{
    /// <summary> 사용자 입력에 의한 행동 정의 </summary>
    public enum UserAction
    {
        Attack,

        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,

        Run,
        Jump,

        // UI
        UI_Inventory,
        UI_Status,
        UI_Skill,
    }
}