using UnityEngine;
using System.Collections.Generic;

public class DelayQuit : MonoBehaviour {

    MobileUI mobileUI;
    bool quitting = false;
    [SerializeField, Range(0, 3)] float delay = 1;

	void Awake () {
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
        if (mode == UIMode.Quitting)
        {
            StartCoroutine(Delay());
        } else
        {
            quitting = false;
        }
    }

    IEnumerator<WaitForSeconds> Delay() {
        if (quitting)
        {
            yield break;           
        }
        mobileUI.SetStatus("Bye bye");
        quitting = true;
        yield return new WaitForSeconds(delay);
        if (quitting)
        {
            mobileUI.Abort();
        }
	}
}
