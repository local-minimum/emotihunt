using UnityEngine;
using UnityEngine.UI;

public enum FlexibleDimension { Width, Height};

public class AreaMaintainer : MonoBehaviour {

    LayoutElement layoutElem;

    [SerializeField, HideInInspector]
    Vector2 referenceRect;

    [SerializeField]
    AnimationCurve adjustmentDecay = new AnimationCurve();

    [SerializeField]
    FlexibleDimension flexibleDimension = FlexibleDimension.Height;

    float lastRatio = -1f;

    void Awake()
    {
        layoutElem = GetComponent<LayoutElement>();
    }	

    void Reset()
    {
        Awake();
        referenceRect = (transform as RectTransform).rect.size;
    }


    float Ratio
    {
        get
        {
            if (flexibleDimension == FlexibleDimension.Height)
            {
                return (transform as RectTransform).rect.width / referenceRect.x;
            } else
            {
                return (transform as RectTransform).rect.height / referenceRect.y;
            }
        }
    }

    void Update () {
        float ratio = Ratio;
        if (ratio != lastRatio)
        {
            if (flexibleDimension == FlexibleDimension.Height)
            {
                layoutElem.minHeight = referenceRect.y / adjustmentDecay.Evaluate(ratio);
            } else
            {
                layoutElem.minWidth = referenceRect.x / adjustmentDecay.Evaluate(ratio);
            }
            lastRatio = ratio;
        }
	}
}
