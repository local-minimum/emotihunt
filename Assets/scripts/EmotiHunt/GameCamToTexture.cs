using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ImageAnalysis;

public class GameCamToTexture : MonoBehaviour {

    ImageAnalysis.Textures.EdgeTexture edgeTexture;

    Sprite sprite2;
    [SerializeField]
    Image image2;

    ImageAnalysis.Textures.HarrisCornerTexture cornerTexture;
    [SerializeField]
    Image image3;
    [SerializeField, Range(0.04f, 0.15f)] float kappa;

    Texture2D camImage;
    bool working = false;


    // Use this for initialization
    void Start () {
        Image img = GetComponent<Image>();
        Texture2D tex = new Texture2D(200, 100);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        img.sprite = sprite;
        edgeTexture = new ImageAnalysis.Textures.EdgeTexture(tex);

        tex = new Texture2D(200, 100);
        Sprite sprite3 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        image3.sprite = sprite3;
        cornerTexture = new ImageAnalysis.Textures.HarrisCornerTexture(tex, kappa);

        tex = new Texture2D(200, 100);
        sprite2 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));

        image2.sprite = sprite2;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (!working)
        {
            StartCoroutine(EdgeDraw());
        }
    }

	IEnumerator<WaitForEndOfFrame> EdgeDraw() {
        working = true;
        yield return new WaitForEndOfFrame();
        if (camImage == null)
            camImage = new Texture2D(200, 100);


        camImage.ReadPixels(new Rect(20, 150, 200, 100), 0, 0);

        Color[] data = camImage.GetPixels();
        int stride = camImage.width;

        sprite2.texture.SetPixels(data);
        sprite2.texture.Apply();

        edgeTexture.Convolve(data, stride);

        cornerTexture.Convolve(data, stride);
        working = false;
        
    }

}
