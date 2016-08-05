using System;
using System.Collections.Generic;

public enum FeedCardType {Post, NotificationScoreCount, Notification};

[Serializable]
public class FeedCard  {
    
    public FeedCardType cardType;
    public string imagePath;
    public List<Emoji> emojis = new List<Emoji>();
    public List<int> scores = new List<int>();
    public string message;
    public DateTime date;

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
        card.date = DateTime.UtcNow;
        return card;
    }

    public static FeedCard CreateScoreCount(string message, int score)
    {
        var card = new FeedCard();
        card.scores.Add(score);
        card.message = message;
        card.cardType = FeedCardType.NotificationScoreCount;
        card.date = DateTime.UtcNow;
        return card;

    }

    public static FeedCard CreatePost(string imagePath)
    {
        var card = new FeedCard();
        card.imagePath = imagePath;
        card.cardType = FeedCardType.Post;
        card.date = DateTime.UtcNow;
        return card;
    }

    public double TotalDaysSince(FeedCard other)
    {
        return (other.date - date).TotalDays;
    }

    public double Age
    {
        get {
            return (DateTime.UtcNow - date).TotalDays;
        }

    }
}
