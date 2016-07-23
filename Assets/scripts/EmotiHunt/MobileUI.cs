using UnityEngine;
using UnityEngine.UI;

public delegate void SnapImage();
public delegate bool CloseEvent();
public delegate void Zoom(float value);
public delegate void UIViewMode(UIMode mode);

public class MobileUI : MonoBehaviour {

    public event SnapImage OnSnapImage;
    public event CloseEvent OnCloseAction;
    public event Zoom OnZoom;

    public event UIViewMode OnModeChange;

    [SerializeField] UIMode _viewMode = UIMode.Composing;

    public UIMode viewMode
    {
        get { return _viewMode; }
    }

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

    public void Play()
    {
        _viewMode = UIMode.Composing;
        if (OnModeChange != null)
            OnModeChange(_viewMode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_viewMode == UIMode.Composing)
            {
                _viewMode = UIMode.Selecting;
            } else if (_viewMode == UIMode.Selecting)
            {
                _viewMode = UIMode.Feed;

            } else
            {
                _viewMode = UIMode.Quitting;
            }
            if (OnModeChange != null)
            {
                OnModeChange(_viewMode);
            }
            if (_viewMode == UIMode.Quitting)
                QuitApp();
        }
    }
}
