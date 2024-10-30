using System.Collections;
using System.Collections.Generic;
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
    float throttleInput;
    Vector3 controlInput;

    Vector3 lastVelocity;
    PhysicMaterial landingGearDefaultMaterial;

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
    }

    // Update is called once per frame
    void Update()
    {
        
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
            }
        }
        // missile lock either rotates towards the target or towards the neutral position
        missileLockDirection = Vector3.RotateTowards(missileLockDirection, targetDir, Mathf.Deg2Rad * lockSpeed * dt, 0);
        MissileLocked = target != null && MissileTracking && Vector3.Angle(missileLockDirection, targetDir) < lockSpeed * dt;

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
        var hardpoint = hardpoints[index];
        var missileGO = Instantiate(missilePrefab, hardpoint.position, hardpoint.rotation);
    }
}
