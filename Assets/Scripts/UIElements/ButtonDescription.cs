using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDescription : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private TextMeshProUGUI targetTextBox;
    [SerializeField]
    private string descriptionText;
    private void OnEnable()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject) targetTextBox.text = descriptionText;
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (targetTextBox == null)
        {
            Debug.Log(gameObject.name + "doesn't have a description but an event is accessing it!");
            return; 
        }
        
        targetTextBox.text = descriptionText;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (targetTextBox == null)
        {
            Debug.Log(gameObject.name + "doesn't have a description but an event is accessing it!");
            return;
        }
        targetTextBox.text = "";
    }
}
