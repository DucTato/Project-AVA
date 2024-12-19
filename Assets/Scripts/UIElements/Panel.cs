using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Panel : MonoBehaviour
{
    [Foldout("Events")]
    public UnityEvent onOpen, onClose;
    [SerializeField]
    private bool DrawChildOnStart, PersistWithParent;
    [SerializeField]
    private GameObject firstOption, previousPanel, childPanel;
    
    private void OnEnable()
    {
        if (firstOption != null && firstOption != EventSystem.current.currentSelectedGameObject) EventSystem.current.SetSelectedGameObject(firstOption);
        if (DrawChildOnStart) childPanel.SetActive(true);
        onOpen?.Invoke();
    }
    private void OnDisable()
    {
        onClose?.Invoke();
    }
    public void DrawChild()
    {
        childPanel.SetActive(true);
    }
    public GameObject GetPrevious()
    {
        return previousPanel;
    }
    public GameObject GetFirstOption()
    {
        return firstOption;
    }
    public void Close()
    {
        if (PersistWithParent) gameObject.SetActive(transform.parent.gameObject.activeInHierarchy);
        else gameObject.SetActive(false);
    }
    public void SetPanelOverride()
    {
        MenuUIController.instance.PanelOverride(gameObject, previousPanel);
    }
}
