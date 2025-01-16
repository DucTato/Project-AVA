using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GroundUnitController : MonoBehaviour
{
    [SerializeField, Foldout("Weapons")]
    [Tooltip("Whether or not this unit can shoot")]
    private bool canShoot;
    [SerializeField, Foldout("Weapons")]
    private WeaponType weaponType;
    [SerializeField, Foldout("Weapons")]
    private GameObject bulletPrefab, missilePrefab;
    [SerializeField, Foldout("Weapons")]
    private Transform shootPoint, turret;
    [SerializeField, Foldout("Weapons")]
    private float weaponSpread;
    [SerializeField, Foldout("Weapons")]
    [Tooltip("Intervals between each fire volley, measured in second(s)")]
    private float shootInterval;
    [SerializeField, Foldout("Weapons")]
    [Tooltip("Fire rate, measured in RPM")]
    private float fireRate;
    [SerializeField, Foldout("Weapons")]
    [Tooltip("How many rounds to fire each volley")]
    private int shootVolley;
    [SerializeField, Foldout("Targeting")]
    private Target _currentTarget;
    [SerializeField, Foldout("Targeting")]
    private float shootRange, targetingUpdateInterval;
    [SerializeField, Foldout("Pathfinding")]
    private bool isChasing;
    [SerializeField, Foldout("Pathfinding")]
    [Tooltip("The radius which this unit can patrol around itself")]
    private float patrolRadius;
    [SerializeField, Foldout("Pathfinding")]
    [Tooltip("Intervals between each move opportunity, measured in second(s)")]
    private float moveInterval;

    private NavMeshAgent navAgent;
    private Vector3 shootDirection, moveDirection;
    private Rigidbody rb;
   
    #region Callbacks
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        { 
            // Only check the movement opportunity if this unit can move
            StartCoroutine(MoveOpportunity(moveInterval));
        }
        if (canShoot)
        {
            StartCoroutine(FindTarget(targetingUpdateInterval));
            StartCoroutine(TryShoot(shootInterval));
        }
        isChasing = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the Targeting system before trying to shoot
        UpdateTargeting();
        UpdatePathfinding();
        rb.velocity = navAgent.velocity;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        navAgent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }
    #endregion
    #region Procedures
    private void UpdateTargeting()
    {
        if (canShoot && _currentTarget != null)
        {
            if (Vector3.Distance(_currentTarget.Position, transform.position) < shootRange)
            {
                // Will shoot if within range
                shootDirection = _currentTarget.Position - transform.position;
                turret.rotation = Quaternion.LookRotation(shootDirection.normalized);
            }
        }
    }
    private void UpdatePathfinding()
    {
        if (isChasing)
        {
            moveDirection = _currentTarget.Position;
            moveDirection.y = transform.position.y;
        }
    }
    private IEnumerator BurstFire(int BurstSize)
    {
        switch (weaponType)
        {
            case WeaponType.Bullet:
                for (int i = 0; i < BurstSize; i++)
                {
                    var spread = Random.insideUnitCircle * weaponSpread;
                    var bulletGO = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0f));
                    bulletGO.GetComponent<Bullet>().Fire(gameObject, 1f);
                    yield return new WaitForSeconds(60f / fireRate);
                }
                break;
            case WeaponType.Missile:
                for (int i = 0; i < BurstSize; i++)
                {
                    var spread = Random.insideUnitCircle * weaponSpread;
                    var missileGO = Instantiate(missilePrefab, shootPoint.position, shootPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0f));
                    missileGO.GetComponent<Missile>().Launch(gameObject, _currentTarget);
                    yield return new WaitForSeconds(60f / fireRate);
                }
                break;
            case WeaponType.Rocket:
                // Rocket is non-guided missile
                for (int i = 0; i < BurstSize; i++)
                {
                    var spread = Random.insideUnitCircle * weaponSpread;
                    var missileGO = Instantiate(missilePrefab, shootPoint.position, shootPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0f));
                    missileGO.GetComponent<Missile>().Launch(gameObject, null);
                    yield return new WaitForSeconds(60f / fireRate);
                }
                break;
        }
    }
    private IEnumerator FindTarget(float interval)
    {
        var target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) _currentTarget = target.GetComponent<Target>();
        yield return new WaitForSeconds(interval);
        StartCoroutine(FindTarget(interval));
    }
    private IEnumerator TryShoot(float interval)
    {
        if (_currentTarget != null && canShoot) StartCoroutine(BurstFire(shootVolley));
        yield return new WaitForSeconds(interval);
        StartCoroutine(TryShoot(interval));
    }
    private IEnumerator MoveOpportunity(float interval)
    {
        // Each move opportunity has a 1/5 chance to fail (stand still)
        if (Random.Range(0,5) == 0)
        {
            isChasing = false;
            yield return new WaitForSeconds(interval);
            StartCoroutine(MoveOpportunity(interval));
        }
        else
        {
            // Each move opportunity has a 1/2 chance of either chasing the target or patrolling in an area
            if (Random.Range(0, 2) != 0 && _currentTarget != null)
            {
                isChasing = true;
            }
            else
            {
                isChasing = false;
                var destination = Utilities.SpawnSphereOnEdgeRandomly3D(gameObject, patrolRadius);
                destination.y = transform.position.y;
                moveDirection = destination;
            }
            navAgent.destination = moveDirection;
            yield return new WaitForSeconds(interval);
            StartCoroutine(MoveOpportunity(interval));
        }
    }
    #endregion
    private enum WeaponType
    {
        Bullet,
        Missile,
        Rocket
    }
    private struct MoveInput
    {
        public Vector3 destinationPosition;
        public float time;
    }
}
