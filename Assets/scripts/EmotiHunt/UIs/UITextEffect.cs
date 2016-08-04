using UnityEngine;
using UnityEngine.UI;

public enum TextEffect { None, Wait, FadeOut, FadeIn};

public class UITextEffect : MonoBehaviour {

    [SerializeField]
    TextEffect effect = TextEffect.None;

    [SerializeField]
    string currentText;

    [SerializeField]
    string nextText;

    float charIndex = -1;

    [SerializeField]
    string waitCharaters = "-/|\\-";

    [SerializeField, Range(0, 1)]
    float waitAnimationPerChar = 1;

    //[SerializeField]
    //string fadeCharacters = "+-.";

    //[SerializeField, Range(0, 1)]
    //float fadeAnimationPerChar = 1;

    [SerializeField, Range(0, 10)]
    float speed = 1;

    [SerializeField, Range(0, 10)]
    float interval = 5f;

    float waitDirection = 1;
    float lastDirectionFlip = 0;

    Text textUI;

	// Use this for initialization
	void Start () {
        textUI = GetComponent<Text>();
        if (!hasCurrentText)
            currentText = textUI.text;
	}

    bool hasCurrentText
    {
        get
        {
            return currentText != null && currentText != "";
        }
    }
	
	void Update () {
	    if (effect == TextEffect.None)
        {
            if (textUI.text != currentText)
            {
                textUI.text = currentText;
                charIndex = -1;
            }
        } else if (effect == TextEffect.Wait)
        {
            if (Time.timeSinceLevelLoad - lastDirectionFlip > interval || waitDirection == 0)
            {
                lastDirectionFlip = Time.timeSinceLevelLoad;
                if (waitDirection < 0)
                {
                    waitDirection = 1f;
                    charIndex = -currentText.Length * -0.3f;
                } else
                {
                    waitDirection = -1;
                    charIndex = currentText.Length * 1.3f;
                }
            }
            else {
                charIndex += waitDirection * Time.deltaTime * speed;
            }
            textUI.text = GetCurrentString(waitAnimationPerChar, waitCharaters);
        }
	}

    string GetCurrentString(float animPerChar, string animSeq)
    {
        int l = currentText.Length;
        char[] chars = new char[l];
        for (int i=0; i < l; i++)
        {
            chars[i] = GetCharacter(i, animPerChar, animSeq);
        }
        return new string(chars);
    }

    char GetCharacter(int index, float animPerChar, string animSeq)
    {
        float delta = charIndex - index;
        if (delta > 0)
        {
            return currentText[index];
        }
        delta /= animPerChar;
        int idSeq = animSeq.Length + Mathf.RoundToInt(delta);
        if (idSeq >= 0 && idSeq < animSeq.Length)
        {
            return animSeq[idSeq];
        } else
        {
            return currentText[index];
        }
    }
}
