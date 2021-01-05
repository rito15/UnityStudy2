using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    public abstract class CharacterNode : INode
    {
        public CharacterCoreClassic Core { get; private set; }
        public CharacterData Data { get; private set; }
        public CharacterState State { get; private set; }

        public CharacterNode(CharacterCoreClassic core)
        {
            Core = core;
            Data = core.Data;
            State = core.State;
        }

        public abstract bool Run();
    }
}