using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    public class CharacterCondition : CharacterNode, IConditionNode
    {
        private Func<bool> _condition;
        public CharacterCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override bool Run() => _condition();
    }
}