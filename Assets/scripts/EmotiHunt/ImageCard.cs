using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class ImageCard : MonoBehaviour {

    [SerializeField]
    Image image;

    [SerializeField]
    Image[] emojiIcons;

    [SerializeField]
    Text[] emojiTexts;

    [SerializeField]
    Text totalText;

    [SerializeField]
    Text bonusText;
    
    public void Setup(FeedCard card)
    {
        SetTextureFromFile(card.imagePath);
        SetScores(card);
    }

    void SetTextureFromFile(string location) {
        location = Application.persistentDataPath + "/" + location;
        Debug.Log(location);
        Texture2D tex;
        tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(location));        
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        sprite.name = location;
        image.sprite = sprite;

    }

    void SetScores(FeedCard card)
    {

        int emojis = card.emojis.Count;

        for (int i=0; i<emojiIcons.Length; i++)
        {
            if (i < emojis)
            {
                SetEmoji(emojiIcons[i], card.emojis[i]);
                emojiTexts[i].text = card.scores[i].ToString();
            } 
            emojiTexts[i].enabled = i < emojis;
            emojiIcons[i].enabled = i < emojis;            
        }

        bonusText.text = card.scores.Last().ToString();
        totalText.text = card.scores.Sum().ToString();
    }

    void SetEmoji(Image img, Emoji emoji) {

        Texture2D tex = new Texture2D(emoji.pixelStride, emoji.height);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, emoji.pixelStride, emoji.height), Vector2.one * 0.5f);
        ImageAnalysis.Convolve.Apply(ref emoji.pixels, emoji.pixelStride, tex);
        sprite.name = emoji.emojiName;
        img.sprite = sprite;
        
    }
}
