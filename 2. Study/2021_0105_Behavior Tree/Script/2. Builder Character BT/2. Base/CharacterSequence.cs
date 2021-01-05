using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> 자식 중에 false가 발견되면 바로 false 리턴 후 순회 중지
    /// <para/> 자식이 모두 true일 경우 true 리턴
    /// </summary>
    public class CharacterSequence : CharacterNode, ISequenceNode
    {
        public List<CharacterNode> ChildList { get; protected set; }

        public CharacterSequence(CharacterNode nodes)
        {
            ChildList = new List<CharacterNode>();
            ChildList.AddRange(nodes.NodeList);
        }

        public override bool Run()
        {
            foreach (var node in ChildList)
            {
                bool result = node.Run();
                if (result == false)
                    return false;
            }
            return true;
        }
    }
}