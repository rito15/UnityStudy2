using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.ClassicCharacter
{
    // 캐릭터 BT - 고전 타입
    public class CharacterCoreClassic : MonoBehaviour, ICore
    {
        public CharacterData Data { get; private set; }
        public CharacterState State { get; private set; }

        private CharacterComposite _rootSelector;

        private void Awake()
        {
            MakeNode();
        }

        private void Update()
        {
            _rootSelector.Run();
        }

        private void MakeNode()
        {
            /*
                                      Selector
                        Sequence                      Sequence
                Condition     Action          Condition     Action
               (Wasd Input)  (Key Move)     (Mouse Input) (Mouse Move)

            */

            _rootSelector = new CharacterSelector(this);
            WasdInputCondition  wasdInput  = new WasdInputCondition(this);
            MouseInputCondition mouseInput = new MouseInputCondition(this);
            KeyboardMoveAction  keyMove    = new KeyboardMoveAction(this);
            //MouseMoveAction     mouseMove  = new MouseMoveAction(this);
            CAction mouseMove = new CAction(() => { Debug.Log("Action : Mouse Move"); });

            CharacterSequence keyMoveSequence   = new CharacterSequence(this);
            CharacterSequence mouseMoveSequence = new CharacterSequence(this);

            _rootSelector.Add(keyMoveSequence).Add(mouseMoveSequence);
            keyMoveSequence.Add(wasdInput).Add(keyMove);
            mouseMoveSequence.Add(mouseInput).Add(mouseMove);
        }

        class WasdInputCondition : CharacterCondition
        {
            public WasdInputCondition(CharacterCoreClassic core) : base(core) { }

            public override bool Check()
            {
                bool check =
                    Input.GetKey(KeyCode.W) ||
                    Input.GetKey(KeyCode.A) ||
                    Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.D);
                Debug.Log($"Condition : Wasd Input ({check})");

                return check;
            }
        }

        class MouseInputCondition : CharacterCondition
        {
            public MouseInputCondition(CharacterCoreClassic core) : base(core) { }

            public override bool Check()
            {
                bool check = Input.GetMouseButton(1);
                Debug.Log($"Condition : Mouse Input ({check})");

                return check;
            }
        }

        class KeyboardMoveAction : CharacterAction
        {
            public KeyboardMoveAction(CharacterCoreClassic core) : base(core) { }

            public override void Act()
            {
                Debug.Log("Action : Keyboard Move");
            }
        }

        class MouseMoveAction : CharacterAction
        {
            public MouseMoveAction(CharacterCoreClassic core) : base(core) { }

            public override void Act()
            {
                Debug.Log("Action : Mouse Move");
            }
        }
    }
}