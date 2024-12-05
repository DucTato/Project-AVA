using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Panel : MonoBehaviour
{
    [SerializeField]
    private GameObject firstOption, previousPanel;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstOption);
    }
    public GameObject GetPrevious()
    {
        return previousPanel;
    }
}
