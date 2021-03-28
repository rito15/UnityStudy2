using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rito.InventorySystem;

// 날짜 : 2021-03-19 PM 11:01:36
// 작성자 : Rito

public class Test_InventoryUI : MonoBehaviour
{
    public Inventory _inventory;

    public ItemData[] _itemDataArray;

    private void Start()
    {
        if (_itemDataArray?.Length > 0)
        {
            for (int i = 0; i < _itemDataArray.Length; i++)
            {
                _inventory.Add(_itemDataArray[i], 3);
            }
        }
    }
}