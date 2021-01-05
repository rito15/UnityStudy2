using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    public class CharacterSelector : CharacterComposite, ISelectorNode
    {
        public CharacterSelector(CharacterCoreClassic core) : base(core) { }

        public override bool Run()
        {
            foreach (var node in NodeList)
            {
                bool result = node.Run();
                if (result == true)
                    return true; 
            }
            return false;
        }
    }
}