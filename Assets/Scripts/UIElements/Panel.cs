using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Panel : MonoBehaviour
{
    [SerializeField]
    private GameObject firstOption, previousPanel, childPanel;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstOption);
        if (childPanel!= null ) childPanel.SetActive(false);
    }
    public GameObject GetPrevious()
    {
        return previousPanel;
    }
}
