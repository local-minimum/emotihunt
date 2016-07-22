using UnityEngine;
using UnityEngine.UI;


public class UIEmojiSelector : MonoBehaviour {

    UISelectionMode ui;
    Button btn;
    Emoji emoji;

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

    void Load(Emoji emoji)
    {
        gameObject.name = "Emoji: " + emoji.emojiName;
        this.emoji = emoji;
        Selected = false;
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
