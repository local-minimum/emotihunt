using UnityEngine;
using ImageAnalysis;
using UnityEngine.UI;

public class EmojiProjection : MonoBehaviour {

    public Image sourceImage;
    Detector detector;
    Image selfImage;
    [SerializeField]
    int trackingEmojiIndex = 0;

    [SerializeField, Range(0, 10)] float maxSqDist = 3f;

    Vector2[] translatedEmojiCorners;
    Vector2[] imageCorners;
    Vector2[] emojiCorners;

    [SerializeField, Range(1, 100)]
    int iterations = 5;

    void Awake()
    {
        detector = GetComponentInParent<Detector>();
        selfImage = GetComponent<Image>();
        selfImage.SetNativeSize();
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

        emojiCorners = emoji.corners.ToVector2();
        imageCorners = corners;

        int[] emojiCornerIndices;
        int[] imageCornerIndices;

        Vector2 imageOrigo = GetGuess(out emojiCornerIndices, out imageCornerIndices);
        Vector2 emojiOrigo = GetEmojiOrigo(emojiCornerIndices);

        float scale = 1;
        float angle = 0;
        if (imageCornerIndices[0] != -1)
        {
            scale = GetScale(emojiCornerIndices, imageCornerIndices);
            angle = GetAngle(emojiOrigo, emojiCornerIndices, imageOrigo, imageCornerIndices);
        }

        float prevScore = -1;
        float score = 0;
        int i = 0;
        Vector2 nextOrigo = imageOrigo;
        float nextAngle = angle;
        float nextScale = scale;
        float stepOrigo = 0.05f;
        float stepAngle = 30f;
        float stepScale = 0.1f;
        float moveFraction = 0.05f;

        while (i < iterations)
        {
            //This function and it's parameters (Vector2, Vector2, float, float) should find a max-score (0-1 value range)
            score = Score(emojiOrigo, imageOrigo, angle, scale);
            Debug.Log(string.Format("Fit Score ({0}): {1}", i, score));

            if (score < prevScore * (i + 100f) / 200f)
            {
                score = prevScore;
                Debug.Log(string.Format("Final Score: {0}", score));

                break;
            } else
            {
                angle = nextAngle;
                scale = nextScale;
                imageOrigo = nextOrigo;
            }

            Vector2 dOrigo = GetSobel(emojiOrigo, imageOrigo, angle, scale, stepOrigo);
            float dOrigoMagnitude = dOrigo.magnitude;

            float dAngle = GetAngleDelta(emojiOrigo, imageOrigo, angle, scale, stepAngle);
            float dScale = GetScaleDelta(emojiOrigo, imageOrigo, angle, scale, stepScale);

            float absDeltaOrigoMagnitude = Mathf.Abs(dOrigoMagnitude);
            float absDeltaAngle = Mathf.Abs(dAngle);
            float absDeltaScale = Mathf.Abs(dScale);
            Debug.Log("dT: " + dOrigoMagnitude + " dA: " + dAngle + " dS: " + dScale);
            if (absDeltaOrigoMagnitude > absDeltaAngle && absDeltaOrigoMagnitude > absDeltaScale)
            {
                Debug.Log("Moving origo: " + dOrigo);
                nextOrigo = imageOrigo - dOrigo.normalized * stepOrigo * moveFraction;
            }
            else if (absDeltaAngle > absDeltaScale)
            {
                Debug.Log("Moving angle: " + dAngle);
                nextAngle = angle - Mathf.Sign(absDeltaAngle) * stepAngle * moveFraction;
            }
            else
            {
                Debug.Log("Moving scale: " + dScale);
                nextScale = scale - Mathf.Sign(absDeltaScale) * stepScale * moveFraction;
            }

            prevScore = score;
            i++;
        }
        PlaceImage(emojiOrigo, imageOrigo, angle, scale);
    }

    float GetScaleDelta(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale, float step)
    {        
        float score_plus = Score(emojiOrigo, imageOrigo, angle, scale + step);
        float score_minus = Score(emojiOrigo, imageOrigo, angle, scale - step);
        return score_plus - score_minus;
    }

    float GetAngleDelta(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale, float step)
    {        
        float score_plus = Score(emojiOrigo, imageOrigo, angle + step, scale);
        float score_minus = Score(emojiOrigo, imageOrigo, angle - step, scale);
        return score_plus - score_minus;
    }

    Vector2 GetSobel(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale, float step)
    {
        float score_NW = Score(emojiOrigo, imageOrigo + new Vector2(-step, -step), angle, scale);
        float score_N = Score(emojiOrigo, imageOrigo + new Vector2(0f, -step), angle, scale);
        float score_NE = Score(emojiOrigo, imageOrigo + new Vector2(+step, -step), angle, scale);

        float score_W = Score(emojiOrigo, imageOrigo + new Vector2(-step, 0f), angle, scale);
        float score_E = Score(emojiOrigo, imageOrigo + new Vector2(+step, 0f), angle, scale);

        float score_SW = Score(emojiOrigo, imageOrigo + new Vector2(-step, step), angle, scale);
        float score_S = Score(emojiOrigo, imageOrigo + new Vector2(0f, step), angle, scale);
        float score_SE = Score(emojiOrigo, imageOrigo + new Vector2(+step, step), angle, scale);

        return new Vector2(
            2 * (score_E - score_W) + (score_NE - score_NW) + (score_SE - score_SW),
            2 * (score_N - score_S) + (score_NW - score_SW) + (score_NE - score_SE)) / 4f;
    }

    void PlaceImage(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale)
    {
        RectTransform r = transform as RectTransform;
        r.pivot = emojiOrigo;
        r.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        //Scale does not account for selfImage:Texture2d size ratio...should be OK if native size

        r.localScale = new Vector3(1f / scale, 1f / scale, 1f);
        r.localPosition = new Vector3(imageOrigo.x * sourceImage.rectTransform.rect.width, imageOrigo.y * sourceImage.rectTransform.rect.height);
        selfImage.enabled = true;

    }

    Vector2 GetEmojiOrigo(int[] emojiIndices)
    {
        Vector2 origo = Vector2.zero;
        for (int i=0; i<emojiIndices.Length; i++)
        {
            origo += emojiCorners[emojiIndices[i]];
        }
        return origo / (float) emojiIndices.Length;
    }

    Vector2 GetGuess(out int[] emojiIndices, out int[] imageIndices)
    {
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
        int l = imageCorners.Length;

        for (int a = 0; a < l; a++)
        {
            for (int b = 0; b < l; b++)
            {
                if (b == a)
                {
                    continue;
                }

                for (int c = 0; c < l; c++)
                {
                    if (c == a || c == b)
                    {
                        continue;
                    }
                    float val = TriUniformSSSTest(e1, e2, e3, a, b, c);
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

        emojiIndices = new int[3] { idE0, idE1, idE2 };

        if (first)
        {
            imageIndices = new int[3] { -1, -1, -1 };
            return Vector2.zero;
        }
        else {
            imageIndices = new int[3] { bestA, bestB, bestC };
            return (imageCorners[bestA] + imageCorners[bestB] + imageCorners[bestC]) / 3f;
        }
    }

    float TriUniformSSSTest(float e1, float e2, float e3, int id0, int id1, int id2)
    { 

        Vector2 imgV1 = imageCorners[id1] - imageCorners[id0];
        Vector2 imgV2 = imageCorners[id2] - imageCorners[id0];

        float i1 = imgV1.magnitude;
        float i2 = imgV1.magnitude;
        float i3 = (imgV2 - imgV1).magnitude;

        float r1 = e1 / i1;
        float r2 = e2 / i2;
        float r3 = e3 / i3;

        float mu = (r1 + r2 + r3) / 3f;

        return (Mathf.Pow(r1 - mu, 2f) + Mathf.Pow(r2 - mu, 2f) + Mathf.Pow(r3 - mu, 2f)) / 3f;
    }


    float GetScale(int[] emojiCornerIndices, int[] imageCornerIndices)
    {
        Vector2 imgV1 = imageCorners[imageCornerIndices[1]] - imageCorners[imageCornerIndices[0]];
        Vector2 imgV2 = imageCorners[imageCornerIndices[2]] - imageCorners[imageCornerIndices[0]];

        Vector2 emoV1 = emojiCorners[emojiCornerIndices[1]] - emojiCorners[emojiCornerIndices[0]];
        Vector2 emoV2 = emojiCorners[emojiCornerIndices[2]] - emojiCorners[emojiCornerIndices[0]];

        float i1 = imgV1.magnitude;
        float i2 = imgV1.magnitude;
        float i3 = (imgV2 - imgV1).magnitude;

        float e1 = emoV1.magnitude;
        float e2 = emoV1.magnitude;
        float e3 = (emoV2 - emoV1).magnitude;

        return (e1 / i1 + e2 / i2 + e3 / i3) / 3f;
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


    float GetAngle(Vector2 emojiOrigo, int[] emojiIndices, Vector2 imageOrigo, int[] imageIndices)
    {
        float angle = 0;
        int l = Mathf.Min(emojiIndices.Length, imageIndices.Length);
        for (int i=0; i< l; i++)
        {
            angle += Vector2.Angle(emojiCorners[emojiIndices[i]] - emojiOrigo, imageCorners[imageIndices[i]] - imageOrigo);
        }
        return angle / l;
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

    Vector2[] GetTranslation(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale)
    {
        int l = emojiCorners.Length;
        Vector2[] newCorners = new Vector2[l];

        for (int i = 0; i < l; i++)
        {
            newCorners[i] = RotateBy(emojiCorners[i] - emojiOrigo, angle) / scale + imageOrigo;
        }
        return newCorners;
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

    public float Score(Vector2 emojiOrigo, Vector2 imageOrigo, float angle, float scale)
    {
        return Score(GetTranslation(emojiOrigo, imageOrigo, angle, scale));
    }

    public float Score(Vector2[] translatedEmojiCorners)
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
