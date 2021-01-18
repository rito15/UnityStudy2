using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 
    /// <para/> 자식들을 순회하는 노드
    /// </summary>
    public abstract class CompositeNode : ICompositeNode
    {
        public List<INode> ChildList { get; protected set; }

        // 생성자
        public CompositeNode(params INode[] nodes) => ChildList = new List<INode>(nodes);

        // 자식 노드 추가
        public CompositeNode Add(INode node)
        {
            ChildList.Add(node);
            return this;
        }

        public abstract bool Run();
    }
}