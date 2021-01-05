using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> 자식 중 하나라도 true이면 true 리턴 후 순회 중지,
    /// <para/> 자식들이 모두 false이면 false 리턴
    /// </summary>
    public class CharacterSelector : CharacterNode, ISelectorNode
    {
        public List<CharacterNode> ChildList { get; protected set; }

        public CharacterSelector(CharacterNode nodes)
        {
            ChildList = new List<CharacterNode>();
            ChildList.AddRange(nodes.NodeList);
        }

        public override bool Run()
        {
            foreach (var node in ChildList)
            {
                bool result = node.Run();
                if (result == true)
                    return true; 
            }
            return false;
        }
    }
}