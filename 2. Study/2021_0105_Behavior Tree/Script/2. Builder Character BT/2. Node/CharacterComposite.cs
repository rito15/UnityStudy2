using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 
    /// <para/> 자식을 순회하는 노드
    /// </summary>
    public abstract class CharacterComposite : CharacterNode, ICompositeNode
    {
        public List<CharacterNode> ChildList { get; protected set; }

        public CharacterComposite(CharacterNode nodes)
        {
            ChildList = new List<CharacterNode>();
            ChildList.AddRange(nodes.NodeList);
        }

        public abstract bool Run();
    }
}