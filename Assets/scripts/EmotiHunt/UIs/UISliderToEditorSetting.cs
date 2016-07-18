using UnityEngine;
using UnityEngine.UI;
using System.Reflection;



public class UISliderToEditorSetting : MonoBehaviour {

    [SerializeField]
    string propertyName;
       
	void Start () {
        EditorUI eUI = GetComponentInParent<EditorUI>();
        Slider s = GetComponent<Slider>();
        if (s.wholeNumbers)
        {
            typeof(EditorUI).GetField(propertyName).SetValue(eUI, Mathf.RoundToInt(s.value));
        } else
        {
            typeof(EditorUI).GetField(propertyName).SetValue(eUI, s.value);

        }
    }
	

}
