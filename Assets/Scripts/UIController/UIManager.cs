using MaskTransitions;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("AVIONICS")]
    [SerializeField] [Foldout("Avionics")]
    float updateRate;
    [SerializeField][Foldout("Avionics")]
    Color normalColor;
    [SerializeField][Foldout("Avionics")]
    Color lockColor;
    [SerializeField][Foldout("Avionics")]
    Compass compass;
    [SerializeField][Foldout("Avionics")]
    PitchLadder pitchLadder;
    [SerializeField][Foldout("Avionics")]
    Bar throttleBar;
    [SerializeField][Foldout("Avionics")]
    private Transform hudCenter;
    [SerializeField][Foldout("Avionics")]
    private Transform stillComponents;
    [SerializeField][Foldout("Avionics")]
    Transform velocityMarker;
    [SerializeField][Foldout("Avionics")]
    Text airspeed;
    [SerializeField][Foldout("Avionics")]
    Text aoaIndicator;
    [SerializeField][Foldout("Avionics")]
    Text gforceIndicator;
    [SerializeField][Foldout("Avionics")]
    Text altitude;
    [SerializeField][Foldout("Avionics")]
    Bar healthBar;
    [SerializeField][Foldout("Avionics")]
    Text healthText;
    [SerializeField][Foldout("Avionics")]
    Transform targetBox;
    [SerializeField][Foldout("Avionics")]
    Text targetName;
    [SerializeField][Foldout("Avionics")]
    Text targetRange;
    [SerializeField][Foldout("Avionics")]
    Transform missileLock;
    [SerializeField][Foldout("Avionics")]
    Transform reticle;
    [SerializeField][Foldout("Avionics")]
    RectTransform reticleLine;
    [SerializeField][Foldout("Avionics")]
    RectTransform targetArrow;
    [SerializeField][Foldout("Avionics")]
    RectTransform missileArrow;
    [SerializeField][Foldout("Avionics")]
    float targetArrowThreshold;
    [SerializeField][Foldout("Avionics")]
    float missileArrowThreshold;
    [SerializeField][Foldout("Avionics")]
    float cannonRange;
    [SerializeField][Foldout("Avionics")]
    float bulletSpeed;
    [SerializeField][Foldout("Avionics")]
    private float hpUpTime;
    [SerializeField][Foldout("Avionics")]
    GameObject aiMessage;
    [SerializeField][Foldout("Avionics")]
    private GameObject mslWarning;
    [SerializeField][Foldout("Avionics")]
    private GameObject avionics;
    [SerializeField][Foldout("Avionics")]
    List<Graphic> missileWarningGraphics;
    
    [Header("MISCS")]
    [SerializeField, Foldout("Miscs")]
    private Bar progressBar;
    [SerializeField, Foldout("Miscs")]
    private TextMeshProUGUI currentTargetInfo, ammoInfo;
    [SerializeField, Foldout("Miscs")]
    private GameObject deathPanel, gameInfo, winPanel;
    

    [SerializeField, Foldout("PauseMenu")]
    private GameObject pausePanel, shopPanel, optionPanel;
    [SerializeField, Foldout("PauseMenu")]
    private TextMeshProUGUI aptScoreText;
    [SerializeField, Foldout("ShopMenu")]
    private TextMeshProUGUI pointTxt;

    private GameObject prevPanel, currPanel;
    private GameObject noPpLayer;
    private PlaneHandler planeHandler;
    private FCS fireControl;
    //Target target;
    AIController aiController;
    private Target selfTarget;
    Transform planeTransform;
    [SerializeField]
    new Camera targetCam;
    Transform cameraTransform;

    GameObject hudCenterGO;
    GameObject velocityMarkerGO;
    GameObject targetBoxGO;
    Image targetBoxImage;
    GameObject missileLockGO;
    Image missileLockImage;
    GameObject reticleGO;
    GameObject targetArrowGO;
    GameObject missileArrowGO;

    private float lastUpdateTime;
    private float HPupTimer, alphaValue;
    private bool hideHP,isPaused;


    const float metersToKnots = 1.94384f;
    const float metersToFeet = 3.28084f;
    public bool IsPaused
    {
        get
        {
            return IsPaused;
        }
        set
        {
            if (!GameManager.instance.GamePhase) return;    // Can't pause during the loading screen
            isPaused = value;
            if (isPaused)
            {
                Time.timeScale = 0f;
                //PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable();
                //PlayerController.instance.playerInput.actions.FindActionMap("UI").Enable();
                Switch2UIMap();
                noPpLayer.SetActive(false);
                OpenPanel(pausePanel);
            }
            else
            {
                Time.timeScale = 1f;
                //PlayerController.instance.playerInput.actions.FindActionMap("UI").Disable();
                //PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Enable();
                Switch2GameMap();
                noPpLayer.SetActive(true);
                ClosePanel(pausePanel);
            }
        }
    }
    public bool IsPlaneActive { get; set; }
    #region CallBacks
    private void Awake()
    {
        ToggleCanvas(false);
        hudCenterGO = hudCenter.gameObject;
        velocityMarkerGO = velocityMarker.gameObject;
        targetBoxGO = targetBox.gameObject;
        targetBoxImage = targetBox.GetComponent<Image>();
        missileLockGO = missileLock.gameObject;
        missileLockImage = missileLock.GetComponent<Image>();
        reticleGO = reticle.gameObject;
        targetArrowGO = targetArrow.gameObject;
        missileArrowGO = missileArrow.gameObject;
        HPupTimer = 0f;
        alphaValue = 1f;
        hideHP = true;
        SetDeathScreen(false);
        SetWinScreen(false);
        
    }
    private void Start()
    {
        IsPaused = false;
        noPpLayer = transform.GetChild(0).gameObject;
    }
    void LateUpdate()
    {
        if (planeHandler == null || !IsPlaneActive)
        {

            return;
        }
        if (targetCam == null)
        {

            return;
        }
        
        //float degreesToPixels = targetCam.pixelHeight / targetCam.fieldOfView;

        throttleBar.SetValue(planeHandler.Throttle);

        if (!planeHandler.Dead)
        {
            UpdateVelocityMarker();
            UpdateHUDCenter();
        }
        else
        {
            hudCenterGO.SetActive(false);
            velocityMarkerGO.SetActive(false);
        }

        if (aiController != null)
        {
            aiMessage.SetActive(aiController.enabled);
        }

        UpdateAirspeed();
        UpdateAltitude();
        UpdateHealth();
        UpdateWeapons();
        UpdateWarnings();

        //update these elements at reduced rate to make reading them easier
        if (Time.time > lastUpdateTime + (1f / updateRate))
        {
            UpdateAOA();
            UpdateGForce();
            lastUpdateTime = Time.time;
        }
        
    }
    #endregion

    
    #region Avionics
    public void ToggleAvionics(bool value)
    {
        avionics.SetActive(value);
    }
    public void DisplayHP()
    {
        HPupTimer = hpUpTime;
    }
    private void UpdateVelocityMarker()
    {
        var velocity = planeTransform.forward;

        if (planeHandler.LocalVelocity.sqrMagnitude > 1)
        {
            velocity = planeHandler.rb.velocity;
        }

        var hudPos = TransformToHUDSpace(cameraTransform.position + velocity);

        if (hudPos.z > 0)
        {
            velocityMarkerGO.SetActive(true);
            velocityMarker.localPosition = new Vector3(hudPos.x, hudPos.y, 0);
        }
        else
        {
            velocityMarkerGO.SetActive(false);
        }
    }

    private void UpdateAirspeed()
    {
        var speed = planeHandler.LocalVelocity.z * metersToKnots;
        airspeed.text = string.Format("{0:0}", speed);
    }

    private void UpdateAOA()
    {
        aoaIndicator.text = string.Format("{0:0.0} AOA", planeHandler.AngleOfAttack * Mathf.Rad2Deg);
    }

    private void UpdateGForce()
    {
        var gforce = planeHandler.LocalGForce.y / 9.81f;
        gforceIndicator.text = string.Format("{0:0.0} G", gforce);
    }

    private void UpdateAltitude()
    {
        var altitude = planeHandler.rb.position.y * metersToFeet;
        this.altitude.text = string.Format("{0:0}", altitude);
    }

    Vector3 TransformToHUDSpace(Vector3 worldSpace)
    {
        // Not accounting for dynamic resolution scaling
        var screenSpace = targetCam.WorldToScreenPoint(worldSpace);
        return screenSpace - new Vector3(targetCam.pixelWidth / 2, targetCam.pixelHeight / 2);
        //var screenSpace = camera.WorldToScreenPoint(worldSpace);
        //var finalSP = new Vector3(screenSpace.x / GetComponent<Canvas>().scaleFactor, screenSpace.y / GetComponent<Canvas>().scaleFactor);
        //return finalSP;
    }

    private void UpdateHUDCenter()
    {
        var rotation = cameraTransform.localEulerAngles;
        var hudPos = TransformToHUDSpace(cameraTransform.position + planeTransform.forward);

        if (hudPos.z > 0)
        {
            hudCenterGO.SetActive(true);
            stillComponents.gameObject.SetActive(true);

            hudCenter.localPosition = new Vector3(hudPos.x, hudPos.y, 0);
            stillComponents.localPosition = hudCenter.localPosition;
            hudCenter.localEulerAngles = new Vector3(0, 0, -rotation.z);
        }
        else
        {
            hudCenterGO.SetActive(false);
            stillComponents.gameObject.SetActive(false);
        }
    }

    private void UpdateHealth()
    {
        if (HPupTimer > 0) 
        { 
            HPupTimer -= Time.deltaTime;
            hideHP = false;
            alphaValue = 1f;
            healthBar.SetAlpha(alphaValue);
            healthBar.SetValue(selfTarget.Health / selfTarget.MaxHealth);
            healthText.text = string.Format("{0:0}", selfTarget.Health + "%");
        } 
        else
        {
            hideHP = true;
        }
        if (hideHP)
        {
            alphaValue = Mathf.MoveTowards(alphaValue, 0f, 0.5f * Time.deltaTime);
            healthBar.SetAlpha(alphaValue);
        }
    }

    private void UpdateWeapons()
    {
        if (fireControl.currTarget == null)
        {
            targetBoxGO.SetActive(false);
            missileLockGO.SetActive(false);
            return;
        }

        //update target box, missile lock
        var targetDistance = Vector3.Distance(planeHandler.rb.position, fireControl.currTarget.Position);
        var targetPos = TransformToHUDSpace(fireControl.currTarget.Position);
        var missileLockPos = fireControl.MissileLocked ? targetPos : TransformToHUDSpace(planeHandler.rb.position + fireControl.MissileLockDirection * targetDistance);

        if (targetPos.z > 0)
        {
            targetBoxGO.SetActive(true);
            targetBox.localPosition = new Vector3(targetPos.x, targetPos.y, 0);
        }
        else
        {
            targetBoxGO.SetActive(false);
        }

        if (fireControl.MissileTracking && missileLockPos.z > 0)
        {
            missileLockGO.SetActive(true);
            missileLock.localPosition = new Vector3(missileLockPos.x, missileLockPos.y, 0);
        }
        else
        {
            missileLockGO.SetActive(false);
        }

        if (fireControl.MissileLocked)
        {
            targetBoxImage.color = lockColor;
            targetName.color = lockColor;
            targetRange.color = lockColor;
            missileLockImage.color = lockColor;
        }
        else
        {
            targetBoxImage.color = normalColor;
            targetName.color = normalColor;
            targetRange.color = normalColor;
            missileLockImage.color = normalColor;
        }

        targetName.text = fireControl.currTarget.Name;
        targetRange.text = string.Format("{0:0 m}", targetDistance);

        //update target arrow
        var targetDir = (fireControl.currTarget.Position - planeHandler.rb.position).normalized;
        var targetAngle = Vector3.Angle(cameraTransform.forward, targetDir);

        if (targetAngle > targetArrowThreshold)
        {
            targetArrowGO.SetActive(true);
            //add 180 degrees if target is behind camera
            float flip = targetPos.z > 0 ? 0 : 180;
            targetArrow.localEulerAngles = new Vector3(0, 0, flip + Vector2.SignedAngle(Vector2.up, new Vector2(targetPos.x, targetPos.y)));
        }
        else
        {
            targetArrowGO.SetActive(false);
        }

        //update target lead
        var leadPos = Utilities.FirstOrderIntercept(planeHandler.rb.position, planeHandler.rb.velocity, bulletSpeed, fireControl.currTarget.Position, fireControl.currTarget.Velocity);
        var reticlePos = TransformToHUDSpace(leadPos);

        if (reticlePos.z > 0 && targetDistance <= cannonRange)
        {
            reticleGO.SetActive(true);
            reticle.localPosition = new Vector3(reticlePos.x, reticlePos.y, 0);

            var reticlePos2 = new Vector2(reticlePos.x, reticlePos.y);
            if (Mathf.Sign(targetPos.z) != Mathf.Sign(reticlePos.z)) reticlePos2 = -reticlePos2;    //negate position if reticle and target are on opposite sides
            var targetPos2 = new Vector2(targetPos.x, targetPos.y);
            var reticleError = reticlePos2 - targetPos2;

            var lineAngle = Vector2.SignedAngle(Vector3.up, reticleError);
            reticleLine.localEulerAngles = new Vector3(0, 0, lineAngle + 180f);
            reticleLine.sizeDelta = new Vector2(reticleLine.sizeDelta.x, reticleError.magnitude);
        }
        else
        {
            reticleGO.SetActive(false);
        }
    }

    private void UpdateWarnings()
    {
        var incomingMissile = selfTarget.GetIncomingMissile();

        if (incomingMissile != null)
        {
            var missilePos = TransformToHUDSpace(incomingMissile.rb.position);
            //
            var missileDir = (incomingMissile.rb.position - planeHandler.rb.position).normalized;
            var missileAngle = Vector3.Angle(cameraTransform.forward, missileDir);
            mslWarning.SetActive(true);
            if (missileAngle > missileArrowThreshold)
            {
                missileArrowGO.SetActive(true);
                //add 180 degrees if target is behind camera
                float flip = missilePos.z > 0 ? 0 : 180;
                missileArrow.localEulerAngles = new Vector3(0, 0, flip + Vector2.SignedAngle(Vector2.up, new Vector2(missilePos.x, missilePos.y)));
            }
            else
            {
                missileArrowGO.SetActive(false);
            }

            foreach (var graphic in missileWarningGraphics)
            {
                graphic.color = lockColor;
            }

            pitchLadder.UpdateColor(lockColor);
            compass.UpdateColor(lockColor);
        }
        else
        {
            missileArrowGO.SetActive(false);
            mslWarning.SetActive(false);
            foreach (var graphic in missileWarningGraphics)
            {
                graphic.color = normalColor;
            }

            pitchLadder.UpdateColor(normalColor);
            compass.UpdateColor(normalColor);
        }
    }
    #endregion
    #region GameHUD
    public void UpdateAmmoHUD(int cannon, int missiles, int storedMissiles)
    {
        ammoInfo.text = string.Format("{0}\n{1} / {2}", cannon, missiles, storedMissiles);  
    }
    public void SetProgressBar(float current, float maxValue)
    {
        progressBar.SetValue(current / maxValue);
    }
    public void SetCurrentTargetInfo(Target target)
    {
        if (target == null) currentTargetInfo.text = " ";
        else currentTargetInfo.text = target.Name + " " + target.rewardPoint + "Pt";
    }
    public void SetDeathScreen(bool value)
    {
        deathPanel.SetActive(value);
        gameInfo.SetActive(!value);
    }
    public void SetWinScreen(bool value)
    {
        winPanel.SetActive(value);
    }
    public void ToggleCanvas(bool isVisible)
    {
        foreach (Canvas c in GetComponentsInChildren<Canvas>()) 
        {
            c.enabled = isVisible;
        }
        IsPlaneActive = isVisible;
    }
    public void TogglePause()
    {
        IsPaused = !isPaused;
    }
    public void SetPlane(PlaneHandler plane)
    {


        if (plane == null)
        {
            planeTransform = null;
            selfTarget = null;
        }
        else
        {
            planeHandler = plane;
            aiController = planeHandler.GetComponent<AIController>();
            planeTransform = planeHandler.GetComponent<Transform>();
            selfTarget = planeHandler.GetComponent<Target>();
            fireControl = planeHandler.GetComponent<FCS>();
        }

        if (compass != null)
        {
            compass.SetPlane(plane);
        }

        if (pitchLadder != null)
        {
            pitchLadder.SetPlane(plane);
        }
    }

    public void SetCamera(Camera camera)
    {


        if (camera == null)
        {
            cameraTransform = null;
        }
        else
        {
            targetCam = camera;
            cameraTransform = camera.GetComponent<Transform>();
        }

        if (compass != null)
        {
            compass.SetCamera(targetCam);
        }

        if (pitchLadder != null)
        {
            pitchLadder.SetCamera(targetCam);
        }
    }
    public void Switch2UIMap()
    {
        PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable();
        PlayerController.instance.SetCurrentInputMap("UI");
    }
    public void Switch2GameMap()
    {
        PlayerController.instance.playerInput.actions.FindActionMap("UI").Disable();
        PlayerController.instance.SetCurrentInputMap("Gameplay");
    }
    /// <summary>
    /// Handles input. However, this method is here for an easier time to debug. This is mainly for the UI for in-game. Not the UI for Main Menu
    /// </summary>
    public void OnEastButton()
    {
        // Press B on the any panel to return to previous panel
        if (currPanel == null) return;
        if (prevPanel == null) 
        {
            TogglePause();     //Press B to unpause
            return;
        }

        ClosePanel(currPanel);
        OpenPanel(prevPanel);
        currPanel = prevPanel;
        SetPreviousPanel(currPanel);
        if (EventSystem.current.isActiveAndEnabled) EventSystem.current.SetSelectedGameObject(currPanel.GetComponent<Panel>().GetFirstOption());
    }
    /// <summary>
    /// General methods. Might com in handy
    /// </summary>
    private void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        currPanel = panel;
    }
    private void ClosePanel(GameObject panel)
    {
        panel.GetComponent<Panel>().Close();
    }
    private void SetPreviousPanel(GameObject currentPanel)
    {
        prevPanel = currentPanel.GetComponent<Panel>().GetPrevious();
    }

    #endregion
    #region PauseMenu
    /// <summary>
    /// General methods for the PAUSE MENU
    /// </summary>
    public void OnOpenShopButton()
    {
        OpenPanel(shopPanel);
        SetPreviousPanel(shopPanel);
        ClosePanel(prevPanel);
        // Updates current points once upon opening the menu
        UpdateCurrentPoint();
    }
    public void OnQuitSessionButton()
    {
        // Resets time scale before going back to main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        // Destroy persistent objects
        Destroy(TransitionManager.Instance.gameObject);
        Destroy(PlayerTracker.instance.gameObject);
    }
    public void OnRetryButton()
    {
        // Resets time scale before going to the new scene
        Time.timeScale = 1f;
        TransitionManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }
    public void OnRebootButton()
    {
        // Resets time scale before going to the new scene
        Time.timeScale = 1f;
        // Resets the seed value to get a new level
        PlayerTracker.instance.seed = 0;
        TransitionManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }
    public void OnOpenPauseMenu()
    {
        // Updates the current point 
        aptScoreText.text = "Aptitude score: " + GameManager.instance.CurrentPoint();
    }
    /// <summary>
    /// SHOP MENU functions
    /// </summary>
    /// 
    
    public void OnGunDmgUpgrade()
    {
        if (PayForUpgrade(50)) fireControl.UpgradeGun(1.05f, 1f);
    }
    public void OnGunRateUpgrade()
    {
        if (PayForUpgrade(65)) fireControl.UpgradeGun(1f, 1.05f);
        
    }
    public void OnMissileRefill()
    {
        if (PayForUpgrade(70)) fireControl.ReSupplyMissile();
    }

    public void OnLiftUpgrade()
    {
        if (PayForUpgrade(40)) planeHandler.UpgradePerformance(1.05f, 1f);
        
    }
    public void OnDragUpgrade()
    {
        if (PayForUpgrade(70)) planeHandler.UpgradePerformance(1f, 0.95f);
        
    }
    public void OnThrustUpgrade()
    {
        if (PayForUpgrade(100)) planeHandler.UpgradeThrust(1.05f);
        
    }

    public void OnAuxItemRefill()
    {
        return;
    }
    public void OnHalfRepair()
    {
        if (PayForUpgrade(80)) selfTarget.Heal(50f);
        
    }
    public void OnFullRepair()
    {
        if (PayForUpgrade(100)) selfTarget.Heal(100f);
        
    }
    /// General methods for shop menu
    private bool PayForUpgrade(int cost)
    {
        var point = GameManager.instance.CurrentPoint();
        if (point >= cost)
        {
            GameManager.instance.SubtractPoint(cost);
            UpdateCurrentPoint();
            return true;
        }
        else return false;
    }
    private void UpdateCurrentPoint()
    {
        pointTxt.text = "Current point(s): " + GameManager.instance.CurrentPoint();
    }
    #endregion
}