using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FCS : MonoBehaviour
{
    [Header("Fire Control System")]
    [SerializeField]
    List<Transform> hardpoints;
    [SerializeField]
    float missileReloadTime;
    [SerializeField]
    float missileDebounceTime;
    [SerializeField]
    GameObject missilePrefab;
    //[SerializeField]
    //Target currTarget;
    [SerializeField]
    float lockRange;
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
    private TgtBehaviourType targetingBehaviour;

    private Rigidbody rb;
    
    private int missileIndex;
    private List<float> missileReloadTimers;
    private HashSet<GameObject> targetsList;
    private float missileDebounceTimer;
    private Vector3 missileLockDirection;
    

    private bool cannonFiring;
    private float cannonDebounceTimer;
    private float cannonFiringTimer;
    public Target currTarget { get; private set; }
    public bool MissileLock { get; private set; }
    public bool MissileTracking { get; private set; }
    public bool MissileLocked { get; private set; } 
    // Start is called before the first frame update
    void Start()
    {
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
        switch (targetingBehaviour)
        {
            case TgtBehaviourType.Player_Allies:
                currTarget = FindClosestTarget("Enemy");
                break;
            case TgtBehaviourType.Enemies:
                currTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<Target>();
                //Debug.Log(currTarget);
                break;
        }
        
    }
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
                // update animation
                break;
            }
        }
    }
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
        if (MissileLocked) Debug.Log("Target Locked");
    }
    private void UpdateCannon(float dt)
    {
        if (cannonFiring && cannonFiringTimer == 0)
        {
            cannonFiringTimer = 60f / cannonFireRate;
            var spread = Random.insideUnitCircle * cannonSpread;
            var bulletGO = Instantiate(bulletPrefab, cannonSpawnPoint.position, cannonSpawnPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0));
            bulletGO.GetComponent<Bullet>().Fire(transform.gameObject);
        }
    }
    private void FireMissile(int index)
    {
        var missileGO = Instantiate(missilePrefab, hardpoints[index].position, hardpoints[index].rotation);
        missileGO.GetComponent<Missile>().Launch(transform.gameObject, MissileLocked ? currTarget : null);
    }
    private void UpdateWeaponCooldown(float dt)
    {
        missileDebounceTimer = Mathf.Max(0, missileDebounceTimer - dt);
        cannonDebounceTimer = Mathf.Max(0, cannonDebounceTimer - dt);
        cannonFiringTimer = Mathf.Max(0, cannonFiringTimer - dt);

        for (int i = 0; i < missileReloadTimers.Count; i++)
        {
            missileReloadTimers[i] = Mathf.Max(0, missileReloadTimers[i] - dt);
            if (missileReloadTimers[i] == 0)
            {
                //Show missiles under wings
            }
        }
    }
    public void FireCannon(bool input)
    {
        cannonFiring = input;
    }
    private void UpdateWeapons(float dt)
    {
        UpdateWeaponCooldown(dt);
        UpdateMissleLock(dt);
        UpdateCannon(dt);
    }
    private Target FindClosestTarget(string tag)
    {

        targetsList = GameObject.FindGameObjectsWithTag(tag).ToHashSet();
        //GameObject[] GO2s = GameObject.FindGameObjectsWithTag("Enemy mBullet");
        
        Target closest = null;
        float distance = lockRange + 1000f;
        Vector3 position = transform.position;
        foreach (GameObject t in targetsList)
        {
            if (t == null) targetsList.Remove(t);
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
    private enum TgtBehaviourType
    {
        Player_Allies,
        Enemies
    }
}


