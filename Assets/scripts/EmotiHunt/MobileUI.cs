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
    Image progressImage;

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
        progressImage.fillAmount = progress;
        Debug.Log(message);
    }

    public void SetStatus(string message)
    {
        SetStatus(message, 0);
    }

    public void SetStatus(string message, float progress)
    {
        statusTextField.text = message;
        progressImage.fillAmount = progress;
    }

    public void Abort()
    {
        if (_viewMode == UIMode.Composing)
        {
            _viewMode = UIMode.Selecting;
        } else if (_viewMode == UIMode.CompositionPhoto)
        {
            _viewMode = UIMode.Composing;
        }
        else if (_viewMode == UIMode.Selecting || _viewMode == UIMode.About)
        {
            _viewMode = UIMode.Feed;

        }
        else
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

    void QuitApp() { 

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
        HandleProgressEvent(ProgressType.Detector, "Processing (takes a while)", 0);
        if (OnSnapImage != null)
        {
            OnSnapImage();
        }

        _viewMode = UIMode.CompositionPhoto;
        if (OnModeChange != null)
        {
            OnModeChange(_viewMode);
        }
    }

    public void Zoom(Slider slider)
    {
        if (OnZoom != null)
            OnZoom(slider.value);
    }

    public void AboutHelp()
    {
        _viewMode = UIMode.About;
        if (OnModeChange != null)
            OnModeChange(_viewMode);

    }

    public void Play()
    {
        if (_viewMode == UIMode.Feed)
        {
            _viewMode = UIMode.Selecting;
        }
        else if (_viewMode == UIMode.Selecting)
        {
            _viewMode = UIMode.Composing;
            HandleProgressEvent(ProgressType.Detector, "Compose image", 0);
        } else
        {
            return;
        }

        if (OnModeChange != null)
            OnModeChange(_viewMode);
    }    

    void Update()
    {
        //TODO: A bit of logic conflict with same view having several modes.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Abort();
        }
    }
}
