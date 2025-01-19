using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FCS : MonoBehaviour
{
    public EventHandler<EventArgs> AmmoUpdate;
    [Header("Fire Control System")]
    [SerializeField]
    private bool consumeAmmo;
    [SerializeField]
    private int cannonVolley, storedMissiles;
    [SerializeField]
    List<Transform> hardpoints;
    [SerializeField]
    float missileReloadTime, fcsUpdateInterval;
    [SerializeField]
    float missileDebounceTime;
    [SerializeField]
    GameObject missilePrefab;
    [SerializeField]
    private Target currentTarget;

    public float lockRange;
    [SerializeField]
    float lockSpeed;
    [SerializeField]
    float lockAngle;
    [SerializeField]
    [Tooltip("Firing rate in Rounds Per Minute")]
    float cannonFireRate;
    [SerializeField]
    float cannonDebounceTime;
    [SerializeField]
    float cannonSpread;
    [SerializeField]
    Transform cannonSpawnPoint;
    [SerializeField]
    GameObject bulletPrefab;
    [SerializeField]
    private ParticleSystem muzzleFX;
    private int tgtIndex;

    public TgtBehaviourType targetingBehaviour;

    private Rigidbody rb;
    
    private int missileIndex, currentMissiles, emptyIndex;
    private List<float> missileReloadTimers;
    private HashSet<GameObject> targetsList;
    private float missileDebounceTimer, fcsUpdateIntervalTimer;
    private Vector3 missileLockDirection;

    private string currentTag;
    private bool cannonFiring;
    private float cannonDebounceTimer;
    private float cannonFiringTimer, cannonDmgMult, cannonRateMult;
    //public Target currTarget { get {return currentTarget; } private set { currentTarget = value; Debug.Log(currentTarget); } }
    public int CurrentCannonAmmo
    {
        get
        {
            return cannonVolley;
        }
        private set
        {
            cannonVolley = value;
            AmmoUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
    public int CurrentMissileAmmo
    {
        get
        {
            return currentMissiles;
        }
        set
        {
            currentMissiles = value;
            AmmoUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
    public int CurrentMissileStorage
    {
        get
        {
            return storedMissiles;
        }
        set
        {
            storedMissiles = value;
            storedMissiles = Mathf.Clamp(storedMissiles, 0, 14);
            AmmoUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
    public Target currTarget 
    {
        get
        {
            return currentTarget;
        }
        private set
        {
            currentTarget = value;
            if (PlayerController.instance.CheckIsPlayer(gameObject)) PlayerController.instance.hudController.SetCurrentTargetInfo(currentTarget);
        }
    }
    public bool MissileLock { get; private set; }
    public bool MissileTracking { get; private set; }
    public bool MissileLocked { get; private set; }
    public Vector3 MissileLockDirection
    {
        get
        {
            return rb.rotation * missileLockDirection;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        currentMissiles = hardpoints.Count;
        cannonDmgMult = 1f;
        cannonRateMult = 1f;
        switch (targetingBehaviour)
        {
            case TgtBehaviourType.Player_Allies:
                currentTag = "Enemy";
                break;
            case TgtBehaviourType.Enemies:
                currentTag = "Player";
                //Debug.Log(currTarget);
                break;
            default:
                currentTag = "Enemy";
                break;
        }
        rb = GetComponent<Rigidbody>();
        targetsList = new HashSet<GameObject>();
        missileReloadTimers = new List<float>(hardpoints.Count);
        foreach (var hp in hardpoints)
        {
            missileReloadTimers.Add(0f);
        }
        missileLockDirection = Vector3.forward;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        UpdateWeapons(Time.fixedDeltaTime);        
    }
    #region FCS functions
    private void UpdateMissleLock(float dt)
    {
        // Default neutral position is forward
        Vector3 targetDir = Vector3.forward;
        MissileTracking = false;
        if (currTarget != null && !currTarget.IsDead)
        {
            var error = currTarget.Position - rb.position;
            var errorDir = Quaternion.Inverse(rb.rotation) * error.normalized;  //Transform into local space
            if (error.magnitude <= lockRange && Vector3.Angle(Vector3.forward, errorDir) <= lockAngle)
            {
                MissileTracking = true;
                targetDir = errorDir;
                //Debug.Log("Target Tracking!" + error.magnitude);
            }
        }
        // missile lock either rotates towards the target or towards the neutral position
        missileLockDirection = Vector3.RotateTowards(missileLockDirection, targetDir, Mathf.Deg2Rad * lockSpeed * dt, 0);
        MissileLocked = currTarget != null && MissileTracking && Vector3.Angle(missileLockDirection, targetDir) < lockSpeed * dt;
        //if (MissileLocked) Debug.Log("Target Locked");
    }
    private void UpdateCannon(float dt)
    { 
        if (cannonFiring && cannonFiringTimer == 0)
        {
            if (consumeAmmo && CurrentCannonAmmo <= 0) return;
            if (consumeAmmo)
            {
                CurrentCannonAmmo--;
            }
            cannonFiringTimer = 60f / cannonFireRate;
            cannonDebounceTimer = cannonDebounceTime;
            var spread = Random.insideUnitCircle * cannonSpread;
            var bulletGO = Instantiate(bulletPrefab, cannonSpawnPoint.position, cannonSpawnPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0));
            bulletGO.GetComponent<Bullet>().Fire(gameObject, cannonDmgMult);
            muzzleFX.Play();
        }
        if (!cannonFiring)
        {
            if (cannonDebounceTimer > 0)
            cannonDebounceTimer = Mathf.Max(0, cannonDebounceTimer - dt);
            else
            {
                // Refill the cannon volley after a cannonDebounceTime
                CurrentCannonAmmo = 150;
            }
        }
    }
    private void FireMissile(int index)
    {
        if (consumeAmmo)
        {
            if (CurrentMissileAmmo <= 0) return;
            var missileGO = Instantiate(missilePrefab, hardpoints[index].position, hardpoints[index].rotation);
            if (missileGO.GetComponent<Missile>() != null)
                missileGO.GetComponent<Missile>().Launch(gameObject, MissileLocked ? currTarget : null);
            else
                missileGO.GetComponent<CinemaWinder>().Launch(gameObject, MissileLocked ? currTarget : null);
            CurrentMissileAmmo--;
        }
        else
        {
            var missileGO = Instantiate(missilePrefab, hardpoints[index].position, hardpoints[index].rotation);
            missileGO.GetComponent<Missile>().Launch(gameObject, MissileLocked ? currTarget : null);
        }
        
    }
    private void UpdateWeaponCooldown(float dt)
    {
        missileDebounceTimer = Mathf.Max(0, missileDebounceTimer - dt);
        
        cannonFiringTimer = Mathf.Max(0, cannonFiringTimer - dt);

        for (int i = 0; i < missileReloadTimers.Count; i++)
        {
            missileReloadTimers[i] = Mathf.Max(0, missileReloadTimers[i] - dt);
            if (missileReloadTimers[i] == 0)
            {
                //Show missiles under wings
                if (i == emptyIndex) Reload();
            }
        }
    }
    private void Reload()
    {
        if (hardpoints.Count < CurrentMissileStorage + CurrentCannonAmmo)
        {
            CurrentMissileStorage -= hardpoints.Count - CurrentMissileAmmo;
            CurrentMissileAmmo = hardpoints.Count;
        }
        else
        {
            CurrentMissileAmmo += CurrentMissileStorage;
            CurrentMissileStorage = 0;
        }
    }
    private void UpdateFCS(float dt, string tag)
    {
        fcsUpdateIntervalTimer = Mathf.Max(0, fcsUpdateIntervalTimer - dt);
        if (fcsUpdateIntervalTimer == 0) 
        {
            fcsUpdateIntervalTimer = fcsUpdateInterval;
            targetsList.Clear();
            targetsList = GameObject.FindGameObjectsWithTag(tag).ToHashSet();
        }
        if (currTarget == null || currTarget.IsDead) currTarget = FindClosestTarget();
    }
    private void UpdateWeapons(float dt)
    {
        UpdateWeaponCooldown(dt);
        UpdateMissleLock(dt);
        UpdateCannon(dt);
        UpdateFCS(dt, currentTag);
    }
    private Target FindClosestTarget()
    {
        //GameObject[] GO2s = GameObject.FindGameObjectsWithTag("Enemy mBullet");
        Target closest = null;
        float distance = lockRange + 1000f;
        Vector3 position = transform.position;
        foreach (GameObject t in targetsList)
        {
            if (t == null)
            {
                continue;
            }

            Vector3 diff = t.transform.position - position;
            float curDistance = diff.magnitude;
            if (curDistance < distance)
            {
                closest = t.GetComponent<Target>();
                distance = curDistance;
            }
        }
        return closest;
    }
    #endregion
    #region Procedures
    public void TryFireMissile()
    {
        // try all available missile
        for (int i = 0; i < hardpoints.Count; i++)
        {
            var index = (missileIndex + i) % hardpoints.Count;
            if (missileDebounceTimer == 0 && missileReloadTimers[index] == 0)
            {
                FireMissile(index);
                missileIndex = (index + 1) % hardpoints.Count;
                missileReloadTimers[index] = missileReloadTime;
                missileDebounceTimer = missileDebounceTime;
                emptyIndex = index;
                // update animation
                // TODO
                break;
            }
        }
    }
    public void FireCannon(bool input)
    {
        cannonFiring = input;
    }
    
    public void CycleTarget()
    {
        if (targetsList.Count == 0)
        {
            Debug.Log("Target List is empty");
            return; 
        }
        List<GameObject> targets = targetsList.ToList();
        if (tgtIndex >= targets.Count) tgtIndex = 0;
        currTarget = targets[tgtIndex++].GetComponent<Target>();
    }
    public void UpgradeGun(float dmgMult, float rateMult)
    {
        cannonDmgMult *= dmgMult;
        cannonRateMult *= rateMult;
        // Clamps values for both of these rates. Damage maxes out at x2; Fire rate maxes out at x5
        cannonDmgMult = Mathf.Clamp(cannonDmgMult, 1f, 2f);
        cannonRateMult = Mathf.Clamp(cannonRateMult, 1f, 5f);
        cannonFireRate *= cannonRateMult;
        cannonSpread *= cannonRateMult; // Faster RoF = more Spread
    }
    public void ReSupplyMissile()
    {
        CurrentMissileStorage += 2;
    }
    #endregion
    public enum TgtBehaviourType
    {
        Player,
        Player_Allies,
        Enemies
    }
}
//public class AmmoUpdateEvent : EventArgs
//{
//    private int cannonAmmo, missileAmmo, storedMissiles;
//    public AmmoUpdateEvent(int cannonAmmo, int missileAmmo, int storedMissiles)
//    {
//        this.cannonAmmo = cannonAmmo;
//        this.missileAmmo = missileAmmo;
//        this.storedMissiles = storedMissiles;
//    }
//    public int GetCannonAmmo()
//    {
//        return cannonAmmo;
//    }
//    public int GetMissileAmmo()
//    {
//        return missileAmmo;
//    }
//    public int GetStoredMissiles()
//    {
//        return storedMissiles;
//    }
//}


