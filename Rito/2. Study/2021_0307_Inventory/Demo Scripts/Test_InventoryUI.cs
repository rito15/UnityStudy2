using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rito.InventorySystem;

// 날짜 : 2021-03-19 PM 11:01:36
// 작성자 : Rito

public class Test_InventoryUI : MonoBehaviour
{
    public InventoryUI _inventoryUI;

    public Sprite[] _itemIcons;

    private void Start()
    {
        if (_itemIcons?.Length > 0)
        {
            for (int i = 0; i < _itemIcons.Length; i++)
            {
                _inventoryUI.TryAddItem(i, _itemIcons[i]);
            }
        }
    }
}