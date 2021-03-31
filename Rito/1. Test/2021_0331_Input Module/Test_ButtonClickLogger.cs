using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-03-31 PM 3:34:53
// 작성자 : Rito

public class Test_ButtonClickLogger : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => Debug.Log("Button Clicked"));
    }
}