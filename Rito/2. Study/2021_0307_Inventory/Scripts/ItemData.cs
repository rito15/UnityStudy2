using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-03-07 PM 8:45:55
// 작성자 : Rito

namespace Rito.InventorySystem
{
    [CreateAssetMenu(fileName = "Item_", menuName = "Rito/Scriptable Objects/Item Data", order = 1)]
    public class ItemData : ScriptableObject
    {
        public int ID => _id;
        public string Name => _name;
        public ItemType Type => _type;
        public Sprite IconSprite => _iconSprite;

        [SerializeField] private int      _id;
        [SerializeField] private string   _name;
        [SerializeField] private ItemType _type;
        [SerializeField] private Sprite   _iconSprite;
        [SerializeField] private GameObject _dropItemPrefab;
    }
}