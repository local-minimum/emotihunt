using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ImageAnalysis.Textures;

public class EditorUI : MonoBehaviour {

    string emojiName = "";
    
    [SerializeField]
    Image img;

    [SerializeField]
    Image cornerImg;

 	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetName(Text text)
    {
        emojiName = text.text;
    }

    public void Serialize(Button button)
    {
        StartCoroutine(analyze(button));
    }

    IEnumerator<WaitForEndOfFrame> analyze(Button button) {

        Texture2D tex = img.sprite.texture;
        if (tex == null)
        {
            Debug.LogWarning("No image loaded");
            yield break;
        }
        if (emojiName == "")
        {
            Debug.LogWarning("Emoji has no name");
            yield break;
        }

        button.interactable = false;
        yield return new WaitForEndOfFrame();
        Debug.Log("Analysing");

        HarrisCornerTexture harrisCorner = GetCornerTexture(tex);

        double[,] I = ImageAnalysis.Convolve.Texture2Double(tex);
        harrisCorner.ConvolveAndApply(I, tex.width);

        button.interactable = true;
    }
    
    HarrisCornerTexture GetCornerTexture(Texture2D source)
    {
        Texture2D target = new Texture2D(source.width, source.height);
        cornerImg.sprite = Sprite.Create(
            target, 
            new Rect(0, 0, source.width, source.height), 
            Vector2.one * 0.5f);

        HarrisCornerTexture cornerTex = new HarrisCornerTexture(cornerImg.sprite.texture);
        return cornerTex;
    }

    public void Load(Button button)
    {
        img.sprite = null;
    }    
}
