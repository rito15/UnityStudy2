using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    /// <summary> 업데이트 명시적 구현 </summary>
    public abstract class CharacterUpdate : CharacterNode, IUpdateNode
    {
        public CharacterUpdate(CharacterCoreClassic core) : base(core) { }
        public override bool Run() { Update(); return true; }
        public abstract void Update();
    }

    /// <summary> 간이 업데이트 </summary>
    public class CUpdate : IUpdateNode
    {
        private readonly Action _update;
        public CUpdate(Action update) => _update = update;
        public bool Run() { _update(); return true; }
    }
}