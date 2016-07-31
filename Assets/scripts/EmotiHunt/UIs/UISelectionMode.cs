﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISelectionMode : MonoBehaviour {

    public static List<string> selectedEmojis = new List<string>();

    [SerializeField] UIEmojiSelector selectorPrefab;
    [SerializeField]
    Transform selectorGrid;

    MobileUI mobileUI;

    List<UIEmojiSelector> selectors = new List<UIEmojiSelector>();
    
    UIEmojiSelected[] selections;
    [SerializeField]
    string[] selectionTexts;

    [SerializeField]
    Button playButton;

    void Start()
    {
        selections = GetComponentsInChildren<UIEmojiSelected>();
        StartCoroutine(SetupSelectors());
        mobileUI = GetComponentInParent<MobileUI>();
    }

    public IEnumerator<WaitForSeconds> SetupSelectors()
    {
        float waitTime = 0.1f;

        while (!Detector.Ready)
        {
            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log(Detector.emojiDB.Version);

        var db = Detector.emojiDB.DB;

        foreach (var kvp in db)
        {
            UIEmojiSelector selector = Instantiate(selectorPrefab);
            selectors.Add(selector);
            selector.transform.SetParent(selectorGrid);
            selector.Setup();
            selector.Set(kvp.Value);
        }

        SetCurrentSelectionText();
    }

    public bool UIEmojiSelect(UIEmojiSelector btn)
    {
        for (int i=0; i<selections.Length; i++)
        {
            if (selections[i].Free)
            {
                selections[i].Set(btn);
                if (i >= selectedEmojis.Count)
                {
                    selectedEmojis.Add(btn.Name);
                } else
                {
                    selectedEmojis[i] = btn.Name;
                }
                SetCurrentSelectionText();
                return true;
            }
        }        
        return false;
    }

    public void RemoveSelection(UIEmojiSelected selected)
    {
        bool moved = false;
        for (int i=System.Array.IndexOf(selections, selected), l = selections.Length - 1; i<l; i++)
        {
            selections[i + 1].ShiftLeft(selections[i]);
            if (selectedEmojis.Count > i + 1)
            {
                selectedEmojis[i] = selectedEmojis[i + 1];
            }
            moved = true;
        }
        if (moved)
        {
            selectedEmojis.RemoveAt(selectedEmojis.Count - 1);
            selections[selections.Length - 1].Unset();
        }

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
        mobileUI.SetStatus(selectionTexts[count], Mathf.Min(1, count / 2f));
        playButton.interactable = count > 1;
        //Debug.Log(string.Join(", ", selectedEmojis.ToArray()));
    }

}
