using UnityEngine;
using UnityEngine.UI;
using ImageAnalysis;

public class UIEmojiSelector : MonoBehaviour {

    UISelectionMode ui;
    Button btn;
    Emoji emoji;
    Texture2D tex;
    Image img;
    Sprite sprite;

    public bool Selected
    {
        get
        {
            return !btn.interactable;
        }

        set
        {
            btn.interactable = !value;
        }
    }

    public string Name
    {
        get
        {
            return emoji.emojiName;
        }
    }

    public Sprite EmojiSprite
    {
        get
        {
            return sprite;
        }
    }


    public void Set(Emoji emoji)
    {
        gameObject.name = "Emoji: " + emoji.emojiName;
        this.emoji = emoji;
        Selected = false;
        tex = new Texture2D(75, 75);        
        Convolve.Apply(ref emoji.pixels, emoji.pixelStride, tex);
        sprite = Sprite.Create(tex, new Rect(0, 0, 75, 75), Vector2.one * 0.5f);
        img.sprite = sprite;
    }

	// Use this for initialization
	void Start () {
        btn = GetComponent<Button>();
        ui = GetComponentInParent<UISelectionMode>();

    }

    public void OnClick()
    {
        Selected = ui.UIEmojiSelect(this);
    }
}
