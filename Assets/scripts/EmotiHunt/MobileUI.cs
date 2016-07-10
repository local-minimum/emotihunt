using UnityEngine;
using UnityEngine.UI;

public delegate void SnapImage();
public delegate bool CloseEvent();
public delegate void Zoom(float value);

public class MobileUI : MonoBehaviour {

    public event SnapImage OnSnapImage;
    public event CloseEvent OnCloseAction;
    public event Zoom OnZoom;

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

    public void Zoom(Slider slider)
    {
        if (OnZoom != null)
            OnZoom(slider.value);
    }
}
