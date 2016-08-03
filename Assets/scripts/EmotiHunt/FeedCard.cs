using System;
using System.Collections.Generic;

public enum FeedCardType {Post, Notification};

[Serializable]
public class FeedCard  {
    
    public FeedCardType cardType;
    public string imagePath;
    public List<Emoji> emojis = new List<Emoji>();
    public List<int> scores = new List<int>();
    public string message;

    public void Add(Emoji emoji, int score)
    {
        emojis.Add(emoji);
        scores.Add(score);
    }

    public static FeedCard CreateNotification(string message)
    {
        var card = new FeedCard();
        card.message = message;
        card.cardType = FeedCardType.Notification;
        return card;
    }

    public static FeedCard CreatePost(string imagePath)
    {
        var card = new FeedCard();
        card.imagePath = imagePath;
        card.cardType = FeedCardType.Post;
        return card;
    }
}
