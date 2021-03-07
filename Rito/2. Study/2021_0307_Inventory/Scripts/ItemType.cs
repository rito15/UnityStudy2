using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-07 PM 8:40:36
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public enum ItemType
    {
        /// <summary> 소비 아이템 </summary>
        Portion,

        /// <summary> 장비 아이템 </summary>
        Equipment,

        /// <summary> 퀘스트 아이템 </summary>
        Quest,

        /// <summary> 재료 아이템 </summary>
        Ingredient,
    }
}