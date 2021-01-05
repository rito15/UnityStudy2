using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /// <summary> 빌더 패턴 BT 노드 </summary>
    public class CharacterNode : INode
    {
        public List<CharacterNode> NodeList { get; protected set; }

        public CharacterNode()
        {
            NodeList = new List<CharacterNode>
            {
                this
            };
        }

        public CharacterNode Selector(CharacterNode nodes)
        {
            CharacterSelector newSelector = new CharacterSelector(nodes);
            NodeList.Add(newSelector);
            return this;
        }

        public CharacterNode Sequence(CharacterNode nodes)
        {
            CharacterSequence newSequence = new CharacterSequence(nodes);
            NodeList.Add(newSequence);
            return this;
        }

        public CharacterNode Condition(Func<bool> condition)
        {
            CharacterCondition newCondition = new CharacterCondition(condition);
            NodeList.Add(newCondition);
            return this;
        }

        public CharacterNode Action(Action action)
        {
            CharacterAction newAction = new CharacterAction(action);
            NodeList.Add(newAction);
            return this;
        }

        public CharacterNode Update(Action update)
        {
            CharacterUpdate newUpdate = new CharacterUpdate(update);
            NodeList.Add(newUpdate);
            return this;
        }

        public virtual bool Run() => true;
    }
}