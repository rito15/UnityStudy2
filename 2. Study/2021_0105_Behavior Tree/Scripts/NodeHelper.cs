using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    // Core에서
    // using static Rito.BehaviorTree.NodeHelper;
    public static class NodeHelper
    {
        public static INode Selector(params INode[] nodes) => new SelectorNode(nodes);
        public static INode Sequence(params INode[] nodes) => new SequenceNode(nodes);
        public static INode Parallel(params INode[] nodes) => new ParallelNode(nodes);

        public static INode IfAction(Func<bool> condition, Action action) => new IfActionNode(condition, action);
        public static INode IfElseAction(Func<bool> condition, Action ifAction, Action ifElseAction) => new IfElseActionNode(condition, ifAction, ifElseAction);

        public static INode Condition(Func<bool> condition) => new ConditionNode(condition);
        public static INode Action(Action action) => new ActionNode(action);
    }
}