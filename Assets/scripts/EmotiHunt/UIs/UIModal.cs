using UnityEngine;
using System.Collections;

public enum UIMode {Selecting, Composing, Gallery, Feed };


public class UIModal : MonoBehaviour {

    [SerializeField]
    UIMode myMode;

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
            child.gameObject.SetActive(mode == myMode);
        }
    }
}
