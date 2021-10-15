using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 날짜 : 2021-10-12 PM 3:46:29
// 작성자 : Rito

/// <summary> 
/// 
/// </summary>
public class Test_GCAlloc : MonoBehaviour
{
    private List<int[]> list = new List<int[]>(1000);

    private void Update()
    {
        //strList.Add(new string('a', 4));
        //string a = new string('a', 4);
        list.Add(new int[1]);
    }
}