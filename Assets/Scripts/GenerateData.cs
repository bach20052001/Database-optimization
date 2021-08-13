using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using ProtoBuf;
using MessagePack;
using System.Diagnostics;
using FlatBuffers;
using AsteraX;
using UnityEngine.UI;

//using System.Diagnostics;

[Serializable]
[ProtoContract]
[MessagePackObject]
public class NumberAsteroidData
{
    [ProtoMember(1)]
    [Key(0)]
    public int type;

    [ProtoMember(2)]
    [Key(1)]
    public int number;
}

[Serializable]
[ProtoContract]
[MessagePackObject]
public class LevelDataObject
{
    [ProtoMember(1)]
    [Key(0)]
    public int levelindex;

    [Key(1)]
    [ProtoMember(2)]
    public float minvelocity;

    [Key(2)]
    [ProtoMember(3)]
    public float maxvelocity;

    [Key(3)]
    [ProtoMember(4)]
    public float maxangularvelocity;

    [Key(4)]
    [ProtoMember(5)]
    public List<NumberAsteroidData> listasteroid = new List<NumberAsteroidData>();

    [Key(5)]
    [ProtoMember(6)]
    public bool hasboss;

    [Key(6)]
    [ProtoMember(7)]
    public int bosstype;
}

[Serializable]
[ProtoContract]
[MessagePackObject]
public class DataLevel
{
    [ProtoMember(1)]
    [Key(0)]
    public List<LevelDataObject> data = new List<LevelDataObject>();
}

public class GenerateData : MonoBehaviour
{
    public int amount;

    public Text elapseTimeDisplay;

    private string pathJson;
    private string pathProto;
    private string pathMsg;

    private string jsonFile;
    private AsteraX.DataLevel dataLevel =  new AsteraX.DataLevel();
    public bool isGenerating;
    public TextWriter tsw;
    private DataLevel listData = new DataLevel();

    [HideInInspector] public DataLevel listDataToLoading;

    public void Generate()
    {
        File.WriteAllText(pathJson, String.Empty);

        File.AppendAllText(pathJson, "{" + "\"data\":");
        File.AppendAllText(pathJson, "[");
        for (int i = 0; i < amount; i++)
        {
            LevelDataObject dataObject = new LevelDataObject();

            dataObject.levelindex = i + 1;
            dataObject.minvelocity = UnityEngine.Random.Range(3, 5);
            dataObject.maxvelocity = UnityEngine.Random.Range(10, 15);
            dataObject.maxangularvelocity = 7;


            for (int j = 0; j < 3; j++)
            {
                NumberAsteroidData asteroidData = new NumberAsteroidData();
                asteroidData.type = j + 1;
                asteroidData.number = UnityEngine.Random.Range(1, 5);

                dataObject.listasteroid.Add(asteroidData);
            }

            dataObject.hasboss = UnityEngine.Random.Range(0, 1) == 1;
            dataObject.bosstype = UnityEngine.Random.Range(1, 3);

            listData.data.Add(dataObject);

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
        listDataToLoading = new DataLevel();
        listDataToLoading.data.Clear();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        listDataToLoading.data.Clear();
        listDataToLoading = JsonUtility.FromJson<DataLevel>(File.ReadAllText(pathJson));
        sw.Stop();

        elapseTimeDisplay.text = sw.ElapsedMilliseconds.ToString() + "ms (JSON)";
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }

    public void LoadToDataObjectProto()
    {
        listDataToLoading = new DataLevel();
        listDataToLoading.data.Clear();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        listDataToLoading.data.Clear();
        using FileStream file = File.OpenRead(pathProto);
        listDataToLoading = Serializer.Deserialize<DataLevel>(file);
        sw.Stop();

        elapseTimeDisplay.text = sw.ElapsedMilliseconds.ToString() + "ms (ProtoBuffer)";
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }   

    public void LoadToDataObjectMsgPack()
    {
        listDataToLoading = new DataLevel();
        listDataToLoading.data.Clear();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        listDataToLoading.data.Clear();
        using var fileStream = File.OpenRead(pathMsg);
        listDataToLoading = MessagePackSerializer.Deserialize<DataLevel>(fileStream);
        sw.Stop();

        elapseTimeDisplay.text = sw.ElapsedMilliseconds.ToString();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }

    public void LoadToDataObjectFlat()
    {
        dataLevel = new AsteraX.DataLevel();

        Stopwatch sw = new Stopwatch();
        sw.Start();

        byte[] data = File.ReadAllBytes("Assets/database_level.bin");

        ByteBuffer bb = new ByteBuffer(data);

        dataLevel = AsteraX.DataLevel.GetRootAsDataLevel(bb);

        sw.Stop();

        elapseTimeDisplay.text = sw.ElapsedMilliseconds.ToString() + "ms (FlatBuffer)";
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
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
    }

    // Start is called before the first frame update
    void Start()
    {
        listDataToLoading = new DataLevel();
        listDataToLoading.data.Clear();
        pathJson = "Assets/database_level.json";
        pathProto = "Assets/database_level_proto.bin";
        pathMsg = "Assets/database_level_msg.bin";
    }
}
