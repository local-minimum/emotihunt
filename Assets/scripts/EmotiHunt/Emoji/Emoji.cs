﻿using UnityEngine;
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
    string baseURI = "http://local-minimum.unknownincubator.com";

    static string dbLocation;

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
        ResetSnapStatuses();
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
        if (dbLocation == null || dbLocation == "")
        {
            dbLocation = Application.persistentDataPath + "/emoji.db";
        }
        EmojiDB emojiDB;

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
                emojiDB = CreateEmojiDb();
                emojiDB.ResetSnapStatuses();
                return emojiDB;
            }

            try
            {
                emojiDB = RequestStreamer.Deserialize<EmojiDB>(stream);
                Debug.Log(string.Format("Loaded emojis at {0}, version {1}", EmojiDB.dbLocation, emojiDB.versionId));
            }
            catch (Exception ex)
            {

                if (ex is ArgumentException || ex is EndOfStreamException)
                {
                    emojiDB = CreateEmojiDb();
                    emojiDB.ResetSnapStatuses();

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
            emojiDB = CreateEmojiDb();
            emojiDB.ResetSnapStatuses();
            return emojiDB;

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

    private EmojiDB()
    {
        if (dbLocation == null || dbLocation == "")
        {
            dbLocation = Application.persistentDataPath + "/emoji.db";
        }
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

    public IEnumerable<KeyValuePair<string, float>> Update()
    {
        if (Debug.isDebugBuild)
        {
            Debug.developerConsoleVisible = true;            
        }
        
        RequestStreamer response = RequestStreamer.Create(baseURI + "/emojihunt/version");
        string waitChars = "-/|\\";
        int charPos = -1;
        while (!response.isDone)
        {
            Debug.Log(response.Poll().ToString());
            charPos++;
            yield return new KeyValuePair<string, float>("Checking version: " + waitChars[charPos % waitChars.Length], 0);
        }

        if (!response.Success)
        {
            yield return new KeyValuePair<string, float>("Error checking version", 0);
            Debug.LogWarning("Connection error: " + response.errors);
            yield break;
        }
        Debug.Log(response.text);

        long onlineVersion = long.Parse(response.text);
        if (onlineVersion > versionId)
        {

            Debug.Log(string.Format("Replacing emoji version {0} with {1}", versionId, onlineVersion));
            yield return new KeyValuePair<string, float>("Downloading data...", 0);

            response = RequestStreamer.Create(baseURI + "/emojihunt/data");
            
            while (!response.isDone)
            {
                Debug.Log(response.Poll());
                if (response.downloading)
                {
                    yield return new KeyValuePair<string, float>(string.Format("Downloaded {0:00%}", response.progress), response.progress);
                }
            }

            if (!response.Success)
            {
                yield return new KeyValuePair<string, float>("Error while updating", 0);
                yield break;
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
                yield return new KeyValuePair<string, float>("Saving local copy", 1);
                SaveEmojiDB(this);
            }
            yield return new KeyValuePair<string, float>(updated ? "Updated emojis!" : "Failed to update!", 1);
        }
    }

    static string prefKeyPattern = "Emoji.{0}";

    public void ResetSnapStatuses()
    {
        foreach (string eName in emojis.Select(e => e.emojiName))
        {
            string key = string.Format(prefKeyPattern, eName);
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }

    public int Taken
    {
        get
        {
            int n = 0;
            for (int i = 0, l = emojis.Count; i < l; i++)
            {
                if (HasBeenPhotographed(emojis[i].emojiName))
                {
                    n++;
                }
            }
            return n;

        }
    }

    public int Remaining
    {
        get
        {
            int n = 0;
            for (int i=0, l=emojis.Count; i< l; i++)
            {
                if (!HasBeenPhotographed(emojis[i].emojiName))
                {
                    n++;
                }
            }
            return n;
        }
    }

    public bool HasBeenPhotographed(string eName)
    {
        return PlayerPrefs.GetInt(string.Format(prefKeyPattern, eName), 0) == 1;
    }

    public void SetPhotographed(string eName)
    {
        PlayerPrefs.SetInt(string.Format(prefKeyPattern, eName), 1);
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
        versionId = template.versionId;
        ResetSnapStatuses();
        if (!Valid)
            Debug.LogError("Data has been tampered with");

    }
}