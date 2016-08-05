using UnityEngine;
using UnityEngine.UI;

public class NotificationCard : MonoBehaviour {

    [SerializeField]
    Text message;

    [SerializeField]
    Text score;

    public void Setup(FeedCard card)
    {
        message.text = card.message;
        if (card.cardType == FeedCardType.NotificationScoreCount)
        {
            if (card.scores.Count > 0)
            {
                score.text = "Score: " + card.scores[0].ToString("D4");
            } else
            {
                score.text = "Score: " + "0000";
            }
        } else
        {
            score.text = "";
        }
    }
}
