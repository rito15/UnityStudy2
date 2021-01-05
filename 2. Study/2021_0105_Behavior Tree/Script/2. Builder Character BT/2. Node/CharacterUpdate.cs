using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    public class CharacterUpdate : CharacterNode, IUpdateNode
    {
        private Action _update;
        public CharacterUpdate(Action action)
        {
            _update = action;
        }
        public override bool Run() { _update(); return true; }
    }
}