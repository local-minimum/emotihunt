using UnityEngine;
using System.Collections;

public class AboutUI : MonoBehaviour {

    MobileUI mobileUI;

    void Awake()
    {
        mobileUI = GetComponentInParent<MobileUI>();
    }

    void OnEnable()
    {
        mobileUI.OnModeChange += HandleMode;
    }

    void OnDisable()
    {
        mobileUI.OnModeChange -= HandleMode;
    }

    private void HandleMode(UIMode mode)
    {
        if (mode == UIMode.About)
        {
            mobileUI.SetStatus("About");
        }
    }

    public void LinkTo(string uri)
    {
        Application.OpenURL(uri);
    }
}
