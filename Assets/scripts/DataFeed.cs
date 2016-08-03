using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataFeed<T> {

    string location;

    private DataFeed()
    {
        location = "";
    }

    public DataFeed(string storageLocation)
    {
        location = storageLocation;
    }

    public void Append(object obj)
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
        byte[] sizeBuffer = new byte[4];
        int curIndex = 0;
        int pos = 0;
        int end = index + size;

        using (FileStream f = File.Open(location, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader br = new BinaryReader(f))
            {
                while (curIndex < end)
                {
                    int read = br.Read(sizeBuffer, pos, 4);
                    if (read != 4)
                    {
                        throw new Exception("Data corrupt, can't read size");
                    }
                    int dataSize = GetBytesAsInt(sizeBuffer);
                    pos += 4;

                    if (curIndex >= index)
                    {

                        byte[] dataBuffer = new byte[dataSize];
                        read = br.Read(dataBuffer, pos, dataSize);
                        if (read != dataSize)
                        {
                            throw new Exception("Data corrupt, can't read serialized object");
                        }

                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Binder = new VersionDeserializationBinder();
                        using (MemoryStream ms = new MemoryStream()) {
                            objects.Add((T) bformatter.Deserialize(ms));
                        }

                    }
                    pos += dataSize;
                    curIndex++;
                }
            }
        }
        return objects;
    }

}
