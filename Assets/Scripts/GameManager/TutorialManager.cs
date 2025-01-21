using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private Sprite[] tutorialStepImages;
    [SerializeField]
    private string[] tutorialMessages;
    [SerializeField]
    private TextMeshProUGUI tutorialText;
    [SerializeField]
    private Image currentStep;
    [SerializeField]
    private Image controllerImage;
    [SerializeField]
    private int tutorialStepCount;
    [SerializeField]
    [Tooltip("How long between each tutorial step, measured in second(s)")]
    private float tutorialInterval;
    [SerializeField]
    private GameObject tutorialGo, promptPanel;

    private bool buttonPrompt;

    /// <summary>
    /// Step        Function
    /// 0           Pitch
    /// 1           Roll
    /// 2           Yaw
    /// 3           Throttle    
    /// 4           Targeting
    /// 6           Weapons
    /// 7           Progress
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(WaitThenDo(tutorialInterval));
        tutorialGo.SetActive(false);
        promptPanel.SetActive(false);
        buttonPrompt = false;
        // turns off the main menu camera
        GameObject.FindGameObjectWithTag("MainMenuCam").GetComponent<CinemachineFreeLook>().Priority = 0;
    }
    private void OnDisable()
    {
        StopAllCoroutines();

    }
    private void Start()
    {
        
        tutorialStepCount = 0;
        PlayerController.instance.fireControl.ToggleInfiniteAmmo(true);

    }
    private void Update()
    {
        if(buttonPrompt)
        {
            ShowTutorialImage(tutorialStepCount);
        }
    }
    private void NextStep()
    {
        Time.timeScale = 1f;
        buttonPrompt = false;
        tutorialGo.SetActive(false);
        tutorialStepCount++;
        if (tutorialStepCount >= tutorialStepImages.Length) 
        {
            CompleteTutorial();
            return;
        }
        StartCoroutine(WaitThenDo(tutorialInterval));
    }
    private void CompleteTutorial()
    {
        tutorialGo.SetActive(true);
        currentStep.enabled = false;
        tutorialText.text = "";
        controllerImage.enabled = false;
        promptPanel.SetActive(true);  
    }
    private void ShowTutorialImage(int step)
    {
        Time.timeScale = 0f;
        // Activate the tutorial panel and set values
        tutorialGo.SetActive(true);
        currentStep.enabled = true;
        controllerImage.enabled = true;
        currentStep.sprite = tutorialStepImages[step];
        tutorialText.text = tutorialMessages[step];
        if (tutorialStepImages[step] == null)
        {
            currentStep.enabled = false;
            controllerImage.enabled = false;
        }
        PlayerController.instance.ToggleAi(false);
        switch (step)
        {
            case 0:
                if (PlayerController.instance.playerInput.actions["ACmovement"].IsPressed())
                    NextStep();
                break;
            case 1:
                if (PlayerController.instance.playerInput.actions["ACmovement"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 2:
                if (PlayerController.instance.playerInput.actions["ACyaw"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 3:
                if (PlayerController.instance.playerInput.actions["ACthrottle"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 4:
                if (PlayerController.instance.playerInput.actions["ACcycleTgt"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 5:
                if (PlayerController.instance.playerInput.actions["ACfireGUN"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 6:
                if (PlayerController.instance.playerInput.actions["ACfireMSL"].WasPerformedThisFrame())
                    NextStep();
                break;
            case 7:
                // This step doesn't need to listen for button events
                StartCoroutine(UnPauseAfterDuration(3f));
                break;
        }
    }
    private IEnumerator WaitThenDo(float interval)
    {
        yield return new WaitForSeconds(interval);
        buttonPrompt = true;
    }
    private IEnumerator UnPauseAfterDuration(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        NextStep();
    }
    public void OnYesButton()
    {
        promptPanel.SetActive(false);
    }
    public void OnNoButton()
    {
        // Stop the tutorial, return to main menu
        // Reuse the quit function :>
        GameManager.instance.hudController.OnQuitSessionButton();       
    }
}
