using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test_JsonSerialization : MonoBehaviour
{
    public Test_Data _td00;
    public Test_Data[] _tdArr;
    public List<Test_Data> _tdList;

    [SerializeField]
    public Test_Data[] _tdArr_Serialized;
    [SerializeField]
    public List<Test_Data> _tdList_Serialized;

    public Test_Data_ListIncluded _tdListCore;
    public Test_Data_ArrayIncluded _tdArrayCore;

    private void Start()
    {
        _td00 = new Test_Data();
        _tdArr = new Test_Data[] { new Test_Data("Name00", 100), new Test_Data("Name01", 101) };
        _tdList = new List<Test_Data>(_tdArr);

        _tdArr_Serialized = new Test_Data[] { new Test_Data("Name00", 100), new Test_Data("Name01", 101) };
        _tdList_Serialized = _tdList;

        _tdListCore = new Test_Data_ListIncluded();
        _tdListCore._tdList = _tdList;

        _tdArrayCore = new Test_Data_ArrayIncluded();
        _tdArrayCore._tdArr = _tdArr;

        // 가능
        Debug.Log(JsonUtility.ToJson(_td00));

        // 불가능
        Debug.Log(JsonUtility.ToJson(_tdArr));
        Debug.Log(JsonUtility.ToJson(_tdList));

        Debug.Log(JsonUtility.ToJson(_tdArr_Serialized));
        Debug.Log(JsonUtility.ToJson(_tdList_Serialized));

        // 가능
        Debug.Log(JsonUtility.ToJson(_tdListCore));
        Debug.Log(JsonUtility.ToJson(_tdArrayCore));
    }
}

[Serializable]
public class Test_Data
{
    public string _name;
    public float _hp;

    public Test_Data() : this("NAME00", 100f) { }

    public Test_Data(string n, float hp)
    {
        _name = n;
        _hp = hp;
    }
}

[Serializable]
public class Test_Data_ListIncluded
{
    public string _nnn;

    [SerializeField]
    public List<Test_Data> _tdList;
}

[Serializable]
public class Test_Data_ArrayIncluded
{
    public string _nnn;

    [SerializeField]
    public Test_Data[] _tdArr;
}