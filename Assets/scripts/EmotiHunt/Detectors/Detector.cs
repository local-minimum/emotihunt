using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

using System;
using System.IO;

public delegate void DetectorStatusEvent(Detector screen, DetectorStatus status);
public delegate void EmojiMatchEvent(int index, Vector2[] corners, Emoji emoji);
public delegate void ProgressEvent(ProgressType t, string message, float progress);
public delegate void EmojiProjectionEvent(EmojiProjection emojiProjection);

public enum ProgressType {Detector, EmojiDB};
public enum DetectorStatus {Filming, DetectingSetup, ReadyToDetect, Detecting, SavedResults, Scoring, WaitingForScreenshot, Screenshotted, Inactive, PreIniting, Initing};

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
    public event EmojiProjectionEvent OnNewEmojiProjection;

    [SerializeField]
    protected Image image;

    protected bool working = false;
    protected HarrisCornerTexture cornerTexture;
    protected double[,] I;

    [SerializeField, Range(100, 800)]
    protected int size = 400;

    [SerializeField, Range(10, 200)]
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

    private string screnShotFileName;
    UIScoreCollector scoreCollector;

    public static bool Ready
    {
        get
        {
            return ready;
        }
    }

    DetectorStatus status = DetectorStatus.PreIniting;

    public DetectorStatus Status
    {
        get
        {
            if (!enabled)
            {
                return DetectorStatus.Inactive;
            }
            else {
                return status;
            }
        }

        set
        {
            status = value;
            if (OnDetectorStatusChange != null)
            {
                OnDetectorStatusChange(this, status);
            }
        }
    }

    void Awake()
    {
        mobileUI = FindObjectOfType<MobileUI>();
        scoreCollector = GetComponentInParent<UIScoreCollector>();
        I = new double[size * size, 3];
        
    }

    void OnEnable()
    {       
        mobileUI.OnSnapImage += StartEdgeDetection;
        mobileUI.OnZoom += HandleZoom;
        mobileUI.OnModeChange += HandleModeChange;
        SetupProjections();
        Status = DetectorStatus.Filming;
    }

    void OnDisable()
    {
        mobileUI.OnSnapImage -= StartEdgeDetection;
        mobileUI.OnZoom -= HandleZoom;
        mobileUI.OnModeChange -= HandleModeChange;
    }

    private void HandleModeChange(UIMode mode)
    {
        if (mode == UIMode.Composing)
        {
            Status = DetectorStatus.Filming; 
        }            
    }

    void SetupProjections()
    {
        var db = emojiDB.DB;
        int i = 0;
        emojis.Clear();
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
                if (OnNewEmojiProjection != null)
                {
                    OnNewEmojiProjection(proj);
                }

            }
            i++;
        }
    }

    public IEnumerator<WaitForSeconds> SetupEmojis()
    {
        status = DetectorStatus.Initing;
        ready = false;
        float waitTime = 0.02f;
        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Loading local data", 0);
        }
        
        yield return new WaitForSeconds(waitTime);
        emojis.Clear();
        emojiDB = EmojiDB.LoadEmojiDB();

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Checking for update", 0);
        }

        foreach (var kvp in emojiDB.Update())
        {
            if (OnProgressEvent != null)
            {
                OnProgressEvent(ProgressType.EmojiDB, kvp.Key, kvp.Value);
            }
            yield return new WaitForSeconds(waitTime);
        }

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.EmojiDB, "Ready", 1);
            OnProgressEvent(ProgressType.Detector, "Ready", 1);
        }
        ready = true;
        status = DetectorStatus.Filming;
    }

    void HandleZoom(float zoom)
    {
        this.zoom = zoom;
    }

    void StartEdgeDetection()
    {
        if (!working)
        {
            StartCoroutine(DetectSetup());
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

#if UNITY_EDITOR

        string ver = emojiDB.Version.ToString();
        Debug.Log("Version:" + ver);
        string location = Application.persistentDataPath + "/version.txt";
        using (FileStream f = File.Open(location, FileMode.Create, FileAccess.Write))
        {
            StreamWriter sw = new StreamWriter(f);
            sw.Write(ver);
            f.Flush();
        }

#endif
    }

    protected abstract void _EdgeDrawCalculation();
    protected abstract void _PostDetection();

    protected IEnumerator<WaitForEndOfFrame> DetectSetup()
    {
        working = true;
        Status = DetectorStatus.DetectingSetup;

        yield return new WaitForEndOfFrame();

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Processing image", 0.0f);
        }

        _EdgeDrawCalculation();

        if (status == DetectorStatus.Filming)
        {
            working = false;
            yield break;
        }

        Status = DetectorStatus.ReadyToDetect;

    }

    protected IEnumerator<WaitForSeconds> Detect() {
        float delta = 0.01f;

        Status = DetectorStatus.Detecting;

        foreach (float progress in GetCornerDetection())
        {
            if (OnProgressEvent != null)
            {
                OnProgressEvent(ProgressType.Detector, "Detecting cornernress", 0.1f + 0.6f * progress);
            }
            yield return new WaitForSeconds(delta);

        }

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Detecting corners", 0.7f);
        }
        yield return new WaitForSeconds(delta);

        if (status == DetectorStatus.Filming)
        {
            working = false;
            yield break;
        }

        foreach (float progress in GetCorners())
        {
            if (OnProgressEvent != null)
            {
                OnProgressEvent(ProgressType.Detector, "Detecting corners", 0.7f + 0.2f * progress);
                yield return new WaitForSeconds(delta);
            }
        }

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Detecting post-process", 0.9f);
        }
        yield return new WaitForSeconds(delta);

        if (status == DetectorStatus.Filming)
        {
            working = false;
            yield break;
        }

        _PostDetection();

        if (OnProgressEvent != null)
        {
            OnProgressEvent(ProgressType.Detector, "Detection done!", 1f);
        }

        yield return new WaitForSeconds(delta);

        if (status == DetectorStatus.Filming)
        {
            working = false;
            yield break;
        }

        working = false;
        Debug.Log("End of detection method");
    }

    IEnumerable<float> GetCorners()
    {
        int l = emojis.Count;
        float total = (l * 10 + 3);
        float progress = 0;

        ImageAnalysis.Coordinate[] coordinates = cornerTexture.LocateCornersAsCoordinates(nCorners, aheadCost, minDistance, (size - cornerTexture.ResponseStride) / 2);
        progress += 2 / total;
        yield return progress;
        corners = ImageAnalysis.Math.CoordinatesToTexRelativeVector2(coordinates, cornerTexture.Texture);
        progress += 1 / total;
        yield return progress;

        for (int i = 0; i < l; i++) {
            if (OnMatchWithEmoji != null)
            {
                OnMatchWithEmoji(i, corners, emojis[i]);
                progress += 10 / total;
                yield return progress;
            }
        }
        yield return 1;
    }

    IEnumerable<float> GetCornerDetection()
    {
        foreach (float progress in cornerTexture.Convolve(I, size))
        {
            yield return progress;
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

    protected IEnumerator<WaitForEndOfFrame> CaptureImage()
    {   
        yield return new WaitForEndOfFrame();
        ScreenShot.CaptureByBounds(transform as RectTransform, Camera.main, SaveTexture);

        var data = FeedCard.CreatePost(screnShotFileName);
        for (int i = 0; i < emojis.Count; i++) {
            data.Add(emojis[i], scoreCollector.GetScore(i));
        }
        data.scores.Add(scoreCollector.Bonus);
        Feed.Storage.Append(data);
        Status = DetectorStatus.SavedResults;
        mobileUI.viewMode = UIMode.Feed;
    }

    protected virtual void SaveTexture(Texture2D tex)
    {
        screnShotFileName = 
            DateTime.Now.ToString("yyyy-MM-dd_HH.mm") + "_" + 
            string.Join("_", UISelectionMode.selectedEmojis.ToArray()) + ".png";

        ScreenShot.WriteToFile(tex, Application.persistentDataPath + "/" + screnShotFileName);
        Status = DetectorStatus.Screenshotted;        
    }

    public void SaveImage()
    {
        if (status == DetectorStatus.WaitingForScreenshot)
        {
            StartCoroutine(CaptureImage());
        }
    }

}
