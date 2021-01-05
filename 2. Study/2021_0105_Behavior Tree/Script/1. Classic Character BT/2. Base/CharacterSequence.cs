using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    public class CharacterSequence : CharacterComposite, ISequenceNode
    {
        public CharacterSequence(CharacterCoreClassic core) : base(core) { }

        public override bool Run()
        {
            foreach (var node in NodeList)
            {
                bool result = node.Run();
                if (result == false)
                    return false;
            }
            return true;
        }
    }
}