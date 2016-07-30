using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

public class WebCamToTexture : Detector {

    Texture2D tex;

    WebCamTexture camTex;

    void Start () {
        camTex = new WebCamTexture();

        image = GetComponent<Image>();
        image.preserveAspect = true;

        tex = SetupDynamicTexture(image, size);
        cornerTexture = new HarrisCornerTexture(tex);
	}

    void Update()
    {
        DetectorStatus status = Status;
        Debug.Log(status);
        if (status == DetectorStatus.Inactive || status == DetectorStatus.Initing || status == DetectorStatus.PreIniting)
        {
            return;
        }

        else if (status == DetectorStatus.Filming)
        {
            if (!camTex.isPlaying)
            {
                camTex.Play();
            }

            if (!working && !showingResults && camTex.didUpdateThisFrame)
                StartCoroutine(ShowCurrentImage());

        }

        else if (status == DetectorStatus.ReadyToDetect)
        {
            StartCoroutine(Detect());
        }
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
            MarkCorners(corners, image.transform);
        }
    }
}
