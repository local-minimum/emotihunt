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
        if (this.btn)
        {
            Unset();
        }
        this.btn = btn;
        if (!btn.Selected)
        {
            btn.Selected = true;
        }
        img.sprite = btn.EmojiSprite;
        button.interactable = true;
    }

    public void Unset()
    {
        if (btn)
        {
            btn.Selected = false;
        }
        btn = null;
        img.sprite = emptySprite;
        button.interactable = false;
    }

    public void ShiftLeft(UIEmojiSelected other)
    {
                
        if (btn == null) { 
            other.Unset();
        } else {
            other.Set(btn);
            btn = null;
        }

    }
}
