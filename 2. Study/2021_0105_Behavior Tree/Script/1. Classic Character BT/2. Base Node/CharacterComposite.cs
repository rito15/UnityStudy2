using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    public abstract class CharacterComposite : CharacterNode, ICompositeNode
    {
        public List<INode> NodeList { get; private set; }

        public CharacterComposite(CharacterCoreClassic core) : base(core)
        {
            NodeList = new List<INode>();
        }

        public override abstract bool Run();

        public CharacterComposite Add(INode childNode)
        {
            NodeList.Add(childNode);
            return this;
        }
    }
}