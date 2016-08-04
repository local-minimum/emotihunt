using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using ImageAnalysis;

public class UICornerMarker : MonoBehaviour {

    Image sourceImage;
    Image selfImage;
    Detector detector;

    public void Setup(Image sourceImage = null)
    {
        if (sourceImage)
        {
            this.sourceImage = sourceImage;
        } else {
            this.sourceImage = transform.parent.GetComponentInParent<Image>();
        }
        selfImage = GetComponent<Image>();

        detector = this.sourceImage.GetComponent<Detector>();
        if (detector)
        {
            detector.OnDetectorStatusChange += HandleDetectorChange;
        }
    }

    void OnDisable()
    {
        if (detector)
        {
            detector.OnDetectorStatusChange -= HandleDetectorChange;
        }
    }

    private void HandleDetectorChange(Detector screen, DetectorStatus status)
    {
        if (status == DetectorStatus.Filming || status == DetectorStatus.Inactive)
        {
            Showing = false;
        }
    }

    public void SetCoordinate(Coordinate coordinate, int offset)
    {
        name = string.Format("Corner ({0}, {1})", coordinate.x, coordinate.y);
        Vector2 v = Math.CoordinateToTexRelativeVector2(coordinate, sourceImage.sprite.texture, offset);
        SetCoordinate(v);
    }

    public void SetCoordinate(Vector2 v)
    { 
        transform.localPosition = Math.TexRelativeVector2ToLocalPosition(v, sourceImage);
        Showing = true;
    }

    public void SetColor(int colorIndex)
    {

    }

    public bool Showing
    {
        get
        {
            return selfImage.enabled;
        }
        
        set
        {
            selfImage.enabled = value;
        }
    }
}
