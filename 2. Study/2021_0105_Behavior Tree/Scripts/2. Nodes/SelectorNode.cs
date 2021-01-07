using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 자식들을 순회하며 true인 것 하나만 수행하는 노드 </summary>
    public class SelectorNode : CompositeNode
    {
        public SelectorNode(params INode[] nodes) : base(nodes) { }

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