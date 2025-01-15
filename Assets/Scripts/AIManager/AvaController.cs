using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class AvaController : MonoBehaviour
{
    [SerializeField, Foldout("Pathfinding")]
    private GameObject navObject;
    [SerializeField, Foldout("Pathfinding")]
    private float patrolRadius;

    [SerializeField, Foldout("Targeting")]
    private Target _currentTarget;
    [SerializeField, Foldout("Targeting")]
    [Tooltip("Intervals between calls for finding a new target, measured in second(s)")]
    private float targetingInterval;
    [SerializeField, Foldout("Targeting")]
    [Tooltip("Distance to switch from gun to missile")]
    private float gunDistance;
    [SerializeField, Foldout("Targeting")]
    private float weaponSpread;
    [SerializeField, Foldout("Targeting")]
    private Transform[] shootPoints;
    
    [SerializeField, Foldout("State Machine")]
    private ActionSequence[] actionSequences;
    [SerializeField, Foldout("State Machine")]
    [Tooltip("Default intervals between opportunity rolls")]
    private float defaultInterval;
    private AvaAction[] _actions;
    private int currentSequence, currentAction;
    private bool isChasing;
    private NavMeshAgent navAgent;
    private Target selfTarget;
    private float lookSpeed, aggressiveness;
    private WeaponType _weaponType;
    private Vector3 moveDirection;
    #region CallBacks
    private void Awake()
    {
        isChasing = false;
        navAgent = navObject.GetComponent<NavMeshAgent>();
        currentSequence = 0;
        _actions = actionSequences[currentSequence].actionsOfThisSequence;
        selfTarget = GetComponent<Target>();
        
    }
    private void OnEnable()
    {
        // Update the progress bar to use as health bar

        ChangeAction();
    }
    private void OnDisable()
    {
        // Trigger win conditions
    }
    private void Update()
    {
        UpdateTargeting();
        UpdateActions();
    }
    #endregion
    #region Procedures
    private void UpdateTargeting()
    {
        if (_currentTarget != null)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_currentTarget.Position - transform.position), lookSpeed * Time.deltaTime);
            if (isChasing)
            {
                moveDirection = _currentTarget.Position - navObject.transform.position;
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
    
    private void UpdateActions()
    {
        if (_weaponType == WeaponType.Boid) return;
        if (_actions[currentAction].canShoot)
        {
            if (Vector3.Distance(_currentTarget.Position, transform.position) > gunDistance)
            {
                _weaponType = WeaponType.Laer;
            }
            else
            {
                _weaponType = WeaponType.Laser;
            }
        }

    }
    private void ChangeAction()
    {
        // Try stopping all existing coroutines
        StopAllCoroutines();

        currentAction = Random.Range(0, _actions.Length);
        Debug.Log("Changed Action! Index: " + currentAction);

        aggressiveness = _actions[currentAction]._aggressiveness;
        aggressiveness = Mathf.Clamp(aggressiveness, 1f, 9.9f);
        lookSpeed = _actions[currentAction].aimSpeed;
        lookSpeed = Mathf.Clamp(lookSpeed, 1f, 10f);

        if (_actions[currentAction].canShoot)
        {
            StartCoroutine(TryShoot((10f - aggressiveness)/ 10f * defaultInterval));        // Higher aggressiveness = more frequent checks
        }
        if (_actions[currentAction].canMove)
        {
            StartCoroutine(MoveOpportunity((10f - aggressiveness) / 10f * defaultInterval));
            navAgent.speed = _actions[currentAction].moveSpeed;
        }
        if (_actions[currentAction].canUseBoids)
        {
            StartCoroutine(TryBoids((10f - aggressiveness) / 10f * defaultInterval));
        }
        StartCoroutine(DoActionInDuration(_actions[currentAction].actionDuration));
    }
    /// <summary>
    /// Overload for ChangeAction() method
    /// </summary>
    /// <param name="index"></param>
    private void ChangeAction(int index)
    {
        // Try stopping all existing coroutines
        StopAllCoroutines();
        Debug.Log("Force-Changed Action!");
        currentAction = Random.Range(0, _actions.Length);

        aggressiveness = _actions[currentAction]._aggressiveness;
        aggressiveness = Mathf.Clamp(aggressiveness, 1f, 9.9f);
        lookSpeed = _actions[currentAction].aimSpeed;
        lookSpeed = Mathf.Clamp(lookSpeed, 1f, 10f);

        if (_actions[currentAction].canShoot)
        {
            StartCoroutine(TryShoot((10f - aggressiveness) / 10f * defaultInterval));
        }
        if (_actions[currentAction].canMove)
        {
            StartCoroutine(MoveOpportunity((10f - aggressiveness) / 10f * defaultInterval));
        }
        if (_actions[currentAction].canUseBoids)
        {
            StartCoroutine(TryBoids((10f - aggressiveness) / 10f * defaultInterval));
        }
        StartCoroutine(DoActionInDuration(_actions[currentAction].actionDuration));
    }
    private IEnumerator DoActionInDuration(float duration)
    {
        // Try update the target before doing actions
        var target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) _currentTarget = target.GetComponent<Target>();
        yield return new WaitForSeconds(duration);
       
        ChangeAction();
    }
    private IEnumerator TryShoot(float interval)
    {
        if (_currentTarget != null)
        {
            // Current target is not null, do the pew pew
            StartCoroutine(BurstFire(_actions[currentAction].shootVolley));
        }
        yield return new WaitForSeconds(interval);
        StartCoroutine(TryShoot(interval));
    }
    private IEnumerator MoveOpportunity(float interval)
    {
        // Each move opportunity has a 1/2 chance of either chasing the target or patrolling in an area
        if (Random.Range(0, 2) != 0 && _currentTarget != null)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
            var destination = Utilities.SpawnSphereOnEdgeRandomly3D(navObject, patrolRadius);
            destination.y = navObject.transform.position.y;
            moveDirection = destination;
        }
        navAgent.destination = moveDirection;
        yield return new WaitForSeconds(interval);
    }
    private IEnumerator TryBoids(float interval)
    {
        // Try to use Boids base on aggressiveness
        if (Random.Range(0, 10) < Mathf.RoundToInt(aggressiveness))
        {
            _weaponType = WeaponType.Boid;
            Debug.Log("Using boids");
            if (!_actions[currentAction].canShoot)
            {
                StopCoroutine(BurstFire(42));   // Try stopping the old coroutine before shooting again

                StartCoroutine(BurstFire(42));   // 42 is the answer :>
            }
        }
        yield return new WaitForSeconds(interval);
        StartCoroutine(TryBoids(interval));
    }
    private IEnumerator BurstFire(int BurstSize)
    {
        switch (_weaponType)
        {
            case WeaponType.Laser:
                // Laser will always shoot from the middle of the Core
                for (int i = 0; i < BurstSize; i++)
                {
                    var spread = Random.insideUnitCircle * weaponSpread;
                    var bulletGO = Instantiate(_actions[currentAction].bulletPrefab, transform.position, transform.rotation * Quaternion.Euler(spread.x, spread.y, 0f));
                    bulletGO.GetComponent<Bullet>().Fire(gameObject, 1f);
                    yield return new WaitForSeconds(60f / _actions[currentAction].fireRate);
                }
                break;
            case WeaponType.Laer:
                // Laer (guided laser) will always shoot from 3 points
                for (int i = 0; i < BurstSize; i++)
                {
                    for (int k = 0; k < shootPoints.Length; k++)
                    {
                        var spread = Random.insideUnitCircle * weaponSpread;
                        var missileGO = Instantiate(_actions[currentAction].missilePrefab, shootPoints[k].position, shootPoints[k].rotation * Quaternion.Euler(spread.x, spread.y, 0f));
                        missileGO.GetComponent<VariableTrackingMissile>().Launch(gameObject, _currentTarget);
                    }
                    yield return new WaitForSeconds(60f / _actions[currentAction].missileRate);
                }
                break;
            case WeaponType.Boid:
                // Boid is guided missile that can be shot down
                // Aggressiveness decides the number of loops that can be called
                for (int i = 0; i < Mathf.FloorToInt(aggressiveness); i++)
                {
                    GetComponent<BoidController>().LaunchBoids(_currentTarget);
                    yield return new WaitForSeconds(60f / 70f);
                }
                _weaponType = WeaponType.Laser; //Reset the weapon type after using boids
                break;
        }
    }
        #endregion
    [System.Serializable]
    private class AvaAction
    {
        [Header("Action")]
        public bool canMove, canShoot, canUseBoids;
        public float actionDuration, moveSpeed, aimSpeed, fireRate, missileRate;
        [Tooltip("High aggressiveness = Frequent opportunity rolls\nMin: 1.0, Max: 10.0")]
        public float _aggressiveness;
        public int shootVolley;
        public GameObject bulletPrefab, missilePrefab;
    }
    [System.Serializable]
    private class ActionSequence
    {
        [Header("Sequence")]
        public AvaAction[] actionsOfThisSequence;
        [Tooltip("Health value at which Ava changes sequence")]
        public float healthThreshold;
    }
    private enum WeaponType
    {
        Laser,
        Laer,
        Boid
    }
}
