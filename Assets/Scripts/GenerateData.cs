using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using ProtoBuf;
using MessagePack;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Diagnostics;

//using System.Diagnostics;

[Serializable]
[ProtoContract]
[MessagePackObject]
public class NumberAsteroidData
{
    [ProtoMember(1)]
    [Key(0)]
    public int Type;

    [ProtoMember(2)]
    [Key(1)]
    public int Number;
}

[Serializable]
[ProtoContract]
[MessagePackObject]
public class LevelDataObject
{
    [ProtoMember(1)]
    [Key(0)]
    public int Level_Index;

    [Key(1)]
    [ProtoMember(2)]
    public float MinVelocity;

    [Key(2)]
    [ProtoMember(3)]
    public float MaxVelocity;

    [Key(3)]
    [ProtoMember(4)]
    public float MaxAngularVelocity;

    [Key(4)]
    [ProtoMember(5)]
    public List<NumberAsteroidData> ListAsteroid = new List<NumberAsteroidData>();

    [Key(5)]
    [ProtoMember(6)]
    public bool HasBoss;

    [Key(6)]
    [ProtoMember(7)]
    public int BossType;
}

[Serializable]
[ProtoContract]
[MessagePackObject]
public class DataLevel 
{
    [ProtoMember(1)]
    [Key(0)]
    public List<LevelDataObject> data_level = new List<LevelDataObject>();
}

public class GenerateData : MonoBehaviour
{
    public int amount;

    private string pathJson;
    private string pathProto;
    private string pathMsg;
    private string pathProtoGoogle;

    private string jsonFile;

    public bool isGenerating;
    public TextWriter tsw;
    private DataLevel listData = new DataLevel();

    [HideInInspector] public DataLevel listDataToLoading;

    public void Generate()
    {
        File.WriteAllText(pathJson, String.Empty);

        File.AppendAllText(pathJson, "{" + "\"data_level\":");
        File.AppendAllText(pathJson, "[");
        for (int i = 0; i < amount; i++)
        {
            UnityEngine.Debug.Log(i);
            LevelDataObject dataObject = new LevelDataObject();

            dataObject.Level_Index = i + 1;
            dataObject.MinVelocity = UnityEngine.Random.Range(3, 5);
            dataObject.MaxVelocity = UnityEngine.Random.Range(10, 15);
            dataObject.MaxAngularVelocity = 7;


            for (int j = 0; j < 3; j++)
            {
                NumberAsteroidData asteroidData = new NumberAsteroidData();
                asteroidData.Type = j + 1;
                asteroidData.Number = UnityEngine.Random.Range(1, 5);

                dataObject.ListAsteroid.Add(asteroidData);
            }

            dataObject.HasBoss = UnityEngine.Random.Range(0, 1) == 1;
            dataObject.BossType = UnityEngine.Random.Range(1, 3);

            listData.data_level.Add(dataObject);

            jsonFile = JsonUtility.ToJson(dataObject);

            File.AppendAllText(pathJson, jsonFile);

            if (i < amount - 1)
            {
                File.AppendAllText(pathJson, ",");
            }
        }
        File.AppendAllText(pathJson, "]");
        File.AppendAllText(pathJson, "}");
    }

    public void LoadToDataObjectJson()
    {
        var startTime = DateTime.Now;
        Stopwatch sw = new Stopwatch();
        var startTick = Environment.TickCount;

        sw.Start();
        listDataToLoading.data_level.Clear();
        listDataToLoading = JsonUtility.FromJson<DataLevel>(File.ReadAllText(pathJson));
        sw.Stop();

        UnityEngine.Debug.Log(listDataToLoading.data_level.Count);
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
        var elapsed = (DateTime.Now - startTime).Milliseconds;
        var elapsed2 = Environment.TickCount - startTick;

        UnityEngine.Debug.Log("JSON" + ": " + elapsed);
        UnityEngine.Debug.Log("JSON2" + ": " + elapsed2);
    }

    public void LoadToDataObjectProto()
    {

        var startTime = DateTime.Now;
        Stopwatch sw = new Stopwatch();
        var startTick = Environment.TickCount;

        sw.Start();
        listDataToLoading.data_level.Clear();
        using FileStream file = File.OpenRead(pathProto);
        listDataToLoading = Serializer.Deserialize<DataLevel>(file);
        sw.Stop();

        UnityEngine.Debug.Log(listDataToLoading.data_level.Count);
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
        var elapsed = (DateTime.Now - startTime).Milliseconds;
        var elapsed2 = Environment.TickCount - startTick;

        UnityEngine.Debug.Log("PROTO" + ": " + elapsed);
        UnityEngine.Debug.Log("PROTO2" + ": " + elapsed2);
    }

    public void LoadToDataObjectMsgPack()
    {
        var startTime = DateTime.Now;
        Stopwatch sw = new Stopwatch();
        var startTick = Environment.TickCount;

        sw.Start();
        listDataToLoading.data_level.Clear();
        using var fileStream = File.OpenRead(pathMsg);
        listDataToLoading = MessagePackSerializer.Deserialize<DataLevel>(fileStream);
        sw.Stop();

        UnityEngine.Debug.Log(listDataToLoading.data_level.Count);
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
        var elapsed = (DateTime.Now - startTime).Milliseconds;
        var elapsed2 = Environment.TickCount - startTick;

        UnityEngine.Debug.Log("MSG" + ": " + elapsed);
        UnityEngine.Debug.Log("MSG2" + ": " + elapsed2);
    }

    public void ExportProtoBuf()
    {
        using var file = new FileStream(pathProto, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        Serializer.Serialize(file, listData);
    }

    public void ExportMsgPack()
    {
        using var fileStream = new FileStream(pathMsg, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        MessagePackSerializer.Serialize(fileStream, listData);

        //Debug.Log(MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(listData)));
    }

    // Start is called before the first frame update
    void Start()
    {
        listDataToLoading = new DataLevel();
        listDataToLoading.data_level.Clear();
        pathJson = "Assets/database_level.json";
        pathProto = "Assets/database_level_proto.bin";
        pathProtoGoogle = "Assets/database_level_protogg.dat";
        pathMsg = "Assets/database_level_msg.bin";
    }
}
