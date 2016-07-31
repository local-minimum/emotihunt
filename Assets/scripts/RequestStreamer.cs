using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections.Generic;
using System.Net;


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

public enum RequestStreamerState { Setup, Initiating, Downloading, FailsafeDownloading, Terminated, Error, Waiting };

public class RequestStreamer
{

    public static bool debugMode = true;

    Stream _stream;
    public Stream stream
    {
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
            }
            else
            {
                return 0;
            }
        }
    }


    public bool downloading
    {
        get
        {
            return _downloaded < _size && _downloaded >= 0;
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

    string URI;

    string _errors = "";

    public string errors
    {

        get { return _errors; }

    }

    public bool Success
    {
        get
        {
            return isDone && _errors == "";
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


    public static RequestStreamer Create(string URI, float timeOut = 5f)
    {
        RequestStreamer obj = new RequestStreamer();
        obj.URI = URI;
        obj.worker = obj.Worker(timeOut);
        return obj;
    }

    public void Abort()
    {

    }

    public RequestStreamerState Poll()
    {
        if (worker.MoveNext())
        {
            return worker.Current;
        }
        return RequestStreamerState.Terminated;
    }

    IEnumerator<RequestStreamerState> worker;

    IEnumerator<RequestStreamerState> Worker(float timeOut)
    {

        yield return RequestStreamerState.Initiating;

        var request = WebRequest.Create(URI);
        request.Timeout = Mathf.RoundToInt(timeOut * 1000);

        yield return RequestStreamerState.Initiating;
        
        WebResponse response = null;
        try
        {
            response = request.GetResponse();
        }
        catch (Exception)
        {
            _errors = string.Format("{0} connection refused", URI);
            _isDone = true;
        }

        if (_isDone)
        {
            yield return RequestStreamerState.Error;
            yield break;
        }

        yield return RequestStreamerState.Initiating;

        SetResponseHeaders(response.Headers);
        SetContentLength();

        yield return RequestStreamerState.Setup;
        float startTime = Time.realtimeSinceStartup;
        if (size > 0)
        {
            var _responseStream = response.GetResponseStream();
            _stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(_stream);

            int readSize = 1024 * 128;
            byte[] buffer = new byte[readSize];
            _downloaded = 0;

            while (downloading && !_abort)
            {

                int read = _responseStream.Read(buffer, 0, readSize);
                writer.Write(buffer, 0, read);
                if (read > 0)
                {
                    startTime = Time.realtimeSinceStartup;
                    _downloaded += read;
                    yield return RequestStreamerState.Downloading;
                }
                else if (Time.realtimeSinceStartup - startTime > timeOut)
                {
                    _errors = "Connection time out";
                    _abort = true;
                    yield return RequestStreamerState.Error;
                    _isDone = true;
                    yield break;
                }
                else
                {
                    yield return RequestStreamerState.Waiting;
                }


            }

            writer.Flush();
            _stream.Position = 0;

        }
        else
        {
            yield return RequestStreamerState.FailsafeDownloading;
            _stream = response.GetResponseStream();
        }

        _isDone = true;
    }

    void SetContentLength(string key = "Content-Length")
    {
        _size = int.Parse(headers[key]);
    }

    void SetResponseHeaders(WebHeaderCollection headers)
    {
        _headers.Clear();
        foreach (string key in headers.AllKeys)
        {
            _headers[key] = headers.Get(key);
        }
    }

    public T Deserialize<T>()
    {
        if (Success)
        {
            return Deserialize<T>(stream);
        }
        else
        {
            throw new Exception("Failed while downloading");
        }
    }

    public string text
    {
        get
        {
            if (isDone)
            {
                _stream.Position = 0;
                return new StreamReader(_stream).ReadToEnd();
            }
            else
            {
                throw new Exception("Failed while downloading");
            }
        }
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
