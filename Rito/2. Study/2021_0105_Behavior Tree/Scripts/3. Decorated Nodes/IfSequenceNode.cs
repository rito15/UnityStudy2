using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-16 PM 11:20:37
// 작성자 : Rito

namespace Rito.BehaviorTree
{
    public class IfSequenceNode : DecoratedCompositeNode
    {
        public IfSequenceNode(Func<bool> condition, params INode[] nodes)
            : base(condition, new SequenceNode(nodes)) { }
    }
}