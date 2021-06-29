using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-06-26 PM 3:45:31
// 작성자 : Rito

namespace Rito
{
    /// <summary> 
    /// 게임 시작시 파괴되는 오브젝트
    /// </summary>
    [DisallowMultipleComponent]
    public class TemporaryGameObject : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }
}