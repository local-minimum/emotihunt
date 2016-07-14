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
    public int nCorners = 20;

    [SerializeField, Range(0, 2)]
    public float aheadCost = 1;

    [SerializeField, Range(0, 9)]
    public int minDistance;

    [SerializeField]
    UICornerMarker cornerPrefab;

    [SerializeField]
    bool useAlpha = true;

    List<UICornerMarker> corners;

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

        double[,] I = ImageAnalysis.Convolve.Texture2Double(tex, useAlpha);
        harrisCorner.ConvolveAndApply(I, tex.width);
        ImageAnalysis.Coordinate[] coordinates = harrisCorner.LocateCornersAsCoordinates(nCorners, aheadCost, minDistance);
        SetEmojiData(coordinates, I, tex.width);
        MarkCorners(coordinates, (tex.width - harrisCorner.ResponseStride) / 2);
        button.interactable = true;
    }

    void MarkCorners(ImageAnalysis.Coordinate[] coordinates, int offset)
    {
        UICornerMarker corner;

        for (int i=0; i< coordinates.Length; i++)
        {
            if (corners.Count <= i)
            {
                corner = Instantiate(cornerPrefab);
                corner.transform.SetParent(img.transform);
                corner.Setup();
                corners.Add(corner);
            } else
            {
                corner = corners[i];
            }
            //Debug.Log(cornerPoints[i, 0] + ", s=" + stride + ", o=" + offset);
            corner.SetCoordinate(coordinates[i]);
            //corner.SetColor(cornerPoints[i, 1]);
        }

        for (int i = coordinates.Length, cL = corners.Count; i<cL; i++)
        {
            corners[i].Showing = false;
        }
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

    public void SetDetectionCorners(Slider slider)
    {
        nCorners = Mathf.RoundToInt(slider.value);
    }
    
    public void SetDetectionAheadCost(Slider slider)
    {
        aheadCost = slider.value;
    }

    public void SetDetectionMinDist(Slider slider)
    {
        minDistance = Mathf.RoundToInt(slider.value);
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

            EmojiDB emojiDB = (EmojiDB)bformatter.Deserialize(stream);
            stream.Close();
            return emojiDB;


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

        HarrisCornerTexture cornerTex = new HarrisCornerTexture(cornerImg.sprite.texture, useAlpha);
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
        corners = GetComponentsInChildren<UICornerMarker>().ToList();
        foreach (UICornerMarker corner in corners)
        {
            corner.Showing = false;
        }
    }  
}
