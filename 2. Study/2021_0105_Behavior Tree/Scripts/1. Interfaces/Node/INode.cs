using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> �ൿƮ�� �ֻ��� �������̽� </summary>
    public interface INode
    {
        bool Run();
    }
}