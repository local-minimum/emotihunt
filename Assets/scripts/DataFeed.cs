using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataFeed<T>
{

    string location;

    private DataFeed()
    {
        location = "";
    }

    public DataFeed(string storageLocation)
    {
        location = storageLocation;
    }

    public void Append(T obj)
    {
        //Serialize the object
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, obj);
        ms.Flush();
        ms.Position = 0;

        //Get binary representations
        byte[] serializedObj = ms.ToArray();

        //Create container
        byte[] bytes = new byte[4 + serializedObj.Length];

        //Add size of serialized object to first 32 positions
        Array.Copy(GetIntAsBytes(serializedObj.Length), bytes, 4);

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
    }


    public static byte[] GetIntAsBytes(int n)
    {
        return BitConverter.GetBytes(n);
    }

    public static int GetBytesAsInt(byte[] b)
    {
        return BitConverter.ToInt32(b, 0);
    }

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
                        int dataSize = GetBytesAsInt(sizeBuffer);
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
                        if (f.Length == pos)
                        {
                            break;
                        }
                    }
                }
            }
        }
        return objects;
    }

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
                                int dataSize = GetBytesAsInt(sizeBuffer);
                                pos += 4;
                                long sought = br.BaseStream.Seek(dataSize, SeekOrigin.Current);
                                pos += dataSize;
                                curIndex++;
                                if (sought != dataSize || pos >= f.Length)
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
}