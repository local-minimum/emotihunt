using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

public class WebCamToTextureEmulator : MonoBehaviour
{

    [SerializeField]
    Sprite sprite;

    [SerializeField, Range(100, 800)]
    int size = 400;

    [SerializeField]
    Image viewImage;
    Texture2D imageTex;

    [SerializeField]
    Image detectionImage;
    Texture2D detectionTex;

    HarrisCornerTexture cornerTexture;
    bool working = false;
    bool showingResults = false;

    float zoom = 0;

    MobileUI mobileUI;

    double[,] I;

    void Awake()
    {
        mobileUI = FindObjectOfType<MobileUI>();
    }

    void Start()
    {

        imageTex = WebCamToTexture.SetupDynamicTexture(viewImage, size);
        detectionTex = WebCamToTexture.SetupDynamicTexture(detectionImage, size);
        cornerTexture = new HarrisCornerTexture(detectionTex);
        detectionImage.enabled = false;
        I = new double[size * size, 3];

    }

    void Update()
    {
        if (!working && !showingResults)
        {
            StartCoroutine(ShowCurrentImage());
        }
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
        if (!working)
        {
            StartCoroutine(EdgeDraw());
        }
    }

    IEnumerator<WaitForEndOfFrame> ShowCurrentImage()
    {

        yield return new WaitForEndOfFrame();

        ImageAnalysis.Convolve.Texture2Double(sprite.texture, ref I, size, zoom);
        ImageAnalysis.Convolve.Apply(ref I, size, imageTex);
        detectionImage.enabled = false;
    }

    IEnumerator<WaitForEndOfFrame> EdgeDraw()
    {
        working = true;
        yield return new WaitForEndOfFrame();

        ImageAnalysis.Convolve.Texture2Double(sprite.texture, ref I, size, zoom);
        ImageAnalysis.Convolve.Apply(ref I, size, imageTex);
        cornerTexture.ConvolveAndApply(I, size);
        detectionImage.enabled = true;
        showingResults = true;
        working = false;
    }
}