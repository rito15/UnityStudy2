using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rito;

public class Test_ActionParamOverhead : MonoBehaviour
{
    public UnityEngine.UI.Text _text;
    public UnityEngine.UI.Button _button;

    public int count = 50000;
    public bool _flag = false;
    private void OnValidate()
    {
        if (_flag)
        {
            _flag = false;
            Start();
        }
    }

    private void Awake()
    {
        _button.onClick.AddListener(Start);
    }

    private void Start()
    {
        RitoWatch.Clear();
        RitoWatch.BeginCheck("Inline");
        TestInline();

        RitoWatch.BeginCheck("Method Call");
        TestMethodCall();

        RitoWatch.BeginCheck("Method Callback");
        TestMethodCallback();

        RitoWatch.BeginCheck("Lambda Callback");
        TestLambdaCallback();

        RitoWatch.BeginCheck("Lambda Method Callback");
        TestLambdaMethodCallback();

        RitoWatch.EndCheck();
        //RitoWatch.PrintAllLogs();

        //Debug.Log(RitoWatch.GetAllLogString());
        _text.text = RitoWatch.GetAllLogString();
    }


    private void TestInline()
    {
        for (int i = 0; i < count; i++)
        {
            _ = Mathf.Round(2.3f) * Mathf.Sin(2.5f);
            _ = Mathf.FloatToHalf(4.4f) * Mathf.Log10(6.34f);
            _ = Mathf.Acos(5.5f) * Mathf.Tan(34.4f);
        }
    }

    private void TestMethodCall()
    {
        for (int i = 0; i < count; i++)
        {
            MethodA();
            MethodB();
            MethodC();
        }
    }

    private void TestLambdaCallback()
    {
        for (int i = 0; i < count; i++)
        {
            ActionCall(() => _ = Mathf.Round(2.3f) * Mathf.Sin(2.5f));
            ActionCall(() => _ = Mathf.FloatToHalf(4.4f) * Mathf.Log10(6.34f));
            ActionCall(() => _ = Mathf.Acos(5.5f) * Mathf.Tan(34.4f));
        }
    }

    private void TestMethodCallback()
    {
        for (int i = 0; i < count; i++)
        {
            ActionCall(MethodA);
            ActionCall(MethodB);
            ActionCall(MethodC);
        }
    }
    private void TestLambdaMethodCallback()
    {
        for (int i = 0; i < count; i++)
        {
            ActionCall(() => MethodA());
            ActionCall(() => MethodB());
            ActionCall(() => MethodC());
        }
    }

    private void ActionCall(System.Action action)
    {
        action();
    }



    private void MethodA()
    {
        _ = Mathf.Round(2.3f) * Mathf.Sin(2.5f);
    }
    private void MethodB()
    {
        _ = Mathf.FloatToHalf(4.4f) * Mathf.Log10(6.34f);
    }
    private void MethodC()
    {
        _ = Mathf.Acos(5.5f) * Mathf.Tan(34.4f);
    }
}
