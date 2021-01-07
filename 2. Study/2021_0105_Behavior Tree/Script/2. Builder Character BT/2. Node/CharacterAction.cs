using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 단순 수행 노드 </summary>
    public class CharacterAction : CharacterNode, IActionNode
    {
        private Action _action;
        public CharacterAction(Action action)
        {
            _action = action;
        }
        public override bool Run() { _action(); return true; }
    }
}