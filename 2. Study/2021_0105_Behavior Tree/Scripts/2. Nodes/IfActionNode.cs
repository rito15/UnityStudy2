using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    // 조건 참일 경우 Action 수행 및 true 리턴
    // 조건 거짓일 경우 false 리턴
    /// <summary> 조건에 따른 수행 노드 </summary>
    public class IfActionNode : IDecoratorNode
    {
        public Func<bool> Condition { get; private set; }
        public Action Action { get; private set; }
        public IfActionNode(Func<bool> condition, Action action)
        {
            Condition = condition;
            Action = action;
        }
        public IfActionNode(ConditionNode condition, ActionNode action)
        {
            Condition = condition.Condition;
            Action = action.Action;
        }

        public bool Run()
        {
            bool result = Condition();
            if(result) Action();
            return result;
        }
    }
}