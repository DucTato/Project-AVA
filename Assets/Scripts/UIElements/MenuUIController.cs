using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MaskTransitions;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MenuUIController : MonoBehaviour
{
    [SerializeField, Foldout("Title Screen")]
    private GameObject switchON, switchOFF;
    [SerializeField, Foldout("Main Menu")]
    private GameObject environmentPanel, mainmenuPanel, creditPanel, quitPanel, optionPanel, missionModePanel, setupAircraftPanel;

    [SerializeField, Foldout("Aircraft setups")]
    private int currentAircraft, currentSpecialItem;
    [SerializeField, Foldout("Aircraft setups")]
    private GameObject[] aircraftPrefabs, specialItems;
    [SerializeField, Foldout("Aircraft setups")]
    private Image currentAircraftImg, currentSpecialItemImg;
    [SerializeField, Foldout("Aircraft setups")]
    private TextMeshProUGUI currentAircraftName, currentSpecialItemName, aircraftDescription, itemDescription;

    [SerializeField, Foldout("Options")]
    private GameObject[] subMenus;
    [SerializeField, Foldout("Options")]
    private Button[] optionCategories;
    //private int optionIndex;

    private GameObject prevPanel, currPanel;
    #region CallBacks
    // Start is called before the first frame update
    private void Awake()
    {
        switchON.SetActive(false);
        switchOFF.SetActive(true);
        
        // Hide that mouse input if a controller is detected
        if (Input.GetJoystickNames().Length > 0 ) Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        
        TransitionManager.Instance.PlayEndHalfTransition(1.2f);
        // Adds listeners to option category buttons
        for (int i = 0; i < optionCategories.Length; i++)
        {
            int tempValue = i;
            optionCategories[i].onClick.AddListener(() => OptionSelect(tempValue));
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switchOFF.SetActive(false);
            switchON.SetActive(true);
            currPanel = mainmenuPanel;
        }
    }
    #endregion
    #region Input Handler
    public void OnStartButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            switchON.SetActive(true);
            switchOFF.SetActive(false);
            
            GameManager.instance.SetWorldCenter(GameObject.Find("WorldCenter"));

            // GameMaster also has itself a "PlayerInput" component, MainMenu will override that
            GameManager.instance.GetComponent<PlayerInput>().enabled = false;
            PlayerTracker.instance.PlaceDownPlayer(true);
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
            if (EventSystem.current.isActiveAndEnabled) EventSystem.current.SetSelectedGameObject(currPanel.GetComponent<Panel>().GetFirstOption());
        }
    }
    #endregion
    #region Main Menu
    public void OnPlayButton()
    {
        mainmenuPanel.SetActive(!mainmenuPanel.activeInHierarchy);
        environmentPanel.SetActive(true);
        currPanel = environmentPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnOptionButton()
    {
        mainmenuPanel.SetActive(!mainmenuPanel.activeInHierarchy);
        optionPanel.SetActive(true);
        currPanel = optionPanel ;
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
        environmentPanel.SetActive(!environmentPanel.activeInHierarchy);
        currPanel = missionModePanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    public void OnStartGameButton()
    {
        // Switches scene
        TransitionManager.Instance.LoadLevel("Mission_Land");
    }
    public void OnSetupAircraftButton()
    {
        setupAircraftPanel.SetActive(true);
        currPanel = setupAircraftPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
    }
    #endregion
    #region MISSION Mode Menu
    public void OnOpenMissionPanel()
    {
        // Confirms the aircraft setups once upon opening
        ConfirmAircraftSetup();
    }
    public void OnOpenAircraftSetupPanel()
    {
        UpdateSelectionDescription();
    }
    public void OnConfirmAircraftButton()
    {
        // Turns off the current panel, returns to previous
        currPanel.SetActive(false);
        prevPanel.SetActive(true);
        currPanel = prevPanel;
        prevPanel = currPanel.GetComponent<Panel>().GetPrevious();
        EventSystem.current.SetSelectedGameObject(currPanel.GetComponent<Panel>().GetFirstOption());
    }
    public void ConfirmAircraftSetup() 
    {
        // This method will be executed in the On Close() Event
        PlayerTracker.instance.SetAircraft(aircraftPrefabs[currentAircraft], specialItems[currentSpecialItem]);
        UpdateSelectionInfo();
        
    }
    public void AircraftDropDown(int option)
    {
        currentAircraft = option;
        UpdateSelectionDescription();
    }
    public void ItemDropDown(int option)
    {
        currentSpecialItem = option;
        UpdateSelectionDescription();
    }
    private void UpdateSelectionInfo()
    {
        currentAircraftImg.sprite = aircraftPrefabs[currentAircraft].GetComponent<ObjectInfo>().Thumbnail();
        currentAircraftName.text = aircraftPrefabs[currentAircraft].name;
        //
        currentSpecialItemImg.sprite = specialItems[currentSpecialItem].GetComponent<ObjectInfo>().Thumbnail();
        currentSpecialItemName.text = specialItems[currentSpecialItem].name;
    }
    private void UpdateSelectionDescription()
    {
        aircraftDescription.text = aircraftPrefabs[currentAircraft].GetComponent<ObjectInfo>().Description();
        itemDescription.text = specialItems[currentSpecialItem].GetComponent<ObjectInfo>().Description();
    }
    #endregion
    #region OPTION Menu
    public void OnOpenOptionPanel()
    {
        OptionSelect(0);
    }
    private void OptionSelect(int index)
    {
        // Disables all the other submenu except for subMenu[index]
        for (int i = 0; i < subMenus.Length; i++)
        {
            if (i == index) subMenus[i].SetActive(true);
            else subMenus[i].SetActive(false);
        }
    }
    #endregion
}
