using UnityEngine;
using System.Collections.Generic;

public enum UIMode {Selecting, Composing, CompositionPhoto, Gallery, Feed, Quitting };


public class UIModal : MonoBehaviour {

    [SerializeField]
    List<UIMode> myModes = new List<UIMode>();

    MobileUI ui;

    void OnEnable()
    {
        ui = GetComponentInParent<MobileUI>();
        ui.OnModeChange += HandleModeChange;
        HandleModeChange(ui.viewMode);
    }

    void OnDisable()
    {
        ui.OnModeChange -= HandleModeChange;   
    }

    private void HandleModeChange(UIMode mode)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(myModes.Contains(mode));
        }
    }
}
