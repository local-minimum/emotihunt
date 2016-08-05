using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// A binary searchable file object serializer.
/// 
/// <remark>The VersionDeserializationBinder is house in the RequestStreamer module (Also really neat stuff)</remark>
/// 
/// <example>
/// Illustration of how to serialize an object.
/// <code>
/// [Serializable]
/// class Post {
///     public string text;
///     public DateTime date;
/// }
/// 
/// class Feed {
///     DataFeed<Post> _storage = new DataFeed<Post>("some/location/feed.bin");
///     void Append(Post post) {
///         _storage.Append(post);
///     }
/// }
/// </code>
/// </example>
/// 
/// </summary>
/// <typeparam name="T">Any serializable type. I.e. a class prefixed with [Serializable]</typeparam>
public class DataFeed<T>
{
    
    public delegate void FeedAppended(T item);

    /// <summary>
    /// Event that is fired when something is appended.
    /// 
    /// <remarks>
    /// Using the saves reading and deserializing the object and
    /// therefore saves some compute.
    /// </remarks>
    /// </summary>
    public event FeedAppended OnFeedAppended;

    string location;

    private DataFeed()
    {
        location = "";
    }

    public DataFeed(string storageLocation)
    {
        location = storageLocation;

        //Creates an empty file at the specified if it doesn't exist.
        if (!File.Exists(location))
        {
            using (FileStream f = File.Open(location, FileMode.Create, FileAccess.Write));
        }
    }

    /// <summary>
    /// Appends the item to the end of the feed-file
    /// </summary>
    /// <param name="item">Item to serialize into the file</param>
    public void Append(T item)
    {
        //Serialize the object
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, item);
        ms.Flush();
        ms.Position = 0;

        //Get binary representations
        byte[] serializedObj = ms.ToArray();

        //Create container
        byte[] bytes = new byte[4 + serializedObj.Length];

        //Add size of serialized object to first 32 positions
        Array.Copy(BitConverter.GetBytes(serializedObj.Length), bytes, 4);

        //Add serialized object
        Array.Copy(serializedObj, 0, bytes, 4, serializedObj.Length);

        //Write out serialized object at end of file
        using (FileStream f = File.Open(location, FileMode.Append, FileAccess.Write, FileShare.None))
        {
            using (BinaryWriter bw = new BinaryWriter(f))
            {
                bw.Write(bytes);
            }
        }

        if (OnFeedAppended != null)
        {
            OnFeedAppended(item);
        }
    }

    /// <summary>
    /// Reads a number of items into a list starting at a certain index.
    /// 
    /// Note: The returned list may be shorter than the requested size if
    /// the end of file is reached
    /// </summary>
    /// <param name="index">Starting index, included, 0=first</param>
    /// <param name="size">Number of items</param>
    /// <returns>A list of the requested items</returns>
    public List<T> Read(int index, int size)
    {

        List<T> objects = new List<T>();

        int curIndex = 0;
        long pos = 0;
        int end = index + size;

        using (FileStream f = File.Open(location, FileMode.Open, FileAccess.Read))
        {
            if (f.Length != 0)
            {

                using (BinaryReader br = new BinaryReader(f))
                {
                    while (curIndex < end)
                    {
                        byte[] sizeBuffer = br.ReadBytes(4);
                        if (sizeBuffer.Length != 4)
                        {
                            throw new DataMisalignedException(
                                string.Format(
                                    "File {0} had truncated SizeBuffer at index {1}, data position {2}, only {3} bytes (should have been 4)",
                                    location, curIndex, pos, sizeBuffer.Length));
                        }
                        int dataSize = BitConverter.ToInt32(sizeBuffer, 0);
                        pos += 4;

                        if (curIndex >= index)
                        {

                            byte[] dataBuffer = br.ReadBytes(dataSize);

                            if (dataBuffer.Length != dataSize)
                            {
                                throw new DataMisalignedException(
                                    string.Format(
                                        "File {0} had truncated Serialized Object at index {1}, data position {2}, only {3} bytes (should have been {4})",
                                        location, curIndex, pos, dataBuffer.Length, dataSize));

                            }

                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Binder = new VersionDeserializationBinder();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                BinaryWriter bw = new BinaryWriter(ms);
                                bw.Write(dataBuffer);
                                ms.Flush();
                                ms.Position = 0;
                                objects.Add((T)bformatter.Deserialize(ms));
                            }

                        }
                        else
                        {
                            br.BaseStream.Seek(dataSize, SeekOrigin.Current);
                        }
                        pos += dataSize;
                        curIndex++;
                        if (f.Length <= pos)
                        {
                            break;
                        }
                    }
                }
            }
        }
        return objects;
    }

    /// <summary>
    /// Browses through the items of the file.
    /// </summary>
    /// <returns>Enumberable of all the items</returns>
    public IEnumerable<T> Browse()
    {
        int curIndex = 0;
        long pos = 0;

        using (FileStream f = File.Open(location, FileMode.Open, FileAccess.Read))
        {
            if (f.Length != 0)
            {

                using (BinaryReader br = new BinaryReader(f))
                {
                    while (true)
                    {
                        byte[] sizeBuffer = br.ReadBytes(4);
                        if (sizeBuffer.Length != 4)
                        {
                            throw new DataMisalignedException(
                                string.Format(
                                    "File {0} had truncated SizeBuffer at index {1}, data position {2}, only {3} bytes (should have been 4)",
                                    location, curIndex, pos, sizeBuffer.Length));
                        }
                        int dataSize = BitConverter.ToInt32(sizeBuffer, 0);
                        pos += 4;

                        byte[] dataBuffer = br.ReadBytes(dataSize);

                        if (dataBuffer.Length != dataSize)
                        {
                            throw new DataMisalignedException(
                                string.Format(
                                    "File {0} had truncated Serialized Object at index {1}, data position {2}, only {3} bytes (should have been {4})",
                                    location, curIndex, pos, dataBuffer.Length, dataSize));

                        }

                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Binder = new VersionDeserializationBinder();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryWriter bw = new BinaryWriter(ms);
                            bw.Write(dataBuffer);
                            ms.Flush();
                            ms.Position = 0;
                            yield return (T) bformatter.Deserialize(ms);
                        }



                        pos += dataSize;
                        curIndex++;
                        if (f.Length <= pos)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Number of items in file.
    /// </summary>
    public int Count
    {
        get
        {
            int curIndex = 0;
            long pos = 0;

            try
            {
                using (FileStream f = File.Open(location, FileMode.Open, FileAccess.Read))
                {
                    if (f.Length != 0)
                    {
                        f.Position = 0;
                        using (BinaryReader br = new BinaryReader(f))
                        {
                            while (true)
                            {
                                byte[] sizeBuffer = br.ReadBytes(4);
                                if (sizeBuffer.Length != 4)
                                {
                                    throw new DataMisalignedException(
                                        string.Format(
                                            "File {0} had truncated SizeBuffer at index {1}, data position {2}, only {3} bytes (should have been 4)",
                                            location, curIndex, pos, sizeBuffer.Length));
                                }
                                int dataSize = BitConverter.ToInt32(sizeBuffer, 0);
                                pos += 4;
                                long filePos = br.BaseStream.Seek(dataSize, SeekOrigin.Current);
                                pos += dataSize;
                                curIndex++;
                                if (pos != filePos || pos >= f.Length)
                                {
                                    break;
                                }
                                
                            }

                        }
                    }
                }

            }
            catch (FileNotFoundException)
            {

            }
            return curIndex;
        }
    }

    /// <summary>
    /// The last item of the file
    /// </summary>
    public T Last
    {
        get
        {
            return Read(Count - 1, 1)[0];
        }
    }

    /// <summary>
    /// The first item of the file
    /// </summary>
    public T First
    {
        get
        {
            return Read(0, 1)[0];
        }
    }

    /// <summary>
    /// Wipes the content of the file
    /// </summary>
    public void Wipe()
    {
        using (FileStream f = File.Open(location, FileMode.Create, FileAccess.Write));
    }
}