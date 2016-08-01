using UnityEngine;
using System.Collections.Generic;

public class UIScoreCollector : MonoBehaviour {

    MobileUI mobileUI;

    [SerializeField]
    Detector detector;

    List<EmojiProjection> emojiProjections = new List<EmojiProjection>();

    [SerializeField, Range(1, 1000)]
    int scoreFloatToInts = 100;

    [SerializeField, Range(1, 30)]
    int bonusMultiplier = 10;

    [SerializeField, Range(0, 0.2f)]
    float animationSpeed = 0.02f;

    List<int> scores = new List<int>();
    List<int> emojiIndices = new List<int>();

    int calculatedScore = 0;
    bool summingUpScores = false;

    [SerializeField, Range(0, 1000)]
    int relevantThreshold = 10;

    int ScoreTotal
    {
        get
        {
            int score = 0;
            for (int i=0; i<scores.Count; i++)
            {
                score += scores[i];
            }
            return score + Bonus;
        }
    }

    int Bonus
    {
        get
        {
            int relevant = 0;
            for (int i=0; i<scores.Count; i++)
            {
                relevant += scores[i] > relevantThreshold ? 1 : 0;
            }
            return Mathf.RoundToInt(Mathf.Pow(relevant, Mathf.Max(1, scores.Count - 1))) * bonusMultiplier;
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
            summingUpScores = false;
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

        detector.Status = DetectorStatus.Scoring;

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

        if (detector.Status != DetectorStatus.Scoring)
        {            
            HandleDetectorStatus(detector, detector.Status);
            yield break;
        }

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
            if (detector.Status != DetectorStatus.Scoring)
            {
                HandleDetectorStatus(detector, detector.Status);
                yield break;
            }
            yield return new WaitForSeconds(longWait * animationSpeed);
            mobileUI.SetStatus("");
            yield return new WaitForSeconds(mediumWait * animationSpeed);

        }

        if (detector.Status != DetectorStatus.Scoring)
        {
            HandleDetectorStatus(detector, detector.Status);
            yield break;
        }

        int bonus = Bonus;
        if (bonus == 0)
        {
            mobileUI.SetStatus("No bonus");
        }

        for (int localScore=0; localScore < bonus; localScore++)
        {
            //64 relates to Bonus calculation of 4 relevant images 4^3=64
            mobileUI.SetStatus(string.Format("Bonus: {0}", localScore), localScore / (64f * bonusMultiplier));
            yield return new WaitForSeconds(animationSpeed);
        }
        calculatedScore += bonus;
        yield return new WaitForSeconds(longWait * animationSpeed);

        mobileUI.SetStatus(string.Format("TOTAL SCORE: {0}", calculatedScore));        
        summingUpScores = false;

        if (detector.Status != DetectorStatus.Scoring)
        {
            HandleDetectorStatus(detector, detector.Status);
            yield break;
        }

        detector.Status = DetectorStatus.WaitingForScreenshot;
    }

}
