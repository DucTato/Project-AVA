using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MaskTransitions;


public class MenuUIController : MonoBehaviour
{
    [SerializeField, Foldout("Title Screen")]
    private GameObject switchON, switchOFF;
    [SerializeField, Foldout("Main Menu")]
    private GameObject gamemodePanel, mainmenuPanel, creditPanel, quitPanel, missionModePanel, setupAircraftPanel;

    private GameObject prevPanel, currPanel;

    // Start is called before the first frame update
    void Start()
    {
        switchON.SetActive(false);
        switchOFF.SetActive(true);
        TransitionManager.Instance.PlayEndHalfTransition(1.2f);
    }

    // Update is called once per frame
    void Update()
    {

    }
    #region Input Handler
    public void OnStartButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            switchOFF.SetActive(false);
            switchON.SetActive(true);
            currPanel = mainmenuPanel;
           
        }
    }
    public void OnCancelButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            if (currPanel == mainmenuPanel || currPanel == null) return;
            currPanel.SetActive(false);
            prevPanel.SetActive(true);
            currPanel = prevPanel;
            prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
        }
    }
    #endregion
    #region Main Menu
    public void OnPlayButton()
    {
        mainmenuPanel.SetActive(!mainmenuPanel.activeInHierarchy);
        gamemodePanel.SetActive(true);
        currPanel = gamemodePanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnCreditButton()
    {
        mainmenuPanel.SetActive(!mainmenuPanel.activeInHierarchy);
        creditPanel.SetActive(true);
        currPanel = creditPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnQuitButton()
    {
        mainmenuPanel.SetActive(!mainmenuPanel.activeInHierarchy);
        quitPanel.SetActive(true);
        currPanel = quitPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnYesQuitButton()
    {
        Application.Quit();
    }
    public void OnNoQuitButton()
    {
        if (currPanel == null) return;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
        currPanel.SetActive(false);
        prevPanel.SetActive(true);
        currPanel = prevPanel;
    }
    public void OnMissionModeButton()
    {
        missionModePanel.SetActive(true);
        gamemodePanel.SetActive(!gamemodePanel.activeInHierarchy);
        currPanel = missionModePanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnStartGameButton()
    {
        // Switch scene
        TransitionManager.Instance.LoadLevel("Mission_Land");
    }
    public void OnSetupAircraftButton()
    {
        setupAircraftPanel.SetActive(true);
        currPanel = setupAircraftPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    #endregion
}
