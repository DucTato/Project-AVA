using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEventHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    private bool SkipSelect;
    [SerializeField]
    private TextMeshProUGUI targetTextBox;
    [SerializeField]
    private string descriptionText;
    private void OnEnable()
    {
        if (targetTextBox != null) 
            if (EventSystem.current.currentSelectedGameObject == gameObject) targetTextBox.text = descriptionText;
    }
    private void OnDisable()
    {
        if (targetTextBox == null)
        {
            return;
        }
        targetTextBox.text = "";
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (SkipSelect)
        {
            GetComponent<Button>().onClick.Invoke();
        }
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
            return;
        }
        targetTextBox.text = "";
    }
}
