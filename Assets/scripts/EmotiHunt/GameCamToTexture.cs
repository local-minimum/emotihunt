using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ImageAnalysis;

public class GameCamToTexture : MonoBehaviour {

    Image img;
    Sprite sprite;


    Sprite sprite2;
    [SerializeField]
    Image image2;

    Sprite sprite3;
    [SerializeField]
    Image image3;

    Texture2D camImage;

    // Use this for initialization
    void Start () {
        img = GetComponent<Image>();
        Texture2D tex = new Texture2D(200, 100);
        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        img.sprite = sprite;

        tex = new Texture2D(200, 100);
        sprite3 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));
        image3.sprite = sprite3;


        tex = new Texture2D(200, 100);
        sprite2 = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2f, tex.height / 2f));

        image2.sprite = sprite2;
	}
	
	// Update is called once per frame
    void Update()
    {
        StartCoroutine(EdgeDraw());

    }

	IEnumerator<WaitForEndOfFrame> EdgeDraw() {

        yield return new WaitForEndOfFrame();
        if (camImage == null)
            camImage = new Texture2D(200, 100);


        camImage.ReadPixels(new Rect(20, 150, 200, 100), 0, 0);

        Color[] data = camImage.GetPixels();
        sprite2.texture.SetPixels(data);
        sprite2.texture.Apply();

        sprite.texture.SetEdges(data);
        
        sprite3.texture.SetDifferenceOfGaussians(data);

    }
}
