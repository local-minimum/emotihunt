using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using ImageAnalysis;

public delegate void DetectorStatusEvent(Detector screen, DetectorStatus status);
public delegate void DetectionEvent(ImageAnalysis.Coordinate[] corners);

public enum DetectorStatus {Filming, Detecting, ShowingResults, Inactive};

public abstract class Detector : MonoBehaviour {

    public event DetectionEvent OnCornersDetected;
    public event DetectorStatusEvent OnDetectorStatusChange;

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


    protected float zoom = 0;

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
        _PostDetection();
        GetCorners();
        working = false;

    }

    void GetCorners()
    {
        Coordinate[] corners = cornerTexture.LocateCornersAsCoordinates(nCorners, aheadCost, minDistance, (size - cornerTexture.ResponseStride)/2);
        if (OnCornersDetected != null)
            OnCornersDetected(corners);
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


}
