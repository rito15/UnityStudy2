using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-03-09 AM 12:13:15

public class RandomRangePick : MonoBehaviour
{
    public struct PickOption
    {
        public string content;
        public float probability;

        public PickOption(string c, float hundredProb)
        {
            content = c;
            probability = hundredProb * 0.01f;
        }
    }

    private void Start()
    {
        var pickTable = new[]{

            new PickOption("눈냥냥",       3.32f),
            new PickOption("토냥냥",       3.32f),
            new PickOption("판냥냥",       3.32f),
            new PickOption("붕어빵단팥이", 12.0f),
            new PickOption("붕어빵크림이", 12.0f),
            new PickOption("붕어빵탄이",   12.0f),
            new PickOption("식빵이",       12.0f),
            new PickOption("마롱이",       12.0f),
            new PickOption("고농축 프리미엄 생명의 물", 15.02f),
            new PickOption("오가닉 원더 쿠키", 15.02f)
        };

        float sum = 0f;
        foreach (var item in pickTable)
        {
            sum += item.probability;
        }

        float r = UnityEngine.Random.Range(0f, sum - Mathf.Epsilon);
        string result = "";
        float current = 0f;

        for (int i = 0; i < pickTable.Length; i++)
        {
            current += pickTable[i].probability;

            if (r < current)
            {
                result = pickTable[i].content;
                break;
            }
        }

        Debug.Log($"Random : {r}, Result : {result}");
    }

    public bool _flag = false;
    private void OnValidate()
    {
        if (_flag)
        {
            _flag = false;
            Start();
        }
    }
}