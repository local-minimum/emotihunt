using UnityEngine;
using System.Collections;
using ImageAnalysis;
using UnityEngine.UI;

public class EmojiProjection : MonoBehaviour {

    public Image sourceImage;
    Detector detector;
    Image selfImage;
    [SerializeField]
    int trackingEmojiIndex = 0;

    [SerializeField, Range(0, 10)] float maxSqDist = 3f;

    void Awake()
    {
        detector = GetComponentInParent<Detector>();
        selfImage = GetComponent<Image>();
    }   

    void Start()
    {
        selfImage.enabled = false;
    }

    void OnEnable()
    {
        detector.OnMatchWithEmoji += HandleNewCorners;
        detector.OnDetectorStatusChange += HandleDetectorStatus;
    }

    void OnDisable()
    {
        detector.OnMatchWithEmoji -= HandleNewCorners;
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
    }

    private void HandleDetectorStatus(Detector screen, DetectorStatus status)
    {

        if (status != DetectorStatus.ShowingResults)
        {
            selfImage.enabled = false;
        }
    }

    private void HandleNewCorners(int index, Vector2[] corners, Emoji emoji)
    {
        if (index != trackingEmojiIndex)
            return;

        SetSelfImage(emoji);
        Vector2[] emojiCorners = emoji.corners.ToVector2();

        //TODO: Random take 3 or something
        int idE0 = 3;
        int idE1 = 5;
        int idE2 = 9;
        Vector2 vEm1 = emojiCorners[idE1] - emojiCorners[idE0];
        Vector2 vEm2 = emojiCorners[idE2] - emojiCorners[idE0];
        float e1 = vEm1.magnitude;
        float e2 = vEm2.magnitude;
        float e3 = (vEm2 - vEm1).magnitude;

        bool first = true;
        int bestA = 0;
        int bestB = 0;
        int bestC = 0;
        float bestVal = 0;
        int l = corners.Length;
        for (int a=0; a< l; a++)
        {
            for (int b=0; b< l; b++)
            {
                if (b == a)
                {
                    continue;
                }

                for (int c=0; c<l; c++)
                {
                    if (c == a || c == b)
                    {
                        continue;
                    }
                    float val = TriUniformSSSTest(e1, e2, e3, corners, a, b, c);
                    if (val < bestVal || first)
                    {
                        bestVal = val;
                        bestA = a;
                        bestB = b;
                        bestC = c;
                        first = false;
                    }
                }
            }
        }

        if (!first)
        {
            Vector2 v = (corners[bestA] + corners[bestB] + corners[bestC]) / 3f;
            transform.localPosition = new Vector3(v.x * sourceImage.rectTransform.rect.width, v.y * sourceImage.rectTransform.rect.height);
            selfImage.enabled = true;
        }
    }

    static float TriUniformSSSTest(float e1, float e2, float e3, Vector2[] imgCorners, int id0, int id1, int id2)
    { 

        Vector2 imgV1 = imgCorners[id1] - imgCorners[id0];
        Vector2 imgV2 = imgCorners[id2] - imgCorners[id0];

        float i1 = imgV1.magnitude;
        float i2 = imgV1.magnitude;
        float i3 = (imgV2 - imgV1).magnitude;

        float r1 = e1 / i1;
        float r2 = e2 / i2;
        float r3 = e3 / i3;

        float mu = (r1 + r2 + r3) / 3f;

        return (Mathf.Pow(r1 - mu, 2f) + Mathf.Pow(r2 - mu, 2f) + Mathf.Pow(r3 - mu, 2f)) / 3f;
    }


    static float GetScale(float e1, float e2, float e3, Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 imgV1 = B - A;
        Vector2 imgV2 = C - A;

        float i1 = imgV1.magnitude;
        float i2 = imgV1.magnitude;
        float i3 = (imgV2 - imgV1).magnitude;

        float r1 = e1 / i1;
        float r2 = e2 / i2;
        float r3 = e3 / i3;

        return (r1 + r2 + r3) / 3f;
    }

    static Vector2 GetOffset(Vector2 a, Vector2 b, Vector2 c, Vector2 A, Vector2 B, Vector2 C)
    {
        return ((A - a) + (B - b) + (C - c)) / 3f;
    }


    static float GetAngle(Vector2 o, Vector2 a, Vector2 b, Vector2 c, Vector2 O, Vector2 A, Vector2 B, Vector2 C)
    {

        return (Vector2.Angle(a - o, A - O) + Vector2.Angle(b - o, B - O) + Vector2.Angle(c - o, C - O)) / 3f;
    }

    public static Vector2 RotateBy(Vector2 v, float a, bool bUseRadians = false)
    {
        if (!bUseRadians) a *= Mathf.Deg2Rad;
        var ca = Mathf.Cos(a);
        var sa = Mathf.Sin(a);
        var rx = v.x * ca - v.y * sa;

        return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
    }

    static Vector2[] GetTranslation(float e1, float e2, float e3, Vector2[] eCorners, 
        int idE0, int idE1, int idE2, Vector2[] imgCorners, int id0, int id1, int id2)
    {
        Vector2 a = eCorners[idE0];
        Vector2 b = eCorners[idE1];
        Vector2 c = eCorners[idE2];
        Vector2 A = imgCorners[id0];
        Vector2 B = imgCorners[id1];
        Vector2 C = imgCorners[id2];
        Vector2 origo = (a + b + c) / 3f;
        Vector2 Origo = (A + B + C) / 3f;        

        float scale = GetScale(e1, e2, e3, A, B, C);
        float angle = GetAngle(origo, a, b, c, Origo, A, B, C);
        
        int l = eCorners.Length;
        Vector2[] newCorners = new Vector2[l];

        for (int i=0; i<l; i++)
        {
            newCorners[i] = RotateBy(eCorners[i] - origo, angle) * scale + Origo;
        }
        return newCorners;
    }

    public float Score(Vector2[] translatedEmojiCorners, Vector2[] imageCorners)
    {
        float score = 0;

        //TODO: maybe ensure no reuse of same corner twice?
        int lI = imageCorners.Length;
        int lE = lE = translatedEmojiCorners.Length;
        for (int idE=0; idE < lE; idE++)
        {
            float minVal = 0;
            bool found = false;
            for (int idI=0; idI < lI; idI++)
            {
                float val = Vector2.SqrMagnitude(imageCorners[idI] - translatedEmojiCorners[idE]);
                if (val < minVal || idI == 0)
                {
                    minVal = val;
                    found = true;
                }
            }

            if (found)
            {
                score = Mathf.Max(maxSqDist - minVal, 0) / maxSqDist;
            }
        }

        return score / lE;
    }


    private void SetSelfImage(Emoji emoji)
    {
        Debug.Log(emoji.pixelStride + "x" + emoji.height);
        Color[] pixels = Convolve.Resize(ref emoji.pixels, emoji.pixelStride, emoji.pixelStride, emoji.height);
        Texture2D tex = new Texture2D(emoji.pixelStride, emoji.height);
        tex.SetPixels(pixels);
        tex.Apply();
        selfImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    }
}
