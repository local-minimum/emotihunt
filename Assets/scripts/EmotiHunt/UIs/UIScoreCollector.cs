using UnityEngine;
using System.Collections.Generic;

public class UIScoreCollector : MonoBehaviour {

    MobileUI mobileUI;

    [SerializeField]
    Detector detector;

    List<EmojiProjection> emojiProjections = new List<EmojiProjection>();

    [SerializeField, Range(1, 1000)]
    int scoreFloatToInts = 100;

    [SerializeField, Range(0, 0.2f)]
    float animationSpeed = 0.02f;

    List<int> scores = new List<int>();
    List<int> emojiIndices = new List<int>();

    int calculatedScore = 0;
    bool summingUpScores = false;

    int ScoreTotal
    {
        get
        {
            int score = 0;
            for (int i=0; i<scores.Count; i++)
            {
                score += scores[i];
            }
            return score;
        }
    }

	void Start () {
        mobileUI = GetComponentInParent<MobileUI>();
	}

	void OnEnable()
    {
        detector.OnNewEmojiProjection += HandleNewEmojiProjection;
        detector.OnDetectorStatusChange += HandleDetectorStatus;

        foreach (var proj in emojiProjections)
        {
            proj.OnScore += HandleScore;
        }
    }

    void OnDisable()
    {
        detector.OnNewEmojiProjection -= HandleNewEmojiProjection;
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
        foreach (var proj in emojiProjections)
        {
            proj.OnScore -= HandleScore;
        }
    }


    private void HandleDetectorStatus(Detector screen, DetectorStatus status)
    {
        if (status == DetectorStatus.DetectingSetup)
        {
            scores.Clear();
            emojiIndices.Clear();
            calculatedScore = 0;

        }
    }

    private void HandleNewEmojiProjection(EmojiProjection emojiProjection)
    {
        emojiProjections.Add(emojiProjection);
        emojiProjection.OnScore += HandleScore;    
    }

    private void HandleScore(int index, float score)
    {
        scores.Add(Mathf.RoundToInt(score * scoreFloatToInts));
        emojiIndices.Add(index);
        if (!summingUpScores)
        {
            StartCoroutine(AnimateScores());
        }
    }

    void Update () {
	    if (!summingUpScores && calculatedScore != ScoreTotal)
        {
            StartCoroutine(AnimateScores());
        }
	}

    IEnumerator<WaitForSeconds> AnimateScores()
    {
        float longWait = 50;
        float mediumWait = 20;

        if (summingUpScores)
            yield break;
        summingUpScores = true;
        mobileUI.SetStatus("Scores...");
        yield return new WaitForSeconds(longWait * animationSpeed);
        //Play catch-up
        int index = 0;
        int summedScores = 0;
        while (summedScores < calculatedScore)
        {
            if (index < scores.Count)
            {
                if (scores[index] + summedScores < calculatedScore)
                {
                    summedScores += scores[index];
                    index++;
                } else
                {
                    break;
                }
            } else
            {
                break;
            }
        }
        Debug.Log(index);

        while (index < scores.Count)
        {
            string emojiName = UISelectionMode.selectedEmojis[emojiIndices[index]];
            Debug.Log(string.Format("{0}: {1}", emojiName, scores[index]));

            mobileUI.SetStatus(string.Format("{0}: {1} points", emojiName, 0));
            for (int localScore=calculatedScore - summedScores; localScore<scores[index]; localScore++)
            {
                mobileUI.SetStatus(string.Format("{0}: {1} points", emojiName , localScore), localScore / (float) scoreFloatToInts);
                yield return new WaitForSeconds(animationSpeed);

            }
            summedScores = calculatedScore = scores[index] + summedScores;
            index++;
            yield return new WaitForSeconds(longWait * animationSpeed);
            mobileUI.SetStatus("");
            yield return new WaitForSeconds(mediumWait * animationSpeed);

        }

        summingUpScores = false;
    }

}
