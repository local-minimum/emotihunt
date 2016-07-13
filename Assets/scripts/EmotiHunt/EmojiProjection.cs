using UnityEngine;
using System.Collections;
using ImageAnalysis;
using UnityEngine.UI;

public class EmojiProjection : MonoBehaviour {

    public Coordinate coordinate;
    public Image sourceImage;
    Detector detector;
    Image selfImage;

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
        detector.OnCornersDetected += HandleNewCorners;
        detector.OnDetectorStatusChange += HandleDetectorStatus;
    }

    void OnDisable()
    {
        detector.OnCornersDetected -= HandleNewCorners;
        detector.OnDetectorStatusChange -= HandleDetectorStatus;
    }

    private void HandleDetectorStatus(Detector screen, DetectorStatus status)
    {

        if (status != DetectorStatus.ShowingResults)
        {
            selfImage.enabled = false;
        }
    }

    private void HandleNewCorners(Coordinate[] corners)
    {
        coordinate = corners[0];
        Vector2 v = Math.CoordinateToRelativeVector2(coordinate, sourceImage.sprite.texture) - sourceImage.rectTransform.pivot;
        transform.localPosition = new Vector3(v.x * sourceImage.rectTransform.rect.width, v.y * sourceImage.rectTransform.rect.height);
        selfImage.enabled = true;
    }
}
