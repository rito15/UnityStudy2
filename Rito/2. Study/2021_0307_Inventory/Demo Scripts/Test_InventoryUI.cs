using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rito.InventorySystem;

// 날짜 : 2021-03-19 PM 11:01:36
// 작성자 : Rito

public class Test_InventoryUI : MonoBehaviour
{
    public Inventory _inventory;

    public ItemData[] _itemDataArray;
    
    [Space(12)]
    public Button _trimButton;
    public Button _sortButton;

    private void Start()
    {
        if (_itemDataArray?.Length > 0)
        {
            for (int i = 0; i < _itemDataArray.Length; i++)
            {
                _inventory.Add(_itemDataArray[i], 3);

                if(_itemDataArray[i] is CountableItemData)
                    _inventory.Add(_itemDataArray[i], 255);
            }
        }

        _trimButton.onClick.AddListener(_inventory.TrimAll);
        _sortButton.onClick.AddListener(_inventory.SortAll);
    }

}