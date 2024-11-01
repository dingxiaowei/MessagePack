using MessagePack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//[MessagePackObject]
//public class MyData
//{
//    [Key(0)]
//    public int Id { get; set; }
//    [Key(1)]
//    public string Name { get; set; }
//    [Key(2)]
//    public Dictionary<string, string> Attributes { get; set; }
//    [Key(3)]
//    public List<string> ListValues { get; set; }
//}

public class TestMsgPack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var data = new MyData
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
        File.WriteAllBytes("D:\\data.bin",bytes);
        Debug.LogError(bytes.Length);
        Debug.LogError(d1.Name);
    }
}
