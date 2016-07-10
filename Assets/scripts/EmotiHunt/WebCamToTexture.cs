using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

public delegate void ShowingResults(WebCamToTexture screen);

public class WebCamToTexture : MonoBehaviour {

    public event ShowingResults OnShowingResults;

    Image image;
    Texture2D tex;

    HarrisCornerTexture cornerTexture;
    bool working = false;
    bool showingResults = false;
    WebCamTexture camTex;
    double[,] I;
    [SerializeField, Range(100, 800)] int size = 400;
    MobileUI mobileUI;
    float zoom = 0;

    void Awake()
    {
        mobileUI = FindObjectOfType<MobileUI>();

    }

    void Start () {
        camTex = new WebCamTexture();

        image = GetComponent<Image>();
        image.preserveAspect = true;

        tex = new Texture2D(size, size);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        image.sprite.name = "Dynamic texture";

        cornerTexture = new HarrisCornerTexture(tex);

        I = new double[size * size, 3];
	}
	
	void Update () {
        if (!camTex.isPlaying)
        {
            camTex.Play();
        }
        if (!working && !showingResults)
            ShowCurrentImage();
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
        showingResults = false;
    }

    void StartEdgeDetection()
    {
        if (!working) {
            StartCoroutine(EdgeDraw());
        }
    }

    IEnumerator<WaitForEndOfFrame> ShowCurrentImage()
    {
        yield return new WaitForEndOfFrame();
        tex.SetPixels(camTex.GetPixels(0, 0, size, size));
        tex.Apply();

        //TODO: Check if this is sufficient?
        transform.rotation = Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.up);
    }

    IEnumerator<WaitForEndOfFrame> EdgeDraw()
    {
        working = true;
        yield return new WaitForEndOfFrame();
        ImageAnalysis.Convolve.WebCam2Double(camTex, ref I, size, zoom);
        cornerTexture.ConvolveAndApply(I, size);
        showingResults = true;
        if (OnShowingResults != null)
        {
            OnShowingResults(this);
        }

        working = false;

    }
}
