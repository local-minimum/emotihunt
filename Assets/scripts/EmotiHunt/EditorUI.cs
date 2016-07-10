using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditorUI : MonoBehaviour {

    string emojiName;
    [SerializeField] Texture2D tex;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetName(Text text)
    {
        emojiName = text.text;
    }

    public void Serialize(Button button)
    {
        StartCoroutine(analyze(button));
    }

    IEnumerator<WaitForEndOfFrame> analyze(Button button) {

        button.interactable = false;

        yield return new WaitForEndOfFrame();


        button.interactable = true;
    }
}
