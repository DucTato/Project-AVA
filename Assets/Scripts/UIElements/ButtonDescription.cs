using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDescription : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]

    private TextMeshProUGUI descriptionText;
    private void OnEnable()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject) descriptionText.enabled = true; 
        else descriptionText.enabled = false;
    }
    public void OnSelect(BaseEventData eventData)
    {
        descriptionText.enabled = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        descriptionText.enabled = false;
    }
}
