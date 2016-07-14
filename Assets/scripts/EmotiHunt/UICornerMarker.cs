using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using ImageAnalysis;

public class UICornerMarker : MonoBehaviour {

    Image sourceImage;
    Image selfImage;

    public void Setup()
    {
        sourceImage = transform.parent.GetComponentInParent<Image>();
        selfImage = GetComponent<Image>();
    }
    
    public void SetCoordinate(Coordinate coordinate)
    {        
        Vector2 v = Math.CoordinateToRelativeVector2(coordinate, sourceImage.sprite.texture) - sourceImage.rectTransform.pivot;
        transform.localPosition = new Vector3(v.x * sourceImage.rectTransform.rect.width, v.y * sourceImage.rectTransform.rect.height);
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
