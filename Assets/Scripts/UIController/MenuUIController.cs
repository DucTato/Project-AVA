using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MaskTransitions;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System.Net;

public class MenuUIController : MonoBehaviour
{
    public static MenuUIController instance;
    [SerializeField, Foldout("Title Screen")]
    private GameObject switchON, switchOFF;
    [SerializeField, Foldout("Main Menu")]
    private GameObject environmentPanel, mainmenuPanel, creditPanel, quitPanel, optionPanel, missionModePanel, setupAircraftPanel, onScreenKeyboard;

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
    [SerializeField, Foldout("Options")]
    private TMP_InputField nameInput, callsignInput;
    [SerializeField, Foldout("Options")]
    private GameObject lastSelection;
    [SerializeField, Foldout("Options")]
    private Slider masterSlider, effectSlider, musicSlider;
    [SerializeField, Foldout("Options")]
    private TextMeshProUGUI masterText, effectText, musicText;
    [SerializeField, Foldout("Options")]
    private Toggle ppToggle;
    //private int optionIndex;
    [SerializeField]
    private GameObject prevPanel, currPanel;
    #region CallBacks
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        switchON.SetActive(false);
        switchOFF.SetActive(true);
        
        // Hide that mouse input if a controller is detected
        //if (Input.GetJoystickNames().Length > 0 ) Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        // First attempt to disable the "Gameplay" Map. It didn't work
        UnityEvent updateInputMap = new UnityEvent();
        updateInputMap.AddListener(() => PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable());
        updateInputMap.Invoke();

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
        // Legacy input system, used for debugging purposes
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //switchON.SetActive(true);
            //switchOFF.SetActive(false);
            //GameManager.instance.SetWorldCenter(GameObject.Find("WorldCenter"));
            //PlayerTracker.instance.PlaceDownPlayer(true);
            //currPanel = mainmenuPanel;
            PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable();
        }
    }
    #endregion
    #region Input Handler
    public void OnStartButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            if (!switchOFF.activeInHierarchy) return;
            switchON.SetActive(true);
            switchOFF.SetActive(false);
            GameManager.instance.SetWorldCenter(GameObject.Find("WorldCenter"));
            PlayerTracker.instance.PlaceDownPlayer(true);
            GameManager.instance.StartPlacingEnemies();
            currPanel = mainmenuPanel;
            // For some reason, only this call will disable the "Gameplay" Map
            PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable();
        }
    }
    public void OnCancelButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
            {
                //Debug.Log("Press B on an Input Field");
                EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>().onEndEdit?.Invoke(null);
            }
            else
            {
                // Press B on the main menu to quit
                if (currPanel == mainmenuPanel)
                {
                    OnQuitButton();
                }
                if (EventSystem.current.currentSelectedGameObject.GetComponent<Slider>() != null)
                {
                    // Press B on the slider to return to previous button
                    EventSystem.current.SetSelectedGameObject(lastSelection);
                    return;
                }
                //
                // Press B on the any panel to return to previous panel
                if (currPanel == null) return;
                ClosePanel(currPanel);
                OpenPanel(prevPanel);
                currPanel = prevPanel;
                SetPreviousPanel(currPanel);
                if (EventSystem.current.isActiveAndEnabled) EventSystem.current.SetSelectedGameObject(currPanel.GetComponent<Panel>().GetFirstOption());
            }
        }
    }
    private void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }
    private void ClosePanel(GameObject panel)
    {
        panel.GetComponent<Panel>().Close();
    }
    private void SetPreviousPanel(GameObject panel)
    {
        prevPanel = panel.GetComponent<Panel>().GetPrevious();
    }
    
    #endregion
    #region Main Menu
    public void PanelOverride(GameObject current, GameObject previous)
    { 
        currPanel = current;
        prevPanel = previous;
    }
    public void OnPlayButton()
    {
        ClosePanel(mainmenuPanel);
        OpenPanel(environmentPanel);
        currPanel = environmentPanel;
        SetPreviousPanel(currPanel);
    }
    public void OnOptionButton()
    {
        ClosePanel(mainmenuPanel);
        OpenPanel(optionPanel);
    }
    public void OnCreditButton()
    {
        ClosePanel(mainmenuPanel);
        OpenPanel(creditPanel);
        currPanel = creditPanel;
        SetPreviousPanel(currPanel);
    }
    public void OnQuitButton()
    {
        ClosePanel(mainmenuPanel);
        OpenPanel(quitPanel);
        currPanel = quitPanel;
        SetPreviousPanel(currPanel);
    }
    public void OnYesQuitButton()
    {
        Application.Quit();
    }
    public void OnNoQuitButton()
    {
        if (currPanel == null) return;
        ClosePanel(currPanel);
        OpenPanel(prevPanel);
        currPanel = prevPanel;
        SetPreviousPanel(currPanel);
    }
    public void OnMissionModeButton()
    {
        OpenPanel(missionModePanel);
        ClosePanel(environmentPanel);
        currPanel = missionModePanel;
        SetPreviousPanel(currPanel);
    }
    public void OnStartGameButton()
    {
        PlayerTracker.instance.SetSpawnHeight(1000f);
        // Switches scene
        TransitionManager.Instance.LoadLevel("Mission_Land");
    }
    public void OnSetupAircraftButton()
    {
        OpenPanel(setupAircraftPanel);
        currPanel = setupAircraftPanel;
        SetPreviousPanel(currPanel);
    }
    #endregion
    #region Pre Mission Menu
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
        ClosePanel(currPanel);
        OpenPanel(prevPanel);
        currPanel = prevPanel;
        SetPreviousPanel(currPanel);
        EventSystem.current.SetSelectedGameObject(currPanel.GetComponent<Panel>().GetFirstOption());
    }
    public void ConfirmAircraftSetup() 
    {
        // This method will be executed in the On Close() Event
        currentAircraft = Mathf.Clamp( currentAircraft, 0, aircraftPrefabs.Length - 1);
        currentSpecialItem = Mathf.Clamp(currentSpecialItem, 0, specialItems.Length - 1);
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
    #region Gameplay Menu
    public void OnNameInputButton()
    {
        nameInput.interactable = true;
        lastSelection = EventSystem.current.currentSelectedGameObject;
        nameInput.Select();
        //onScreenKeyboard.SetActive(true);
    }
    public void OnCallsignInputButton()
    {
        callsignInput.interactable = true;
        lastSelection = EventSystem.current.currentSelectedGameObject;
        callsignInput.Select();
        //onScreenKeyboard.SetActive(true);
    }
    public void OnDeSelectInputField()
    {
        nameInput.interactable = false;
        callsignInput.interactable = false;
        EventSystem.current.SetSelectedGameObject(lastSelection);
    }
    #endregion
    #region Audio Menu
    public void OnMasterButton()
    {
        lastSelection = EventSystem.current.currentSelectedGameObject;
        masterSlider.Select();
    }
    public void OnEffectButton()
    {
        lastSelection = EventSystem.current.currentSelectedGameObject;
        effectSlider.Select();
    }
    public void OnMusicButton()
    {
        lastSelection = EventSystem.current.currentSelectedGameObject;
        musicSlider.Select();
    }
    public void OnUpdateSliderValues()
    {
        // Handle values from the sliders
        masterSlider.value = Mathf.Round(masterSlider.value * 10f) / 10f;   // Rounding to the first decimal point
        effectSlider.value = Mathf.Round(effectSlider.value * 10f) / 10f;
        musicSlider.value = Mathf.Round(musicSlider.value * 10f) / 10f;
        masterText.text = string.Format("{0:0}%", masterSlider.value * 100);    // Formatting Ex: 10%, 20%
        effectText.text = string.Format("{0:0}%", effectSlider.value * 100);
        musicText.text = string.Format("{0:0}%", musicSlider.value * 100);
    }
    #endregion
    #region Graphic Menu
    public void OnPPToggle()
    {
        GameObject.FindGameObjectWithTag("PPVolume").GetComponent<Volume>().enabled = ppToggle.isOn;
        PlayerTracker.instance.SetPostProcessing(ppToggle.isOn);
    }
    public void OnUpdatePP()
    {
        ppToggle.isOn = GameObject.FindGameObjectWithTag("PPVolume").GetComponent<Volume>().enabled;
    }
    #endregion
    public void OnOpenOptionPanel()
    {
        // Initial setups 
        OptionSelect(0);
        nameInput.interactable = false;
        callsignInput.interactable = false;
    }
    private void OptionSelect(int index)
    {
        // Disables all the other submenu except for subMenu[index]
        for (int i = 0; i < subMenus.Length; i++)
        {
            if (i == index) subMenus[i].SetActive(true);
            else subMenus[i].SetActive(false);
        }
        currPanel = optionPanel;
        prevPanel = mainmenuPanel;
    }
    #endregion
}
