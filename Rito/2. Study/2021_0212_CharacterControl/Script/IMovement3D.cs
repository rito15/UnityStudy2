using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-02-26 PM 5:32:53
// 작성자 : Rito

namespace Rito.CharacterControl
{
    public interface IMovement3D
    {
        /// <summary> 현재 이동 중인지 여부 </summary>
        bool IsMoving();
        /// <summary> 지면에 닿아 있는지 여부 </summary>
        bool IsGrounded();
        /// <summary> 지면으로부터의 거리 </summary>
        float GetDistanceFromGround();

        /// <summary> 월드 이동벡터 초기화(이동 명령) </summary>
        void SetMovement(in Vector3 worldMoveDirection, bool isRunning);
        /// <summary> 점프 명령 - 점프 성공 여부 리턴 </summary>
        bool SetJump();
        /// <summary> 이동 중지 </summary>
        void StopMoving();

        /// <summary> 밀쳐내기 </summary>
        void KnockBack(in Vector3 force, float time);
    }
}