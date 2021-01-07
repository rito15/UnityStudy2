using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    // 캐릭터 BT - 빌더형
    /******************************************************************
     *                          노드 구성
     ******************************************************************/
    public partial class CharacterCore : MonoBehaviour, ICore
    {
        public CharacterData Data { get; private set; }
        public CharacterState State { get; private set; }

        private CharacterNode _root;

        private void Awake()
        {
            MakeNode();
        }

        private void Update()
        {
            _root.Run();
        }

        private void MakeNode()
        {
            /*
                                      Selector
                        Sequence                      Sequence
                Condition     Action          Condition     Action
               (Wasd Input)  (Key Move)     (Mouse Input) (Mouse Move)

            */
            _root =
            Selector
            (
                ConditionalAction(WasdInput, KeyMove)
                .ConditionalAction(MouseInput, MouseMove)
            );

        }

        private CharacterNode Selector(CharacterNode nodes)
        {
            return new CharacterSelector(nodes);
        }

        private CharacterNode Sequence(CharacterNode nodes)
        {
            return new CharacterSequence(nodes);
        }

        private CharacterNode Parallel(CharacterNode nodes)
        {
            return new CharacterParallel(nodes);
        }

        private CharacterNode Condition(Func<bool> condition)
        {
            return new CharacterCondition(condition);
        }

        private CharacterNode ConditionalAction(Func<bool> condition, Action action)
        {
            return new CharacterConditionalAction(condition, action);
        }

        private CharacterNode ConditionalUpdate(Func<bool> condition, Action update)
        {
            return new CharacterConditionalUpdate(condition, update);
        }

        private CharacterNode Action(Action action)
        {
            return new CharacterAction(action);
        }

        private CharacterNode Update(Action update)
        {
            return new CharacterUpdate(update);
        }
    }
}