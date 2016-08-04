using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System.Linq;

public class EditorUI : MonoBehaviour {

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

    [SerializeField]
    bool pad = true;

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

        double[,] sourceImg = ImageAnalysis.Convolve.Texture2Double(tex, useAlpha);
        double[,] I = ImageAnalysis.Convolve.Texture2Double(tex, harrisCorner.width, harrisCorner.height, useAlpha);
        foreach(float f in harrisCorner.Convolve(I, harrisCorner.width))
        {
            Debug.Log(f);
        }
        harrisCorner.SetPixelsVisible();
        harrisCorner.ApplyTargetToTexture(harrisCorner.Texture);
        ImageAnalysis.Coordinate[] coordinates = harrisCorner.LocateCornersAsCoordinates(nCorners, aheadCost, minDistance);
        int offset = (tex.width - harrisCorner.ResponseStride) / 2;
        Debug.Log("Offset: " + offset);
        SetEmojiData(coordinates, sourceImg, tex, offset);
        MarkCorners(coordinates, offset);
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
                corner.Setup(img);
                corners.Add(corner);
            } else
            {
                corner = corners[i];
            }
            //Debug.Log(cornerPoints[i, 0] + ", s=" + stride + ", o=" + offset);
            corner.SetCoordinate(coordinates[i], offset);
            //corner.SetColor(cornerPoints[i, 1]);
        }

        for (int i = coordinates.Length, cL = corners.Count; i<cL; i++)
        {
            corners[i].Showing = false;
        }
    }

    void SetEmojiData(ImageAnalysis.Coordinate[] corners, double[,] pixels, Texture2D tex, int offset)
    {
        currentEmoji = new Emoji();
        currentEmoji.emojiName = emojiName;
        currentEmoji.corners = Vector2Surrogate.CreateArray(ImageAnalysis.Math.CoordinatesToTexRelativeVector2(corners, tex, offset));
        currentEmoji.pixels = pixels;
        currentEmoji.pixelStride = tex.width;
        currentEmoji.height = tex.height;

        //TODO: Something more useful
        currentEmoji.secret = GetHashCode().ToString();
        currentEmoji.hash = CreateHash(currentEmoji.corners, currentEmoji.secret);
       
    }

    public void SaveEmoji(Button button)
    {
        Detector.SetEmoji(currentEmoji);
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

    static string CreateHash(Vector2Surrogate[] corners, string secret)
    {
        return secret;
    }

    HarrisCornerTexture GetCornerTexture(Texture2D source)
    {
        Texture2D target = new Texture2D(source.width, source.height);
        cornerImg.sprite = Sprite.Create(
            target, 
            new Rect(0, 0, source.width, source.height), 
            Vector2.one * 0.5f);

        cornerImg.sprite.name = string.Format("Result img ({0}, {1}), {2}, {3}", source.width, source.height, useAlpha ? "Alpha included" : "No alpha", pad ? "Padded" : "Shrinking");

        HarrisCornerTexture cornerTex = new HarrisCornerTexture(cornerImg.sprite.texture, useAlpha, pad);
        return cornerTex;
    }

    public void Load(Button button)
    {
        img.sprite = null;
    }  
    
    public void Start()
    {
        Detector.emojiDB = EmojiDB.LoadEmojiDB();
        Debug.Log("DB Version: " + Detector.emojiDB.Version);
        string names = Detector.emojiDB.Names;
        Debug.Log(string.IsNullOrEmpty(names) ? "Empty DB" : names);
        corners = GetComponentsInChildren<UICornerMarker>().ToList();
        foreach (UICornerMarker corner in corners)
        {
            corner.Showing = false;
        }
    }  
}
