using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using ImageAnalysis;

public class UICornerMarker : MonoBehaviour {

    Image sourceImage;
    Image selfImage;

    public void Setup(Image sourceImage = null)
    {
        if (sourceImage)
        {
            this.sourceImage = sourceImage;
        } else {
            this.sourceImage = transform.parent.GetComponentInParent<Image>();
        }
        selfImage = GetComponent<Image>();
    }

    public void SetCoordinate(Coordinate coordinate, int offset)
    {
        //Debug.Log(coordinate.x + ", " + coordinate.y);
        Vector2 v = Math.CoordinateToTexRelativeVector2(coordinate, sourceImage.sprite.texture, offset) - sourceImage.rectTransform.pivot;
        SetCoordinate(v, offset);
    }

    public void SetCoordinate(Vector2 v, int offset)
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
