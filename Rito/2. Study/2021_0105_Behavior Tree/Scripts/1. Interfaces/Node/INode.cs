using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> 행동트리 최상위 인터페이스 </summary>
    public interface INode
    {
        bool Run();
    }
}