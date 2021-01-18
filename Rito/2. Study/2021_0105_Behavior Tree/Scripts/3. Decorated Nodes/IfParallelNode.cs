using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-16 PM 11:23:43
// 작성자 : Rito

namespace Rito.BehaviorTree
{
    public class IfParallelNode : DecoratedCompositeNode
    {
        public IfParallelNode(Func<bool> condition, params INode[] nodes)
            : base(condition, new ParallelNode(nodes)) { }
    }
}