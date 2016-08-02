using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIButton : MonoBehaviour {

    [SerializeField]
    Detector detector;

    [SerializeField]
    List<DetectorStatus> interactableStatuses;

    [SerializeField] Selectable uiItem;

    [SerializeField]
    bool removeWhenNotInteractable = false;

    void OnEnable()
    {
        detector.OnDetectorStatusChange += HandleStatusChange;
        HandleStatusChange(detector, detector.Status);
    }

    void OnDisable()
    {
        detector.OnDetectorStatusChange -= HandleStatusChange;
    }

    private void HandleStatusChange(Detector screen, DetectorStatus status)
    {
        if (removeWhenNotInteractable)
        {
            uiItem.enabled = interactableStatuses.Contains(status);
            foreach (Image img in uiItem.GetComponentsInChildren<Image>())
            {
                img.enabled = uiItem.enabled;
            }
        }
        else {
            uiItem.interactable = interactableStatuses.Contains(status);
        }
    }
}
