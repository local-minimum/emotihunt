using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIButton : MonoBehaviour {

    [SerializeField]
    Detector detector;

    [SerializeField]
    List<DetectorStatus> interactableStatuses;

    [SerializeField] Selectable uiItem;

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
        uiItem.interactable = interactableStatuses.Contains(status);
    }
}
