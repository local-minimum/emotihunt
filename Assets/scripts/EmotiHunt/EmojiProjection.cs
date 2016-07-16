using UnityEngine;
using System.Collections;
using ImageAnalysis;
using UnityEngine.UI;

public class EmojiProjection : MonoBehaviour {

    public Coordinate coordinate;
    public Image sourceImage;
    Detector detector;
    Image selfImage;
    [SerializeField]
    int trackingEmojiIndex = 0;

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

    private void HandleNewCorners(int index, Coordinate[] corners, Emoji emoji)
    {
        if (index != trackingEmojiIndex)
            return;

        SetSelfImage(emoji);
        
        coordinate = corners[0];
        Vector2 v = Math.CoordinateToTexRelativeVector2(coordinate, sourceImage.sprite.texture) - sourceImage.rectTransform.pivot;
        transform.localPosition = new Vector3(v.x * sourceImage.rectTransform.rect.width, v.y * sourceImage.rectTransform.rect.height);
        selfImage.enabled = true;
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
