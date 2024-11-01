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
    public Dictionary<int, long> Attributes { get; set; }
    [Key(3)]
    public List<string> ListValues { get; set; }
}

//use menu Lua->Copy lua files to Resources. 之后才能发布到手机
public class TestCustomLoader : LuaClient
{
    string tips = "Test custom loader";
    IntPtr address;
    int size = 0;
    int times = 10;
    MyData data;

    unsafe void GenerateFile()
    {
        data = new MyData
        {
            Id = 1,
            Name = "John",
            Attributes = new Dictionary<int, long>
            {
                { 1, 11111 },
                { 2, 22222 }
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

        //LuaFunction func1 = luaState.GetFunction("TestParam");
        //for (int i = 0; i < 1000; i++)
        //{
        //    func1.BeginPCall();
        //    func1.Push(address);
        //    func1.Push(size);
        //    func1.PCall();
        //    func1.EndPCall();
        //}
        //func1.Dispose();
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

    void SharedMemoryCase()
    {
        LuaFunction func = luaState.GetFunction("TestSharedMemory");
        for (int i = 0; i < times; i++)
        {
            func.BeginPCall();
            func.Push(address);
            func.Push(size);
            func.PCall();
            func.EndPCall();
        }
        func.Dispose();
    }

    void CallWrapCase()
    {
        LuaFunction func = luaState.GetFunction("TestCallWrap");
        for (int i = 0; i < times; i++)
        {
            func.BeginPCall();
            func.Push(data.Attributes);
            func.PCall();
            func.EndPCall();
        }
        func.Dispose();
    }

    void TestLuaTable()
    {
        LuaTable table = luaState.GetTable("mydata");
        Debug.LogError("------");
        Debug.LogError(table);
        int top = luaState.LuaGetTop();
        luaState.LuaSetTop(top);
        try
        {
            table.RawSetIndex(1, 1);
            table.RawSetIndex(2, "Jon");
            luaState.Push(table);

            //luaState.Push(1);
            //luaState.LuaRawSet(-3);

            //luaState.Push("John");
            //luaState.LuaRawSet(-3);


            //luaState.LuaPop(1);
        }
        catch (Exception e)
        {
            luaState.LuaSetTop(top);
            throw e;
        }

        //luaState.LuaSetTop(top);

        LuaFunction func = luaState.GetFunction("TestLuaTable");
        func.Call();
        func.Dispose();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 200, 400, 400), tips);
        if (GUI.Button(new Rect(10, 70, 150, 30), "内存共享测试"))
        {
            SharedMemoryCase();
        }
        if (GUI.Button(new Rect(10, 150, 150, 30), "wrap交互测试"))
        {
            CallWrapCase();
        }

        if (GUI.Button(new Rect(10, 230, 150, 30), "luatable测试"))
        {
            TestLuaTable();
        }
    }
    //private void Update()
    //{
    //    SharedMemoryCase();
    //    CallWrapCase();
    //}
}
