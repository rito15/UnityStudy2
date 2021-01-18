using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rito.BehaviorTree;

using static Rito.BehaviorTree.NodeHelper;

public class CoreTest : MonoBehaviour, ICore
{
    private INode _rootNode;

    private void Awake()
    {
        MakeNode();
    }

    private void Update()
    {
        _rootNode.Run();
    }

    /// <summary> BT 노드 조립 </summary>
    private void MakeNode()
    {
        _rootNode =

            //If(() => Input.GetKey(KeyCode.Q)).
            Selector
            (
                IfAction(KeyMoveInput, KeyMoveAction),
                IfAction(MouseMoveInput, MouseMoveAction)
            );
    }

    private bool KeyMoveInput()
    {
        bool result =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        Debug.Log($"Condition : Key Move INPUT ({result})");
        return result;
    }
    private bool MouseMoveInput()
    {
        bool result = Input.GetMouseButton(1);
        Debug.Log($"Condition : Mouse Move INPUT ({result})");
        return result;
    }
    private void KeyMoveAction() => Debug.Log($"Action : Key Move");
    private void MouseMoveAction() => Debug.Log($"Action : Mouse Move");
}
