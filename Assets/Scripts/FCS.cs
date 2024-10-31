using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField]
    Target target;
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


    private Rigidbody rb;
    
    int missileIndex;
    List<float> missileReloadTimers;
    float missileDebounceTimer;
    Vector3 missileLockDirection;

    bool cannonFiring;
    float cannonDebounceTimer;
    float cannonFiringTimer;
    public bool MissileLock { get; private set; }
    public bool MissileTracking { get; private set; }
    public bool MissileLocked { get; private set; } 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        target = FindClosestTarget();
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
        if (target != null && !target.IsDead)
        {
            var error = target.Position - rb.position;
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
        MissileLocked = target != null && MissileTracking && Vector3.Angle(missileLockDirection, targetDir) < lockSpeed * dt;
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
        missileGO.GetComponent<Missile>().Launch(transform.gameObject, MissileLocked ? target : null);
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
    private Target FindClosestTarget()
    {
        GameObject[] GOs = GameObject.FindGameObjectsWithTag("Enemy");
        //GameObject[] GO2s = GameObject.FindGameObjectsWithTag("Enemy mBullet");
        //GameObject[] GOs = GO1s.Concat(GO2s).ToArray();
        Target closest = null;
        float distance = lockRange + 1000f;
        Vector3 position = transform.position;
        foreach (GameObject go in GOs)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.magnitude;
            if (curDistance < distance)
            {
                closest = go.GetComponent<Target>();
                distance = curDistance;
            }
        }
        return closest;
    }
}
