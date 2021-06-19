using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-06-17 PM 9:10:15
// 작성자 : Rito

namespace Rito.StrategyPattern.Old
{
    /// <summary> 플레이어 캐릭터(컨텍스트 역할) </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        private const float GrapplingDistance = 1f;
        private const float PunchDistance = 3f;
        private const float DashDistance = 10f;

        // 현재 타겟으로 설정된 적
        private Enemy targetEnemy;

        private void Update()
        {
            if (targetEnemy == null)
            {
                FindEnemy();
            }
            else
            {
                // 적으로부터의 거리 계산
                float distanceFromEnemy = GetDistanceFromEnemy();

                // 거리에 따른 공격 전략 선택
                if (distanceFromEnemy <= GrapplingDistance)
                {
                    GrapplingAttack();
                }
                else if (distanceFromEnemy <= PunchDistance)
                {
                    PunchAttack();
                }
                else if (distanceFromEnemy <= DashDistance)
                {
                    DashAttack();
                }
            }
        }

        public void FindEnemy()
        {
            // 적을 찾아 targetEnemy 필드에 초기화
        }

        // 적으로부터의 거리 계산
        public float GetDistanceFromEnemy()
        {
            return Vector3.Distance(targetEnemy.transform.position, transform.position);
        }

        public void GrapplingAttack()
        {
            Debug.Log("잡기 공격");
        }

        public void PunchAttack()
        {
            Debug.Log("펀치 공격");
        }

        public void DashAttack()
        {
            Debug.Log("돌진 공격");
        }
    }

    /// <summary> 적 </summary>
    public class Enemy : MonoBehaviour { }
}