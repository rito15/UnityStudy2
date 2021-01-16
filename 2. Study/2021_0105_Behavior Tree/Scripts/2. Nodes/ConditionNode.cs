using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 조건 검사 노드 </summary>
    public class ConditionNode : IDecoratorNode
    {
        public Func<bool> Condition { get; protected set; }
        public ConditionNode(Func<bool> condition)
        {
            Condition = condition;
        }

        public bool Run() => Condition();

        // Func <=> ConditionNode 타입 캐스팅
        public static implicit operator ConditionNode(Func<bool> condition) => new ConditionNode(condition);
        public static implicit operator Func<bool>(ConditionNode condition) => new Func<bool>(condition.Condition);

        // Decorated Node Creator Methods
        public IfActionNode Action(Action action)
            => new IfActionNode(Condition, action);

        public IfSequenceNode Sequence(params INode[] nodes)
            => new IfSequenceNode(Condition, nodes);

        public IfSelectorNode Selector(params INode[] nodes)
            => new IfSelectorNode(Condition, nodes);

        public IfParallelNode Parallel(params INode[] nodes)
            => new IfParallelNode(Condition, nodes);
    }
}