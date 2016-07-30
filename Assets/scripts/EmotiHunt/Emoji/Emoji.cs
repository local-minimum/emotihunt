using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;



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
            Stream stream = null;
            try
            {
               stream = File.Open(dbLocation, FileMode.Open);
            }  catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    Debug.LogWarning(string.Format("Access to '{0}' not allowed", dbLocation));
                } else if (ex is FileNotFoundException)
                {
                    Debug.LogWarning(string.Format("File '{0}' not found", dbLocation));
                } else if (ex is IOException) {
                    Debug.LogWarning(string.Format("I/O exception accessing '{0}'", dbLocation));
                } else {
                    throw;
                }
                return CreateEmojiDb();
            }

            EmojiDB emojiDB;
            try
            {
                emojiDB = RequestStreamer.Deserialize<EmojiDB>(stream);
            }
            catch (Exception ex)
            {

                if (ex is ArgumentException || ex is EndOfStreamException)
                {
                    emojiDB = CreateEmojiDb();
                }
                else
                {
                    throw;
                }
            }
            return emojiDB;


        }
        catch (Exception ex)
        {

            Debug.LogWarning(string.Format("Unexpected exception loading '{0}', {1}", dbLocation, ex));
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
        if (Debug.isDebugBuild)
        {
            Debug.developerConsoleVisible = true;            
        }
        string baseURI = "http://212.85.82.101:5050";
        RequestStreamer response = RequestStreamer.Create(baseURI + "/emoji/version");
        string waitChars = "-/|\\";
        int charPos = -1;
        while (!response.isDone)
        {
            Debug.Log(response.Poll().ToString());
            charPos++;
            yield return "Checking version: " + waitChars[charPos % waitChars.Length];
        }

        if (!response.Success)
        {
            yield return "Error checking version";
            Debug.LogWarning("Connection error: " + response.errors);
            yield break;
        }
        Debug.Log(response.text);

        long onlineVersion = long.Parse(response.text);
        if (onlineVersion > versionId)
        {

            yield return "Downloading data...";

            response = RequestStreamer.Create(baseURI + "/emoji/download");
            
            while (!response.isDone)
            {
                Debug.Log(response.Poll());
                if (response.downloading)
                {
                    yield return string.Format("Downloaded {0:00%}", response.progress);
                }
            }

            bool updated = false;

            try
            {
                
                EmojiDB newEmojiDB = response.Deserialize<EmojiDB>();
                Update(newEmojiDB);
                updated = true;    
                            
            } catch (Exception)
            {
                
                Debug.LogWarning("Update failed");
                throw;
            }

            if (updated)
            {
                yield return "Saving local copy";
                SaveEmojiDB(this);
            }
            yield return updated ? "Updated emojis!" : "Failed to update!";
        }
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