using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-06-17 PM 9:10:15
// 작성자 : Rito

namespace Rito.StrategyPattern.New
{
    /// <summary> 플레이어 캐릭터(컨텍스트 역할) </summary>
    public class PlayerCharacter : MonoBehaviour
    {
        private const float GrapplingDistance = 1f;
        private const float PunchDistance = 3f;
        private const float DashDistance = 10f;

        // 현재 타겟으로 설정된 적
        private Enemy targetEnemy;

        // 현재 공격 전략
        private IAttackStrategy attackStrategy;

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
                    attackStrategy = GrapplingAttack.instance;
                }
                else if (distanceFromEnemy <= PunchDistance)
                {
                    attackStrategy = PunchAttack.instance;
                }
                else if (distanceFromEnemy <= DashDistance)
                {
                    attackStrategy = DashAttack.instance;
                }
                else
                {
                    attackStrategy = StopAttack.instance;
                }

                // 현재 전략을 통해 적 공격
                attackStrategy.Attack(targetEnemy);
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
    }



    /// <summary> 적 </summary>
    public class Enemy : MonoBehaviour { }



    /// <summary> 공격 행동 전략 인터페이스 </summary>
    public interface IAttackStrategy
    {
        void Attack(Enemy target);
    }

    /// <summary> 공격 행동 전략 </summary>
    public abstract class AttackStrategy<T> : IAttackStrategy where T :  AttackStrategy<T>, new()
    {
        // 각 하위 클래스마다 생성될 싱글톤 인스턴스
        public static readonly T instance = new T();
        public abstract void Attack(Enemy target);
    }

    /// <summary> 초근접 - 잡기 공격 </summary>
    public class GrapplingAttack : AttackStrategy<GrapplingAttack>
    {
        public override void Attack(Enemy target)
        {
            Debug.Log("잡기 공격");
        }
    }

    /// <summary> 근접 - 펀치 공격 </summary>
    public class PunchAttack : AttackStrategy<PunchAttack>
    {
        public override void Attack(Enemy target)
        {
            Debug.Log("펀치 공격");
        }
    }

    /// <summary> 비근접 - 돌진 공격 </summary>
    public class DashAttack : AttackStrategy<DashAttack>
    {
        public override void Attack(Enemy target)
        {
            Debug.Log("돌진 공격");
        }
    }

    /// <summary> 공격하지 않음 - Null 패턴의 활용 </summary>
    public class StopAttack : AttackStrategy<StopAttack>
    {
        public override void Attack(Enemy target)
        {
            Debug.Log("공격 중지");
        }
    }
}