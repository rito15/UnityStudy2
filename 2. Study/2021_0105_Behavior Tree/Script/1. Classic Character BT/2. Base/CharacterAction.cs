using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    /// <summary> 액션 명시적 구현 </summary>
    public abstract class CharacterAction : CharacterNode, IActionNode
    {
        public CharacterAction(CharacterCoreClassic core) : base(core) { }
        public override bool Run() { Act(); return true; }
        public abstract void Act();
    }

    /// <summary> 간이 액션 </summary>
    public class CAction : IActionNode
    {
        private readonly Action _action;
        public CAction(Action action) => _action = action;
        public bool Run() { _action(); return true; }
    }
}