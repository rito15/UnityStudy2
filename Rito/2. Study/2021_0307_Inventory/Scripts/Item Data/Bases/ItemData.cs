using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-03-07 PM 8:45:55
// 작성자 : Rito

namespace Rito.InventorySystem
{
    public abstract class ItemData : ScriptableObject
    {
        public int ID => _id;
        public string Name => _name;
        public Sprite IconSprite => _iconSprite;

        [SerializeField] private int      _id;
        [SerializeField] private string   _name;
        [SerializeField] private Sprite   _iconSprite;
        [SerializeField] private GameObject _dropItemPrefab;
    }
}