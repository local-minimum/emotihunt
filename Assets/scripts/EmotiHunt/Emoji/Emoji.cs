using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


public sealed class VersionDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            Type typeToDeserialze = null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;
            typeToDeserialze = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            return typeToDeserialze;
        }
        return null;
    }
}


[Serializable]
public class Emoji
{
    public string emojiName;
    public string secret;    
    public string hash;

    public double[,] pixels;
    public int pixelStride;
    public int height;

    public Vector2Surrogate[] corners;
    

}

[Serializable]
public class EmojiDB: ISerializable
{

    static string dbLocation = Application.persistentDataPath + "/emoji.db";

    List<Emoji> emojis = new List<Emoji>();
    string checksum;
    long versionId;

    public long Version
    {
        get
        {
            return versionId;
        }
    }

    public string Names
    {
        get
        {
            return string.Join(", ", emojis.Select(e => e.emojiName).ToArray());
        }
    }


    public void Set(Emoji emoji)
    {
        var db = DB;
        db[emoji.emojiName] = emoji;
        emojis = db.Values.ToList();
        checksum = CalculateChecksum();
        versionId++;
    }

    public Dictionary<string, Emoji> DB {
        get
        {
            Dictionary<string, Emoji> db = new Dictionary<string, Emoji>();
            foreach (Emoji emoji in emojis)
            {
                if (emoji != null)
                {
                    db[emoji.emojiName] = emoji;                    
                }
            }
            return db;
        }

        set
        {
            var db = DB;         
            foreach (KeyValuePair<string, Emoji> kvp in value)
            {
                db[kvp.Key] = kvp.Value;
            }
            emojis = db.Values.ToList();
            checksum = CalculateChecksum();
        }
    }

    public bool Valid
    {
        get
        {
            return checksum == CalculateChecksum() && !string.IsNullOrEmpty(checksum);
        }
    }


    public static EmojiDB LoadEmojiDB()
    {
        try
        {
            Debug.Log(dbLocation);
            Stream stream = File.Open(dbLocation, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            EmojiDB emojiDB;
            try
            {
                emojiDB = (EmojiDB)bformatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex)
            {

                if (ex is ArgumentException || ex is EndOfStreamException)
                {
                    stream.Close();
                    emojiDB = CreateEmojiDb();
                }
                else
                {
                    throw;
                }
            }
            return emojiDB;


        }
        catch (FileNotFoundException)
        {
            return CreateEmojiDb();
        }
    }

    public static EmojiDB CreateEmojiDb()
    {
        return new EmojiDB();        
    }

    public static void SaveEmojiDB(EmojiDB db)
    {
        Stream stream = File.Open(dbLocation, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, db);
        stream.Close();
    }

    string CalculateChecksum()
    {
        return "";
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("emojis", emojis);
        info.AddValue("checksum", checksum);
        info.AddValue("versionId", versionId);

    }

    public EmojiDB()
    {
        emojis = new List<Emoji>();
        checksum = null;
        versionId = -1;
    }

    public EmojiDB(SerializationInfo info, StreamingContext ctxt)
    {
        emojis = (List<Emoji>)info.GetValue("emojis", typeof(List<Emoji>));
        checksum = (string)info.GetValue("checksum", typeof(string));
        try {
            versionId = (long)info.GetValue("versionId", typeof(long));
        } catch (SerializationException)
        {
            versionId = 0;
        }

        if (!Valid)
            Debug.LogError("Data has been tampered with");
    }

    public IEnumerable<string> Update()
    {
        string baseURI = "http://212.85.82.101:5050";
        var response = new WWW(baseURI + "/emoji/version");
        while (!response.isDone)
        {
            yield return "Checking version...";
        }

        if (response.error != "" && response.error != null)
        {
            yield return "Error checking version ";
            Debug.LogWarning(response.error);
            yield break;
        }

        long onlineVersion = long.Parse(response.text);
        if (onlineVersion > versionId)
        {

            yield return "Downloading data...";
            Stream s = GetDataStream(baseURI + "/emoji/download");
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            bool updated = false;

            try
            {
                
                //Stream s = GenerateStreamFromString(response.text);
                
                var newEmojiDB = (EmojiDB)bformatter.Deserialize(s);
                Update(newEmojiDB);
                updated = true;    
                            
            } catch (Exception)
            {
                
                Debug.LogWarning("Update failed");
                throw;
            }

            yield return updated ? "Updated emojis!" : "Failed to update!";
        }
    }

    Stream GetDataStream(string URI)
    {
        Debug.Log("requesting: "  + URI);
        var request = System.Net.WebRequest.Create(URI);
        var response = request.GetResponse();
        Debug.Log(string.Join(", ", response.Headers.AllKeys));
        return response.GetResponseStream();
        /*                   
        BinaryReader reader = new BinaryReader(response.GetResponseStream());
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        long size = 0;
        //Until size is correct instead;
        while (reader.PeekChar() != -1)
        {
            
            writer.Write(reader.ReadByte());
            size++;
        }
        Debug.Log(size);
        writer.Flush();
        stream.Position = 0;
        return stream;
        */
    }

    public Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(Convert.FromBase64String(s));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public void Update(EmojiDB template)
    {
        emojis = template.emojis;
        checksum = template.checksum;
        template.versionId = versionId;

        if (!Valid)
            Debug.LogError("Data has been tampered with");

    }
}
