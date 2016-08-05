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
    NotificationCard notificationCardPrefab;

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
        Storage.OnFeedAppended += Prepend;
        mobileUI.OnModeChange += HandleModeChange;
        selectionMode.OnNewBoard += HandleNewBoard;
    }

    void OnDisable()
    { 
        mobileUI.OnModeChange -= HandleModeChange;
        selectionMode.OnNewBoard -= HandleNewBoard;
        Storage.OnFeedAppended -= Prepend;
    }

    public void Prepend(FeedCard item)
    {
        if (item.cardType == FeedCardType.Post)
        {
            ImageCard iCard = Instantiate(imageCardPrefab);
            iCard.name = "(Shot) Photo " + Storage.Count;
            iCard.transform.SetParent(contentTransform);
            iCard.Setup(item);
            (iCard.transform as RectTransform).SetAsFirstSibling();

        }
        else
        {
            NotificationCard nCard = Instantiate(notificationCardPrefab);
            nCard.name = "(Live) " + item.cardType;
            nCard.transform.SetParent(contentTransform);
            nCard.Setup(item);
            (nCard.transform as RectTransform).SetAsFirstSibling();
        }

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
    }

    private void HandleModeChange(UIMode mode)
    {
        if (mode == UIMode.Feed)
        {
            mobileUI.SetStatus("Feed");
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
                    iCard.name = "(Archive) Photo " + (Mathf.Max(index + i, 0) + 1);
                    iCard.transform.SetParent(contentTransform);
                    iCard.Setup(post);
                } else
                {
                    NotificationCard nCard = Instantiate(notificationCardPrefab);
                    nCard.name = "(Archive) " + post.cardType;
                    nCard.transform.SetParent(contentTransform);
                    nCard.Setup(post);
                }
            }
            index -= newPosts.Count;
        }
        catch (System.IO.FileNotFoundException)
        {
            Debug.LogWarning("No feed save data");
        } catch (System.DataMisalignedException)
        {
            Debug.LogError("Feed is corrupt, will be wiped");
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

    public void ScrollEvent(Vector2 pos)
    {
        if (pos.y < 0)
        {
            LoadBatch();
        }
    }
}
