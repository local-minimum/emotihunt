﻿using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

            RequestStreamer sResponse = RequestStreamer.Create(baseURI + "/emoji/download");
            
            while (!sResponse.isDone)
            {
                Debug.Log("Progress: " + sResponse.progress);
                Debug.Log("Downloaded: " + sResponse.downloaded);
                Thread.Sleep(10);
                yield return string.Format("Downloaded {0:00%}", sResponse.progress);
            }

            bool updated = false;

            try
            {
                
                EmojiDB newEmojiDB = sResponse.Deserialize<EmojiDB>();
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

public class RequestStreamer
{

    Stream _stream;
    public Stream stream {
        get
        {
            return _stream;
        }
    }

    int _size = -1;
    public int size
    {
        get
        {
            return _size;
        }
    }

    bool _isDone = false;
    public bool isDone
    {
        get
        {
            return _isDone;
        }
    }

    public float progress
    {
        get
        {
            if (downloading)
            {
                return (float)_downloaded / _size;
            } else
            {
                return 0;
            }
        }
    }


    public bool downloading
    {
        get
        {
            return _downloaded < _size && _downloaded > 0;
        }
    }

    int _downloaded = 0;
    public int downloaded
    {
        get
        {
            return _downloaded;
        }
    }

    int _pollFrequency = 40;

    string URI;
    Thread thread;

    public bool started
    {
        get
        {
            return thread != null;
        }
    }

    private RequestStreamer()
    {

    }

    Dictionary<string, string> _headers = new Dictionary<string, string>();

    public Dictionary<string, string> headers
    {
        get
        {
            return _headers;
        }
    }

    bool _abort = false;

    public static RequestStreamer Create(string URI)
    {
        RequestStreamer obj = new RequestStreamer();
        obj.URI = URI;
        obj.thread = new Thread(new ThreadStart(obj.Worker));
        
        obj.thread.Start();
        return obj;
    }

    public void Abort()
    {
        _abort = true;
        if (!downloading)
        {
            thread.Abort();
        }
    }

    void Worker()
    {
        
        var request = System.Net.WebRequest.Create(URI);
        var response = request.GetResponse();
        
        SetResponseHeaders(response.Headers);
        SetContentLength();

        if (size > 0)
        {
            BinaryReader reader = new BinaryReader(response.GetResponseStream());
            _stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(_stream);
            
            int readSize = 4048;
            byte[] buffer = new byte[readSize];
            _downloaded = 0;
            while (_downloaded < size || _abort)
            {

                int read = reader.Read(buffer, 0, readSize);
                writer.Write(buffer, 0, read);
                _downloaded += read;

            }

            writer.Flush();
            _stream.Position = 0;

        }
        else
        {
            _stream = response.GetResponseStream();
        }
        _isDone = true;
    }

    void SetContentLength(string key = "Content-Length")
    {
        if (headers.ContainsKey(key))
        {
            _size = int.Parse(headers[key]);
        } else
        {
            Debug.LogWarning(string.Join(", ", headers.Keys.ToArray()));
        }
        _size = -1;
    }

    void SetResponseHeaders(System.Net.WebHeaderCollection headers)
    {
        _headers.Clear();
        foreach (string key in headers.AllKeys)
        {
            _headers[key] = headers.Get(key);
        }
    }

    public T Deserialize<T>()
    {
        return Deserialize<T>(stream);
    }

    public static T Deserialize<T>(Stream stream)
    {
        T obj;
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        
        try
        {

            obj = (T)bformatter.Deserialize(stream);
            CloseIOStream(stream);

        }
        catch (Exception)
        {
            CloseIOStream(stream);
            throw;                
        }

        return obj;
    }

    static void CloseIOStream(Stream s)
    {
        s.Close();
    }
}
