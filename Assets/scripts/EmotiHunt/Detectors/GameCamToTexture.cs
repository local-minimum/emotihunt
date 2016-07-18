using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ImageAnalysis;

public class GameCamToTexture : MonoBehaviour {

    ImageAnalysis.Textures.HarrisCornerTexture cornerTexture;

    Sprite sprite2;
    [SerializeField]
    Image image2;

    ImageAnalysis.Textures.HarrisCornerTexture overlayTexture;
    [SerializeField]
    Image image3;
    [SerializeField, Range(0.04f, 0.15f)] float kappa;
    [SerializeField, Range(0f, 1f)]
    float threshold;

    Texture2D camImage;
    bool working = false;

    [SerializeField, Range(10, 42)] int nCorners = 24;
    [SerializeField, Range(1, 4)] float aheadCost = 1.4f;
    [SerializeField, Range(0, 40)] int minDistance = 9;

    double[,] I;
    Color[] data;

    // Use this for initialization
    void Start () {
        Image img = GetComponent<Image>();
        Texture2D tex = new Texture2D(200, 100);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        img.sprite = sprite;
        cornerTexture = new ImageAnalysis.Textures.HarrisCornerTexture(tex);

        tex = new Texture2D(200, 100);
        Sprite sprite3 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        image3.sprite = sprite3;
        overlayTexture = new ImageAnalysis.Textures.HarrisCornerTexture(tex, kappa);

        tex = new Texture2D(200, 100);
        sprite2 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));

        image2.sprite = sprite2;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (!working)
        {
            overlayTexture.Kappa = kappa;
            overlayTexture.Threshold = threshold;
            StartCoroutine(EdgeDraw());
        }
    }

	IEnumerator<WaitForEndOfFrame> EdgeDraw() {
        working = true;
        yield return new WaitForEndOfFrame();
        if (camImage == null)
        {
            camImage = new Texture2D(200, 100);
            I = new double[200 * 100, 3];
        }

        camImage.ReadPixels(new Rect(20, 150, 200, 100), 0, 0);
        data = camImage.GetPixels();
        Convolve.Color2Double(ref data, ref I);

        int stride = camImage.width;

        sprite2.texture.SetPixels(data);
        sprite2.texture.Apply();

        cornerTexture.ConvolveAndApply(I, stride);


        overlayTexture.Texture.SetPixels(data);
        int[,] corners = overlayTexture.LocateCorners(nCorners, aheadCost, minDistance);
        int responseStride = overlayTexture.ResponseStride;

        for (int i=0; i<nCorners; i++)
        {
            /*if (i < 3)
            {
                Debug.Log(i + ": " + corners[i, 0] + ", color " + corners[i, 1]);
            }*/
            Blit.Cross(Math.ConvertCoordinate(corners[i, 0], responseStride), overlayTexture.Texture, corners[i, 1]);
        }

        overlayTexture.Texture.Apply();

        working = false;
        
    }

}
