using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Transform shootPoint;
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

    private Vector3 shootDirection;
   
    #region Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (canShoot)
        {
            StartCoroutine(FindTarget(targetingUpdateInterval));
            StartCoroutine(TryShoot(shootInterval));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the Targeting system before trying to shoot
        UpdateTargeting();
    }
    #endregion
    #region Procedures
    private void UpdateTargeting()
    {
        if (canShoot && _currentTarget != null)
        {
            if (Vector3.Distance(_currentTarget.transform.position, transform.position) > shootRange) _currentTarget = null;
            else
            {
                shootDirection = _currentTarget.transform.position - transform.position;
                shootPoint.rotation = Quaternion.LookRotation(shootDirection);
            }
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
    #endregion
    private enum WeaponType
    {
        Bullet,
        Missile,
        Rocket
    }
}
