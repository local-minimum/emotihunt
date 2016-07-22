using UnityEngine;
using System.Collections;

public class UISelectionMode : MonoBehaviour {

    UIEmojiSelected[] selections;

    void Start()
    {
        selections = GetComponentsInChildren<UIEmojiSelected>();
    }

    public bool UIEmojiSelect(UIEmojiSelector btn)
    {
        for (int i=0; i<selections.Length; i++)
        {
            if (selections[i].Free)
            {
                selections[i].Set(btn);
                return true;
            }
        }

        return false;
    }

    public void RemoveSelection(UIEmojiSelected selected)
    {
        for (int i=System.Array.IndexOf(selections, selected), l = selections.Length - 1; i<l; i++)
        {
            selections[i + 1].Shift(selections[i]);
        }
        selections[selections.Length - 1].Unset();
    }
}
