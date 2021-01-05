using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> �ڽ� �߿� false�� �߰ߵǸ� �ٷ� false ���� �� ��ȸ ����
    /// <para/> �ڽ��� ��� true�� ��� true ����
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