using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-01-16 PM 11:23:12
// 작성자 : Rito

namespace Rito.BehaviorTree
{
    public class IfSelectorNode : DecoratedCompositeNode
    {
        public IfSelectorNode(Func<bool> condition, params INode[] nodes)
            : base(condition, new SelectorNode(nodes)) { }
    }
}