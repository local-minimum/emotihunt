using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System;

public class WebCamToTextureEmulator : Detector
{

    [SerializeField]
    Sprite sprite;

    Texture2D imageTex;

    [SerializeField]
    Image detectionImage;

    Texture2D detectionTex;

    void Start()
    {

        imageTex = WebCamToTexture.SetupDynamicTexture(image, size);
        detectionTex = WebCamToTexture.SetupDynamicTexture(detectionImage, size);
        cornerTexture = new HarrisCornerTexture(detectionTex);
        detectionImage.enabled = false;

    }

    void Update()
    {
        if (Status == DetectorStatus.Filming)
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
        if (debug)
        {
            MarkCorners(corners, image.transform);
        }
    }

}