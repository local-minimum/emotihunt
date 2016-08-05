using UnityEngine;
using UnityEngine.UI;

public class ExpandToContent : MonoBehaviour {

    float size = -1;

    VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();

    }

	void Update () {
        float newSize = ContentHeight;

        if (size != newSize)
        {
            RectTransform rt = transform as RectTransform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, newSize);
            size = newSize;
        }    
	}

    float ContentHeight
    {
        get
        {
            RectTransform rt = transform as RectTransform;

            float height = 0;
            for (int i = 0; i < rt.childCount; i++)
            {
                RectTransform child = rt.GetChild(i) as RectTransform;
                height += child.sizeDelta.y;
                if (i > 0)
                {
                    height += layoutGroup.spacing;
                }

            }
            return height;
            
        }
    }


}
