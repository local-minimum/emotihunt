using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

public class WebCamToTexture : MonoBehaviour {

    Image image;
    Texture2D tex;

    HarrisCornerTexture cornerTexture;
    bool working = false;
    WebCamTexture camTex;
    double[,] I;
    [SerializeField, Range(100, 800)] int size = 400;    

	void Start () {
        camTex = new WebCamTexture(size, size);

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

        if (!working)
        {
            StartCoroutine(EdgeDraw());
        }
	}

    IEnumerator<WaitForEndOfFrame> EdgeDraw()
    {
        working = true;
        yield return new WaitForEndOfFrame();
        ImageAnalysis.Convolve.WebCam2Double(camTex, ref I, size);
        cornerTexture.Convolve(I, size);
        working = false;

    }
}
