using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary>
    /// <para/> 조건이 true일 경우 액션 수행 및 true 리턴, 
    /// <para/> 조건이 false일 경우 false 리턴
    /// <para/> Sequence(Condition().Action())을 하나로 합친 형태
    /// </summary>
    public class CharacterConditionalAction : CharacterNode, IActionNode
    {
        private readonly Func<bool> _condition;
        private readonly Action _action;
        public CharacterConditionalAction(Func<bool> condition, Action action)
        {
            _condition = condition;
            _action = action;
        }
        public override bool Run()
        {
            if (_condition())
            {
                _action();
                return true;
            }
            return false;
        }
    }
}