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

    [SerializeField]
    Text statusTextField;

    [SerializeField]
    Detector detector;

    public UIMode viewMode
    {
        get { return _viewMode; }
    }

    void OnEnable()
    {        
        detector.OnProgressEvent += HandleProgressEvent;
        StartCoroutine(detector.SetupEmojis());
    }

    void OnDisable()
    {
        detector.OnProgressEvent -= HandleProgressEvent;
    }

    private void HandleProgressEvent(ProgressType t, string message, float progress)
    {
        statusTextField.text = message;
        Debug.Log(message);
    }

    public void QuitApp()
    {
        bool caught = false;
        if (OnCloseAction != null)
        {
            foreach (CloseEvent e in OnCloseAction.GetInvocationList())
            {
                if (e())
                {
                    caught = true;
                }

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
        HandleProgressEvent(ProgressType.Detector, "Compose image", 0);
        if (OnModeChange != null)
            OnModeChange(_viewMode);
    }    

    void Update()
    {
        //TODO: A bit of logic conflict with same view having several modes.
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
            Debug.Log(_viewMode);
            if (_viewMode == UIMode.Quitting)
                QuitApp();
        }
    }
}
