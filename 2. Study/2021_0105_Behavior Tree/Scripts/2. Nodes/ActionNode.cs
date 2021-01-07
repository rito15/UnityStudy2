using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 행동 수행 노드 </summary>
    public class ActionNode : ILeafNode
    {
        public Action Action { get; private set; }
        public ActionNode(Action action)
        {
            Action = action;
        }

        public bool Run()
        {
            Action();
            return true;
        }

        // Action <=> ActionNode 타입 캐스팅
        public static implicit operator ActionNode(Action action) => new ActionNode(action);
        public static implicit operator Action(ActionNode action) => new Action(action.Action);
    }
}