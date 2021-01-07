using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 자식들 중 false가 나올 때까지 연속으로 순회하는 노드 </summary>
    public class SequenceNode : CompositeNode
    {
        public SequenceNode(params INode[] nodes) : base(nodes) { }

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