using UnityEngine;
using System.Collections;

public delegate void SnapImage();
public delegate bool CloseEvent();

public class MobileUI : MonoBehaviour {

    public event SnapImage OnSnapImage;
    public event CloseEvent OnCloseAction;

    public void QuitApp()
    {
        bool caught = false;
        foreach(CloseEvent e in OnCloseAction.GetInvocationList())
        {
            if (e())
            {
                caught = true;
            }

        }

        if (!caught)
        {
            Application.Quit();
        }
    }

    public void Snap()
    {
        if (OnSnapImage != null)
            OnSnapImage();
    }
}
