using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System;

public class WebCamToTextureEmulator : Detector
{

    [SerializeField]
    Sprite sprite;

    [SerializeField]
    Image viewImage;
    Texture2D imageTex;

    [SerializeField]
    Image detectionImage;

    [SerializeField]
    bool debug;

    [SerializeField]
    UICornerMarker cornerPrefab;

    Texture2D detectionTex;
    List<UICornerMarker> cornerMarkers = new List<UICornerMarker>();

    void Start()
    {

        imageTex = WebCamToTexture.SetupDynamicTexture(viewImage, size);
        detectionTex = WebCamToTexture.SetupDynamicTexture(detectionImage, size);
        cornerTexture = new HarrisCornerTexture(detectionTex);
        detectionImage.enabled = false;

    }

    void Update()
    {
        if (!working && !showingResults)
        {
            StartCoroutine(ShowCurrentImage());
        }
    }

    IEnumerator<WaitForEndOfFrame> ShowCurrentImage()
    {

        yield return new WaitForEndOfFrame();

        ImageAnalysis.Convolve.Texture2Double(sprite.texture, ref I, size, zoom);
        ImageAnalysis.Convolve.Apply(ref I, size, imageTex);
        detectionImage.enabled = false;
    }

    protected override void _EdgeDrawCalculation()
    {
        ImageAnalysis.Convolve.Texture2Double(sprite.texture, ref I, size, zoom);
        ImageAnalysis.Convolve.Apply(ref I, size, imageTex);

    }

    protected override void _PostDetection()
    {
        cornerTexture.ApplyTargetToTexture(detectionTex);
        detectionImage.enabled = true;
        MarkCorners(corners, (imageTex.width - cornerTexture.ResponseStride) / 2);
    }

    void MarkCorners(ImageAnalysis.Coordinate[] coordinates, int offset)
    {
        UICornerMarker corner;

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (cornerMarkers.Count <= i)
            {
                corner = Instantiate(cornerPrefab);
                corner.transform.SetParent(viewImage.transform);
                corner.Setup();
                cornerMarkers.Add(corner);
            }
            else
            {
                corner = cornerMarkers[i];
            }
            corner.SetCoordinate(coordinates[i], offset);
        }

        for (int i = coordinates.Length, cL = cornerMarkers.Count; i < cL; i++)
        {
            cornerMarkers[i].Showing = false;
        }
    }
}