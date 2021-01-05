using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> �ڽ� �� �ϳ��� true�̸� true ���� �� ��ȸ ����,
    /// <para/> �ڽĵ��� ��� false�̸� false ����
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