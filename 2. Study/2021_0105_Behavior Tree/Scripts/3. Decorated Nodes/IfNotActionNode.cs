using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-13 PM 4:20:16

namespace Rito.BehaviorTree
{
    // 조건 거짓일 경우 Action 수행 및 true 리턴
    // 조건 참일 경우 false 리턴
    /// <summary> 조건에 따른 수행 노드 </summary>
    public class IfNotActionNode : ILeafNode
    {
        public Func<bool> Condition { get; private set; }
        public Action Action { get; private set; }
        public IfNotActionNode(Func<bool> condition, Action action)
        {
            Condition = () => !condition();
            Action = action;
        }
        public IfNotActionNode(ConditionNode condition, ActionNode action)
        {
            Condition = () => !condition.Condition();
            Action = action.Action;
        }

        public bool Run()
        {
            bool result = Condition();
            if (result) Action();
            return result;
        }
    }
}