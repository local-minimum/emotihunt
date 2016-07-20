using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;
using System;

public class WebCamToTexture : Detector {

    Image image;
    Texture2D tex;

    WebCamTexture camTex;

    void Start () {
        camTex = new WebCamTexture();

        image = GetComponent<Image>();
        image.preserveAspect = true;

        tex = SetupDynamicTexture(image, size);
        cornerTexture = new HarrisCornerTexture(tex);
	}
	
	void Update () {
        if (!camTex.isPlaying)
        {
            camTex.Play();
        }
        if (!working && !showingResults && camTex.didUpdateThisFrame)
            StartCoroutine(ShowCurrentImage());
	}


    IEnumerator<WaitForEndOfFrame> ShowCurrentImage()
    {
        yield return new WaitForEndOfFrame();
        transform.rotation = Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.forward * -1);
        ImageAnalysis.Convolve.WebCam2Double(camTex, ref I, size, zoom);
        ImageAnalysis.Convolve.Apply(ref I, size, tex);        
    }

    protected override void _EdgeDrawCalculation()
    {
        transform.rotation = Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.forward * -1);
        ImageAnalysis.Convolve.WebCam2Double(camTex, ref I, size, zoom);

    }

    protected override void _PostDetection()
    {
        //cornerTexture.ApplyTargetToTexture(tex);
        ImageAnalysis.Convolve.Apply(ref I, size, tex);
        if (debug)
        {
            MarkCorners(corners, (tex.width - cornerTexture.ResponseStride) / 2, image.transform);
        }
    }
}
