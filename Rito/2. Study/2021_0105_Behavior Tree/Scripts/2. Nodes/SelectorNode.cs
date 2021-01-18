using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> �ڽĵ��� ��ȸ�ϸ� true�� �� �ϳ��� �����ϴ� ��� </summary>
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