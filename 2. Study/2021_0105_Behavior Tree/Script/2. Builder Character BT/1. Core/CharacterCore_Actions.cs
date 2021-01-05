using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree.Character
{
    /******************************************************************
     *                        Action �ʵ� ����
     ******************************************************************/
    public partial class CharacterCore : MonoBehaviour, ICore
    {
        Action KeyMove = new Action(() => Debug.Log("Action : Key Move"));
        Action MouseMove = new Action(() => Debug.Log("Action : Mouse Move"));
    }
}