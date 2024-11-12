using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneHUD : MonoBehaviour
{
    [SerializeField]
    float updateRate;
    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color lockColor;
    [SerializeField]
    List<GameObject> helpDialogs;
    [SerializeField]
    Compass compass;
    [SerializeField]
    PitchLadder pitchLadder;
    [SerializeField]
    Bar throttleBar;
    [SerializeField]
    private Transform hudCenter;
    [SerializeField]
    private Transform stillComponents;
    [SerializeField]
    Transform velocityMarker;
    [SerializeField]
    Text airspeed;
    [SerializeField]
    Text aoaIndicator;
    [SerializeField]
    Text gforceIndicator;
    [SerializeField]
    Text altitude;
    [SerializeField]
    Bar healthBar;
    [SerializeField]
    Text healthText;
    [SerializeField]
    Transform targetBox;
    [SerializeField]
    Text targetName;
    [SerializeField]
    Text targetRange;
    [SerializeField]
    Transform missileLock;
    [SerializeField]
    Transform reticle;
    [SerializeField]
    RectTransform reticleLine;
    [SerializeField]
    RectTransform targetArrow;
    [SerializeField]
    RectTransform missileArrow;
    [SerializeField]
    float targetArrowThreshold;
    [SerializeField]
    float missileArrowThreshold;
    [SerializeField]
    float cannonRange;
    [SerializeField]
    float bulletSpeed;
    [SerializeField]
    private float hpUpTime;
    [SerializeField]
    GameObject aiMessage;
    [SerializeField]
    private GameObject mslWarning;

    [SerializeField]
    List<Graphic> missileWarningGraphics;
    private PlaneHandler plane;
    private FCS fireControl;
    //Target target;
    AIController aiController;
    Target selfTarget;
    Transform planeTransform;
    new Camera camera;
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
    private bool hideHP;

    const float metersToKnots = 1.94384f;
    const float metersToFeet = 3.28084f;

    void Start()
    {
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
    }

    public void SetPlane(PlaneHandler plane)
    {
        this.plane = plane;

        if (plane == null)
        {
            planeTransform = null;
            selfTarget = null;
        }
        else
        {
            aiController = plane.GetComponent<AIController>();
            planeTransform = plane.GetComponent<Transform>();
            selfTarget = plane.GetComponent<Target>();
            fireControl = plane.GetComponent<FCS>();
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
        this.camera = camera;

        if (camera == null)
        {
            cameraTransform = null;
        }
        else
        {
            cameraTransform = camera.GetComponent<Transform>();
        }

        if (compass != null)
        {
            compass.SetCamera(camera);
        }

        if (pitchLadder != null)
        {
            pitchLadder.SetCamera(camera);
        }
    }

    public void ToggleHelpDialogs()
    {
        foreach (var dialog in helpDialogs)
        {
            dialog.SetActive(!dialog.activeSelf);
        }
    }
    public void DisplayHP()
    {
        HPupTimer = hpUpTime;
    }
    void UpdateVelocityMarker()
    {
        var velocity = planeTransform.forward;

        if (plane.LocalVelocity.sqrMagnitude > 1)
        {
            velocity = plane.rb.velocity;
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

    void UpdateAirspeed()
    {
        var speed = plane.LocalVelocity.z * metersToKnots;
        airspeed.text = string.Format("{0:0}", speed);
    }

    void UpdateAOA()
    {
        aoaIndicator.text = string.Format("{0:0.0} AOA", plane.AngleOfAttack * Mathf.Rad2Deg);
    }

    void UpdateGForce()
    {
        var gforce = plane.LocalGForce.y / 9.81f;
        gforceIndicator.text = string.Format("{0:0.0} G", gforce);
    }

    void UpdateAltitude()
    {
        var altitude = plane.rb.position.y * metersToFeet;
        this.altitude.text = string.Format("{0:0}", altitude);
    }

    Vector3 TransformToHUDSpace(Vector3 worldSpace)
    {
        // Not accounting for dynamic resolution scaling
        var screenSpace = camera.WorldToScreenPoint(worldSpace);
        return screenSpace - new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
        //var screenSpace = camera.WorldToScreenPoint(worldSpace);
        //var finalSP = new Vector3(screenSpace.x / GetComponent<Canvas>().scaleFactor, screenSpace.y / GetComponent<Canvas>().scaleFactor);
        //return finalSP;
    }

    void UpdateHUDCenter()
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

    void UpdateWeapons()
    {
        if (fireControl.currTarget == null)
        {
            targetBoxGO.SetActive(false);
            missileLockGO.SetActive(false);
            return;
        }

        //update target box, missile lock
        var targetDistance = Vector3.Distance(plane.rb.position, fireControl.currTarget.Position);
        var targetPos = TransformToHUDSpace(fireControl.currTarget.Position);
        var missileLockPos = fireControl.MissileLocked ? targetPos : TransformToHUDSpace(plane.rb.position + fireControl.MissileLockDirection * targetDistance);

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
        var targetDir = (fireControl.currTarget.Position - plane.rb.position).normalized;
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
        var leadPos = Utilities.FirstOrderIntercept(plane.rb.position, plane.rb.velocity, bulletSpeed, fireControl.currTarget.Position, fireControl.currTarget.Velocity);
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

    void UpdateWarnings()
    {
        var incomingMissile = selfTarget.GetIncomingMissile();

        if (incomingMissile != null)
        {
            var missilePos = TransformToHUDSpace(incomingMissile.rb.position);
            //
            var missileDir = (incomingMissile.rb.position - plane.rb.position).normalized;
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

    void LateUpdate()
    {
        if (plane == null) return;
        if (camera == null) return;

        float degreesToPixels = camera.pixelHeight / camera.fieldOfView;

        throttleBar.SetValue(plane.Throttle);

        if (!plane.Dead)
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
}