using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System;

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

public class EditorUI : MonoBehaviour {

    static string dbLocation = "Assets/data/emoji.db";


    string emojiName = "";

    [SerializeField]
    Image img;

    [SerializeField]
    Image cornerImg;

    [SerializeField]
    Emoji currentEmoji;

    [SerializeField, Range(12, 42)]
    int nCorners = 20;

    [SerializeField, Range(0, 2)]
    float aheadCost = 1;

    [SerializeField, Range(0, 9)]
    int minDistance;

    public void SetName(Text text)
    {
        emojiName = text.text;
    }

    public void Serialize(Button button)
    {
        StartCoroutine(analyze(button));
    }

    IEnumerator<WaitForEndOfFrame> analyze(Button button) {

        Texture2D tex = img.sprite.texture;
        if (tex == null)
        {
            Debug.LogWarning("No image loaded");
            yield break;
        }
        if (emojiName == "")
        {
            Debug.LogWarning("Emoji has no name");
            yield break;
        }

        button.interactable = false;
        yield return new WaitForEndOfFrame();
        Debug.Log("Analysing");

        HarrisCornerTexture harrisCorner = GetCornerTexture(tex);

        double[,] I = ImageAnalysis.Convolve.Texture2Double(tex);
        harrisCorner.ConvolveAndApply(I, tex.width);
        int[,] corners = harrisCorner.LocateCorners(nCorners, aheadCost, minDistance);
        SetEmojiData(ImageAnalysis.Math.ConvertCoordinate(corners, harrisCorner.ResponseStride), I, tex.width);
        button.interactable = true;
    }

    void SetEmojiData(ImageAnalysis.Coordinate[] corners, double[,] pixels, int stride)
    {
        currentEmoji = new Emoji();
        currentEmoji.emojiName = emojiName;
        currentEmoji.corners = corners;
        currentEmoji.pixels = pixels;
        currentEmoji.pixelStride = stride;
        currentEmoji.height = pixels.Length / stride;

        //TODO: Something more useful
        currentEmoji.secret = GetHashCode().ToString();
        currentEmoji.hash = CreateHash(currentEmoji.corners, currentEmoji.secret);
       
    }

    public void SaveEmoji(Button button)
    {
        SetEmoji(currentEmoji);
    }

    static string CreateHash(ImageAnalysis.Coordinate[] corners, string secret)
    {
        return secret;
    }

    static void SetEmoji(Emoji emoji)
    {
        EmojiDB db = LoadEmojiDB();
        db.Set(emoji);
        SaveEmojiDB(db);
    }

    static EmojiDB LoadEmojiDB()
    {
        try {
            Stream stream = File.Open(dbLocation, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();

            /*try
            {*/
                EmojiDB emojiDB = (EmojiDB)bformatter.Deserialize(stream);
                stream.Close();
                return emojiDB;

            /*}
            catch (SerializationException)
            {
                Debug.LogError("Previous emoji database not deserializable or empty");
                stream.Close();
                return CreateEmojiDb();
            }*/
        } catch (FileNotFoundException)
        {
            return CreateEmojiDb();
        }
    }

    static EmojiDB CreateEmojiDb()
    {
        EmojiDB db = new EmojiDB();          
        return db;
    }
    

    static void SaveEmojiDB(Dictionary<string, Emoji> db)
    {
        var emojiDB = LoadEmojiDB();
        emojiDB.DB = db;
        SaveEmojiDB(emojiDB);
    }

    static void SaveEmojiDB(EmojiDB db)
    {
        Stream stream = File.Open(dbLocation, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, db);
        stream.Close();
    }

    HarrisCornerTexture GetCornerTexture(Texture2D source)
    {
        Texture2D target = new Texture2D(source.width, source.height);
        cornerImg.sprite = Sprite.Create(
            target, 
            new Rect(0, 0, source.width, source.height), 
            Vector2.one * 0.5f);

        HarrisCornerTexture cornerTex = new HarrisCornerTexture(cornerImg.sprite.texture);
        return cornerTex;
    }

    public void Load(Button button)
    {
        img.sprite = null;
    }  
    
    public void Start()
    {
        string names = LoadEmojiDB().Names;
        Debug.Log(string.IsNullOrEmpty(names) ? "Empty DB" : names);
    }  
}
