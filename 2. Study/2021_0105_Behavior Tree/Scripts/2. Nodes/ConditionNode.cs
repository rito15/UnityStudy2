using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 조건 검사 노드 </summary>
    public class ConditionNode : ILeafNode
    {
        public Func<bool> Condition { get; private set; }
        public ConditionNode(Func<bool> condition)
        {
            Condition = condition;
        }

        public bool Run() => Condition();

        // Func <=> ConditionNode 타입 캐스팅
        public static implicit operator ConditionNode(Func<bool> condition) => new ConditionNode(condition);
        public static implicit operator Func<bool>(ConditionNode condition) => new Func<bool>(condition.Condition);
    }
}