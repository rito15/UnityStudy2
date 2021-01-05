using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    /// <summary> 조건 검사 명시적 구현 </summary>
    public abstract class CharacterCondition : CharacterNode, IConditionNode
    {
        public CharacterCondition(CharacterCoreClassic core) : base(core) { }
        public override bool Run() => Check();
        public abstract bool Check();
    }

    /// <summary> 간이 조건 검사 </summary>
    public class CCondition : IConditionNode
    {
        private readonly Func<bool> _condition;
        public CCondition(Func<bool> condition) => _condition = condition;
        public bool Run() => _condition();
    }
}