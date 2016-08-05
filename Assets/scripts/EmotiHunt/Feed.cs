using UnityEngine;
using System.Linq;


public class Feed : MonoBehaviour {

    static DataFeed<FeedCard> _Storage;

    public static DataFeed<FeedCard> Storage
    {
        get
        {
            if (_Storage == null)
            {
                _Storage = new DataFeed<FeedCard>(Application.persistentDataPath + "/feed.bin");
            }
            return _Storage;
        }
    }

    [SerializeField]
    int index = 0;

    [SerializeField, Range(1, 10)]
    int readLength = 5;

    [SerializeField]
    Transform contentTransform;

    [SerializeField]
    ImageCard imageCardPrefab;

    [SerializeField]
    Detector detector;

    [SerializeField]
    UISelectionMode selectionMode;

    MobileUI mobileUI;

    void Awake()
    {
        mobileUI = GetComponentInParent<MobileUI>();
    }

    void OnEnable()
    {
        detector.OnDetectorStatusChange += HandleDetectorStatus;
        mobileUI.OnModeChange += HandleModeChange;
        selectionMode.OnNewBoard += HandleNewBoard;
    }

    void OnDisable()
    {
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
        mobileUI.OnModeChange -= HandleModeChange;
        selectionMode.OnNewBoard -= HandleNewBoard;
    }

    private void HandleNewBoard(BoardEvent boardEvent, int scoreFrom)
    {
        int score = 0;
        foreach (FeedCard card in Storage.Read(scoreFrom, Storage.Count - scoreFrom))
        {
            score += card.scores.Sum();
        }

        string message = "";
        if (boardEvent == BoardEvent.Updated) {
            message = "New emojis terminated active round";
        } else if (boardEvent == BoardEvent.ResetAge)
        {
            message = "Time's up (more than 7 days past since board got started)";
        } else if (boardEvent == BoardEvent.ResetDone)
        {
            message = "Round completed!";
        }
        
        
        Storage.Append(FeedCard.CreateScoreCount(message, score));
        PrependNewest();
    }

    private void HandleModeChange(UIMode mode)
    {
        if (mode == UIMode.Feed)
        {
            mobileUI.SetStatus("Feed");
        }
    }

    private void HandleDetectorStatus(Detector screen, DetectorStatus status)
    {
        if (status == DetectorStatus.SavedResults)
        {
            PrependNewest();
        }
    }

    void Start () {
        int l = Storage.Count;
        index = Mathf.Max(0, l - readLength);
        Debug.Log("Feed length: " + l);
        Debug.Log("Feed at: " + index);
        
        LoadBatch();
    }

    void LoadBatch()
    {
        try
        {
            var newPosts = Storage.Read(index, readLength);
            Debug.Log("Batch size: " + newPosts.Count);
            for (int i = newPosts.Count - 1; i > -1; i--)
            {
                FeedCard post = newPosts[i];
                if (post.cardType == FeedCardType.Post)
                {
                    ImageCard iCard = Instantiate(imageCardPrefab);
                    iCard.name = "(Archive) Photo " + (index + i + 1);
                    iCard.transform.SetParent(contentTransform);
                    iCard.Setup(post);
                }
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            Debug.LogWarning("No feed save data");
        } catch (System.DataMisalignedException)
        {
            Debug.LogError("Feed is corrupt, will be wiped");
        }
    }

    public void PrependNewest()
    {
        FeedCard post = Storage.Last;
        if (post.cardType == FeedCardType.Post)
        {
            ImageCard iCard = Instantiate(imageCardPrefab);
            iCard.name = "(Shot) Photo " + Storage.Count;
            iCard.transform.SetParent(contentTransform);
            iCard.Setup(post);
            (iCard.transform as RectTransform).SetAsFirstSibling();

        }

    }

    public FeedCard GetMostRecent(FeedCardType cardType, out int index)
    {
        index = 0;
        int idx = 0;
        FeedCard last = null;
        foreach (FeedCard card in Storage.Browse())
        {
            if (card.cardType == cardType)
            {
                last = card;
                index = idx;
            }
            idx++;
        }
        return last;
    }

}
