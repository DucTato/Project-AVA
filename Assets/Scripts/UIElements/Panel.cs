using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Panel : MonoBehaviour
{
    public UnityEvent onOpen;
    public UnityEvent onClose;
    [SerializeField]
    private GameObject firstOption, previousPanel, childPanel;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstOption);
        if (childPanel!= null ) childPanel.SetActive(false);
        onOpen?.Invoke();
    }
    private void OnDisable()
    {
        onClose?.Invoke();
    }
    public GameObject GetPrevious()
    {
        return previousPanel;
    }
    public GameObject GetFirstOption()
    {
        return firstOption;
    }
}
