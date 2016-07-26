using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

using System;

public delegate void DetectorStatusEvent(Detector screen, DetectorStatus status);
public delegate void EmojiMatchEvent(int index, Vector2[] corners, Emoji emoji);
public delegate void ProgressEvent(ProgressType t, string message, float progress);


public enum ProgressType {Detector, EmojiDB};
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
    public event ProgressEvent OnProgressEvent;

    [SerializeField]
    protected Image image;

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

    public static EmojiDB emojiDB;
    static bool ready = false;

    public static bool Ready
    {
        get
        {
            return ready;
        }
    }

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
        mobileUI.OnSnapImage += StartEdgeDetection;
        mobileUI.OnCloseAction += HandleCloseEvent;
        mobileUI.OnZoom += HandleZoom;
        SetupProjections();
    }


    void OnDisable()
    {
        mobileUI.OnSnapImage -= StartEdgeDetection;
        mobileUI.OnCloseAction -= HandleCloseEvent;
        mobileUI.OnZoom -= HandleZoom;
    }

    void SetupProjections()
    {
        var db = emojiDB.DB;
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

    public IEnumerator<WaitForSeconds> SetupEmojis()
    {
        ready = false;
        float waitTime = 0.1f;
        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Checking for updates", -1);
        }
        
        yield return new WaitForSeconds(waitTime);
        emojis.Clear();
        emojiDB = EmojiDB.LoadEmojiDB();

        foreach(string status in emojiDB.Update())
        {
            if (OnProgressEvent != null)
            {
                OnProgressEvent(ProgressType.EmojiDB, status, -1);
            }
            yield return new WaitForSeconds(waitTime);
        }

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.EmojiDB, "Ready", 1);
            OnProgressEvent(ProgressType.Detector, "Ready", 1);
        }
        ready = true;
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
            mobileUI.Play();
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
        emojiDB.Set(emoji);
        EmojiDB.SaveEmojiDB(emojiDB);
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
