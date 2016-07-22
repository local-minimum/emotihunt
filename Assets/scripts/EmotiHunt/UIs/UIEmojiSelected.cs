using UnityEngine;
using UnityEngine.UI;

public class UIEmojiSelected : MonoBehaviour {

    [SerializeField]
    Sprite emptySprite;

    UIEmojiSelector btn;
    Button button;
    Image img;

    void Start()
    {
        button = GetComponent<Button>();
        img = GetComponent<Image>();
        Unset();
    }

    public bool Free
    {
        get
        {
            return btn == null;
        }
    }

    public void Set(UIEmojiSelector btn)
    {
        this.btn = btn;
        img.sprite = btn.EmojiSprite;
        button.interactable = true;
    }

    public void Unset()
    {
        btn = null;
        img.sprite = emptySprite;
        button.interactable = false;
    }

    public void Shift(UIEmojiSelected other)
    {
        if (btn == null) { 
            other.Unset();
        }else {
            other.Set(btn);
        }
        Unset();
    }
}
