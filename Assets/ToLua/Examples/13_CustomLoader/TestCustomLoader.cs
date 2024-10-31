using UnityEngine;
using System.IO;
using LuaInterface;
using MessagePack;
using System.Collections.Generic;
using System;

[MessagePackObject]
public class MyData
{
    [Key(0)]
    public int Id { get; set; }
    [Key(1)]
    public string Name { get; set; }
    [Key(2)]
    public Dictionary<string, string> Attributes { get; set; }
    [Key(3)]
    public List<string> ListValues { get; set; }
}

//use menu Lua->Copy lua files to Resources. 之后才能发布到手机
public class TestCustomLoader : LuaClient
{
    string tips = "Test custom loader";
    IntPtr address;
    int size = 0;
    unsafe void GenerateFile()
    {
        var data = new MyData
        {
            Id = 1,
            Name = "John",
            Attributes = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            },
            ListValues = new List<string>() { "1", "2" }
        };
        var bytes = MessagePackSerializer.Serialize(data);
        var d1 = MessagePackSerializer.Deserialize<MyData>(bytes);
        File.WriteAllBytes("D:\\data.bin", bytes);
        fixed (byte* ptr = bytes)
        {
            size = bytes.Length;
            address = (IntPtr)ptr;
        }
    }

    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    protected override void CallMain()
    {
        GenerateFile();
        LuaFunction func = luaState.GetFunction("Test");
        func.Call();
        func.Dispose();

        LuaFunction func1 = luaState.GetFunction("TestParam");
        func1.BeginPCall();
        func1.Push(address);
        func1.Push(size);
        func1.PCall();
        func1.EndPCall();
        func1.Dispose();
    }

    protected override void StartMain()
    {
        luaState.DoFile("MessagePack.lua");
        luaState.DoFile("TestLoader.lua");
        CallMain();
    }

    new void Awake()
    {
#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived += Logger;
#else
        Application.RegisterLogCallback(Logger);
#endif    
        base.Awake();
    }

    new void OnApplicationQuit()
    {
        base.OnApplicationQuit();

#if UNITY_5 || UNITY_2017 || UNITY_2018
        Application.logMessageReceived -= Logger;
#else
        Application.RegisterLogCallback(null);
#endif    
    }

    void Logger(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), tips);
    }
}
