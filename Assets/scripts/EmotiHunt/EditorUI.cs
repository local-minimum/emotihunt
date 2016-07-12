﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorUI : MonoBehaviour {

#if UNITY_EDITOR
    static string dbLocation = "Assets/data/emoji.db";
#endif

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
        SetEmojiData(ImageAnalysis.Math.ConvertCoordinate(corners, harrisCorner.ResponseStride), tex.GetPixels(), tex.width);
        button.interactable = true;
    }

    void SetEmojiData(ImageAnalysis.Coordinate[] corners, Color[] pixels, int stride)
    {
        currentEmoji = ScriptableObject.CreateInstance<Emoji>();
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
        Dictionary<string, Emoji> db = LoadEmojiDB();
        db[emoji.emojiName] = emoji;
        SaveEmojiDB(db);
    }

    static Dictionary<string, Emoji> LoadEmojiDB()
    {
        Dictionary<string, Emoji> db = new Dictionary<string, Emoji>();
#if UNITY_EDITOR
        int i = 0;

        EmojiDB emojiDB = AssetDatabase.LoadAssetAtPath<EmojiDB>(dbLocation);
        foreach (Emoji emoji in emojiDB.emojis)
        {
            if (emoji != null)
            {
                db[emoji.emojiName] = emoji;
                i++;
            }
        }
        Debug.Log("Loaded Emoji DB, length = " + i);
#endif
        return db;
    }

    static void CreateEmojiDb()
    {
#if UNITY_EDITOR       
        EmojiDB baseObj = ScriptableObject.CreateInstance<EmojiDB>();
        baseObj.name = "Emoji DB";
        AssetDatabase.CreateAsset(baseObj, dbLocation);
#endif
    }
    


    static void SaveEmojiDB(Dictionary<string, Emoji> db)
    {
#if UNITY_EDITOR
        EmojiDB emojiDB = AssetDatabase.LoadAssetAtPath<EmojiDB>(dbLocation);
        emojiDB.emojis = db.Values.ToList();

        //TODO: All confused about scriptable objects saving
        AssetDatabase.SaveAssets();
#endif
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
}
