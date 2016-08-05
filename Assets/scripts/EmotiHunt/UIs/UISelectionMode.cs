using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum BoardEvent {Updated, ResetAge, ResetDone};

public delegate void NewBoard(BoardEvent boardEvent, int fromIndex);

public class UISelectionMode : MonoBehaviour {

    public event NewBoard OnNewBoard;

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

    [SerializeField]
    Detector detector;

    [SerializeField]
    Feed feed;

    long activeEmojiVersion = -1;

    void Awake()
    {
        selections = GetComponentsInChildren<UIEmojiSelected>();
        mobileUI = GetComponentInParent<MobileUI>();
    }

    void Start()
    {

        StartCoroutine(SetupSelectors());

        int idx;
        FeedCard recentScoring = feed.GetMostRecent(FeedCardType.NotificationScoreCount, out idx);
        if (recentScoring != null && recentScoring.Age > 7)
        {
            BoardEventAction(BoardEvent.ResetAge, idx);
        }
    }

    void OnEnable()
    {
        mobileUI.OnModeChange += HandleModeChange;
        detector.OnDetectorStatusChange += HandleDetectorStatus;
    }

    void OnDisable()
    {
        mobileUI.OnModeChange -= HandleModeChange;
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
    }

    void Update()
    {
        if (activeEmojiVersion < 0 && Detector.emojiDB != null)
        {
            activeEmojiVersion = Detector.emojiDB.Version;
        }
    }

    private void HandleDetectorStatus(Detector screen, DetectorStatus status)
    {
        
        if (status == DetectorStatus.SavedResults)
        {
            SetSelectedAsPhotographed();
            RemoveSelections();
            
            if (Detector.emojiDB.Version > activeEmojiVersion)
            {
                int idx;
                feed.GetMostRecent(FeedCardType.NotificationScoreCount, out idx);

                Debug.Log("Emojis updated");
                BoardEventAction(BoardEvent.Updated, idx);


            }
            else if (Detector.emojiDB.Remaining < 2)
            {
                int idx;
                feed.GetMostRecent(FeedCardType.NotificationScoreCount, out idx);

                Debug.Log("Board completed");
                BoardEventAction(BoardEvent.ResetDone, idx);
                
            }


            
        }
    }

    void BoardEventAction(BoardEvent eventType, int sinceIndex)
    {
        Detector.emojiDB.ResetSnapStatuses();

        foreach (var selector in selectors)
        {
            selector.Selected = false;
        }

        if (OnNewBoard != null)
        {
            OnNewBoard(eventType, sinceIndex);
        }

    }

    private void HandleModeChange(UIMode mode)
    {
        if (mode == UIMode.Selecting)
        {
            SetCurrentSelectionText();
        }
    }

    void SetSelectedAsPhotographed()
    {
        foreach (string eName in selectedEmojis)
        {
            Detector.emojiDB.SetPhotographed(eName);
        }
        selectedEmojis.Clear();
    }

    public IEnumerator<WaitForSeconds> SetupSelectors()
    {
        float waitTime = 0.1f;

        while (!Detector.Ready)
        {
            yield return new WaitForSeconds(waitTime);
        }

        var db = Detector.emojiDB.DB;

        foreach (var kvp in db)
        {
            UIEmojiSelector selector = Instantiate(selectorPrefab);
            selectors.Add(selector);
            selector.transform.SetParent(selectorGrid);
            selector.Setup();
            selector.Set(kvp.Value);
        }

        if (mobileUI.viewMode == UIMode.Selecting)
        {
            SetCurrentSelectionText();
        }
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

    void RemoveSelections()
    {
        foreach(var selected in selections)
        {
            selected.Unset();
        }
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
