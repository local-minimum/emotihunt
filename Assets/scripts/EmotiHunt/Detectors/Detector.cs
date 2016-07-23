using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
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

public delegate void DetectorStatusEvent(Detector screen, DetectorStatus status);
public delegate void EmojiMatchEvent(int index, Vector2[] corners, Emoji emoji);

public enum DetectorStatus {Filming, Detecting, ShowingResults, Inactive};

[Serializable]
public struct Vector2Surrogate {

    public float x;
    public float y;

    public Vector2 V2 { get { return new Vector2(x, y); } set { x = value.x; y = value.y; } }

    public static Vector2Surrogate[] CreateArray(Vector2[] source)
    {
        int l = source.Length;
        Vector2Surrogate[] target = new Vector2Surrogate[l];
        for (int i=0; i< l; i++)
        {
            target[i].V2 = source[i];
        }

        return target;
    }
}

public static class Vector2Helpers
{
    public static Vector2[] ToVector2(this Vector2Surrogate[] source)
    {
        int l = source.Length;
        Vector2[] target = new Vector2[l];
        for (int i=0; i< l; i++)
        {
            target[i] = source[i].V2;
        }
        return target;
    }
}


public abstract class Detector : MonoBehaviour {

    public event EmojiMatchEvent OnMatchWithEmoji;
    public event DetectorStatusEvent OnDetectorStatusChange;

    [SerializeField]
    protected Image image;

    static string dbLocation = Application.persistentDataPath + "/emoji.db";

    protected bool working = false;
    protected bool showingResults = false;
    protected HarrisCornerTexture cornerTexture;
    protected double[,] I;

    [SerializeField, Range(100, 800)]
    protected int size = 400;

    [SerializeField, Range(10, 42)]
    int nCorners = 24;
    [SerializeField, Range(1, 4)]
    float aheadCost = 1.4f;
    [SerializeField, Range(0, 40)]
    int minDistance = 9;

    [SerializeField]
    UICornerMarker cornerPrefab;

    [SerializeField]
    protected bool debug;

    [SerializeField]
    EmojiProjection emojiProjectionPrefab;

    List<EmojiProjection> projections = new List<EmojiProjection>();

    List<Emoji> emojis = new List<Emoji>();

    List<UICornerMarker> cornerMarkers = new List<UICornerMarker>();

    protected float zoom = 1;
    protected Vector2[] corners;

    MobileUI mobileUI;

    public DetectorStatus Status
    {
        get
        {
            if (!enabled) {
                return DetectorStatus.Inactive;
            } else if (working)
            {
                return DetectorStatus.Detecting;
            } else if (showingResults)
            {
                return DetectorStatus.ShowingResults;
            } else
            {
                return DetectorStatus.Filming;
            }
        }
    }

    void Awake()
    {
        mobileUI = FindObjectOfType<MobileUI>();
        I = new double[size * size, 3];
        
    }

    void OnEnable()
    {
        SetupEmojis();
        mobileUI.OnSnapImage += StartEdgeDetection;
        mobileUI.OnCloseAction += HandleCloseEvent;
        mobileUI.OnZoom += HandleZoom;
    }


    void OnDisable()
    {
        mobileUI.OnSnapImage -= StartEdgeDetection;
        mobileUI.OnCloseAction -= HandleCloseEvent;
        mobileUI.OnZoom -= HandleZoom;
    }

    void SetupEmojis()
    {
        emojis.Clear();
        var db = LoadEmojiDB().DB;
        int i = 0;
        foreach (string emojiName in UISelectionMode.selectedEmojis)
        {
            emojis.Add(db[emojiName]);
            if (projections.Count <= i)
            {
                var proj = Instantiate(emojiProjectionPrefab);
                proj.transform.SetParent(transform);
                proj.Setup(image);
                proj.SetTrackingIndex(i);
                projections.Add(proj);

            }
            i++;
        }
    }

    void HandleZoom(float zoom)
    {
        this.zoom = zoom;
    }

    bool HandleCloseEvent()
    {
        if (showingResults)
        {
            CloseResults();
            return true;
        }
        return false;
    }

    void CloseResults()
    {
        //TODO: Some more clean up probably
        if (OnDetectorStatusChange != null)
        {
            OnDetectorStatusChange(this, DetectorStatus.Filming);
        }

        showingResults = false;
    }

    void StartEdgeDetection()
    {
        if (!working)
        {
            StartCoroutine(Detect());
        }
    }
    public static Texture2D SetupDynamicTexture(Image image, int size)
    {
        Texture2D tex = new Texture2D(size, size);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        image.sprite.name = "Dynamic texture";
        return tex;
    }

    public static void SetEmoji(Emoji emoji)
    {
        EmojiDB db = LoadEmojiDB();
        db.Set(emoji);
        SaveEmojiDB(db);
    }

    public static EmojiDB LoadEmojiDB()
    {
        try
        {
            Stream stream = File.Open(dbLocation, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            EmojiDB emojiDB;
            try {
                emojiDB = (EmojiDB)bformatter.Deserialize(stream);
                stream.Close();
            } catch (Exception ex) {

                if (ex is ArgumentException || ex is EndOfStreamException)
                {
                    stream.Close();
                    emojiDB = CreateEmojiDb();
                } else
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
        EmojiDB db = new EmojiDB();
        return db;
    }


    public static void SaveEmojiDB(Dictionary<string, Emoji> db)
    {
        var emojiDB = LoadEmojiDB();
        emojiDB.DB = db;
        SaveEmojiDB(emojiDB);
    }

    public static void SaveEmojiDB(EmojiDB db)
    {
        Stream stream = File.Open(dbLocation, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, db);
        stream.Close();
    }

    protected abstract void _EdgeDrawCalculation();
    protected abstract void _PostDetection();

    protected IEnumerator<WaitForEndOfFrame> Detect()
    {
        working = true;
        if (OnDetectorStatusChange != null)
        {
            OnDetectorStatusChange(this, DetectorStatus.Detecting);
        }

        yield return new WaitForEndOfFrame();
        _EdgeDrawCalculation();

        GetCornerDetection();
        GetCorners();

        _PostDetection();
        working = false;

    }

    void GetCorners()
    {
        corners = ImageAnalysis.Math.CoordinatesToTexRelativeVector2(
            cornerTexture.LocateCornersAsCoordinates(nCorners, aheadCost, minDistance, (size - cornerTexture.ResponseStride)/2),
            cornerTexture.Texture);
        for (int i = 0; i < emojis.Count; i++) {
            if (OnMatchWithEmoji != null)
                OnMatchWithEmoji(i, corners, emojis[i]);
        }
    }

    void GetCornerDetection()
    {
        cornerTexture.Convolve(I, size);
        showingResults = true;
        if (OnDetectorStatusChange != null)
        {
            OnDetectorStatusChange(this, DetectorStatus.ShowingResults);
        }
    }

    protected void MarkCorners(Vector2[] coordinates, Transform parent)
    {
        UICornerMarker corner;

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (cornerMarkers.Count <= i)
            {
                corner = Instantiate(cornerPrefab);
                corner.transform.SetParent(parent);
                corner.Setup();
                cornerMarkers.Add(corner);
            }
            else
            {
                corner = cornerMarkers[i];
            }
            corner.SetCoordinate(coordinates[i]);
        }

        for (int i = coordinates.Length, cL = cornerMarkers.Count; i < cL; i++)
        {
            cornerMarkers[i].Showing = false;
        }
    }


}
