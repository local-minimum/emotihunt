﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UISelectionMode : MonoBehaviour {

    UIEmojiSelected[] selections;
    [SerializeField]
    string[] selectionTexts;

    [SerializeField]
    Text text;

    [SerializeField]
    Button playButton;

    void Start()
    {
        selections = GetComponentsInChildren<UIEmojiSelected>();
        SetCurrentSelectionText();
    }

    public bool UIEmojiSelect(UIEmojiSelector btn)
    {
        for (int i=0; i<selections.Length; i++)
        {
            if (selections[i].Free)
            {
                selections[i].Set(btn);
                SetCurrentSelectionText();
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
        SetCurrentSelectionText();
    }

    int CountSelections()  
    {
        int count = 0;
        for (int i = 0; i < selections.Length; i++)
        {
            if (!selections[i].Free)
            {
                count++;
            }
        }
        return count;

    }

        void SetCurrentSelectionText()
    {
        int count = CountSelections();
        text.text = selectionTexts[count];
        playButton.interactable = count > 1;
    }
}
