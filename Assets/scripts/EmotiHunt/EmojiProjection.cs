using UnityEngine;
using System.Collections;
using ImageAnalysis;
using UnityEngine.UI;

public class EmojiProjection : MonoBehaviour {

    public Coordinate coordinate;
    public Image sourceImage;
        
    void Update()
    {
        Vector2 v = Math.CoordinateToRelativeVector2(coordinate, sourceImage.sprite.texture) - sourceImage.rectTransform.pivot;
        transform.localPosition = new Vector3(v.x * sourceImage.rectTransform.rect.width, v.y * sourceImage.rectTransform.rect.height);

    }
}
