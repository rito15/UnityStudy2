using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary>
    /// <para/> 조건이 true일 경우 업데이트 수행 및 true 리턴, 
    /// <para/> 조건이 false일 경우 false 리턴
    /// <para/> Sequence(Condition().Update())를 하나로 합친 형태
    /// </summary>
    public class CharacterConditionalUpdate : CharacterNode, IUpdateNode
    {
        private readonly Func<bool> _condition;
        private readonly Action _update;
        public CharacterConditionalUpdate(Func<bool> condition, Action update)
        {
            _condition = condition;
            _update = update;
        }
        public override bool Run()
        {
            if (_condition())
            {
                _update();
                return true;
            }
            return false;
        }
    }
}