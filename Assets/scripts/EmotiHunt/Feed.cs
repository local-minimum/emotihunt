﻿using UnityEngine;
using System.Collections.Generic;


public class Feed : MonoBehaviour {

    public static DataFeed<FeedCard> Storage;

    [SerializeField]
    int index = 0;

    [SerializeField, Range(1, 10)]
    int readLength = 5;

    [SerializeField]
    Transform contentTransform;

    [SerializeField]
    ImageCard imageCardPrefab;

    [SerializeField] Detector detector;

    MobileUI mobileUI;

    void Awake()
    {
        mobileUI = GetComponentInParent<MobileUI>();
    }

    void OnEnable()
    {
        Storage = new DataFeed<FeedCard>(Application.persistentDataPath + "/feed.bin");
        detector.OnDetectorStatusChange += HandleDetectorStatus;
        mobileUI.OnModeChange += HandleModeChange;
    }

    void OnDisable()
    {
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
        mobileUI.OnModeChange -= HandleModeChange;
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

}