using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> 자식들 리턴에 관계 없이 모두 순회
    /// </summary>
    public class CharacterParallel : CharacterComposite, ISelectorNode
    {
        public CharacterParallel(CharacterNode nodes) : base(nodes) { }

        public override bool Run()
        {
            foreach (var node in ChildList)
            {
                node.Run();
            }
            return true;
        }
    }
}