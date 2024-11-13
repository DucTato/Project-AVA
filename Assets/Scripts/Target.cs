using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Target : MonoBehaviour
{
    [SerializeField]
    private float health, maxHealth;
    [SerializeField]
    private new string name;
    [SerializeField]
    private GameObject smokeCloudPrefab, fireCloudPrefab;
    [SerializeField]
    private Vector3[] damagePoints = new Vector3[4];
    const float sortInterval = 0.5f;
    private float sortTimer;
    private int objID;
    public bool IsDead { get; private set; }
    public Vector3 Position {get { return rb.position; } }
    public Vector3 Velocity {get { return rb.velocity; } }
    public string Name
    {
        get
        {
            return name;
        }
    }
    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    public float Health
    {
        get
        {
            return health;
        }
        private set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            
            if (health == 0 && maxHealth != 0 && !IsDead)
            {
                Die();
            }
        }
    }
    private Rigidbody rb;
    private List<Missile> incomingMissiles;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = maxHealth;
        incomingMissiles = new List<Missile>();
        objID = gameObject.GetInstanceID();
        // Generate random position for a maximum of 4 damage points
        for (int i = 0; i < damagePoints.Length; i++)
        {
            float randomX = Random.Range(GetComponent<BoxCollider>().bounds.min.x, GetComponent<BoxCollider>().bounds.max.x);
            float randomZ = Random.Range(GetComponent<BoxCollider>().bounds.min.z, GetComponent<BoxCollider>().bounds.max.z);
            damagePoints[i] = new Vector3(randomX, GetComponent<BoxCollider>().bounds.max.y, randomZ);
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        sortTimer = Mathf.Max(0, sortTimer - Time.fixedDeltaTime);

        if (sortTimer == 0)
        {
            SortIncomingMissiles();
            sortTimer = sortInterval;
        }
    }
    public Missile GetIncomingMissile()
    {
        if (incomingMissiles.Count > 0)
        {
            return incomingMissiles[0];
        }

        return null;
    }
    public void NotifyMissileLaunched(Missile missile, bool value)
    {
        if (value)
        {
            incomingMissiles.Add(missile);
            SortIncomingMissiles();
        }
        else
        {
            incomingMissiles.Remove(missile);
        }
    }
    private void SortIncomingMissiles()
    {
        Vector3 pos = Position;

        if (incomingMissiles.Count > 0)
        {
            incomingMissiles.Sort((Missile a, Missile b) => { 
                float distA = Vector3.Distance(a.rb.position, pos);
                float disB = Vector3.Distance(b.rb.position, pos);
                return distA.CompareTo(disB);
            });
        }
    }
    private void Die()
    {
        GetComponent<PlaneHandler>()?.ToggleDeadState();
        IsDead = true;
        GameObject smoke = Instantiate(smokeCloudPrefab, damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
        GameObject fire = Instantiate(fireCloudPrefab, damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
        smoke.GetComponent<ParticleSystem>().Play();
        fire.GetComponent<ParticleSystem>().Play();
        tag = "Dead";
        //Debug.Log(gameObject + " taken out");
        
    }
    public void DealDamage(float dmg)
    {
        Health -= dmg;
        if (objID == PlayerController.instance.PlayerID)
        {
            PlayerController.instance.hudController.DisplayHP();
        }
        // Damage FX
        if (dmg > 30f)
        {
            GameObject smoke = Instantiate(smokeCloudPrefab, damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
            GameObject fire = Instantiate(fireCloudPrefab, damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
            var smokePS = smoke.GetComponent<ParticleSystem>().main;
            smokePS.playOnAwake = false;
            smoke.GetComponent<ParticleSystem>().Stop();
            smokePS.duration = 5f;
            smokePS.loop = false;
            smokePS.stopAction = ParticleSystemStopAction.Destroy;
            smokePS.simulationSpace = ParticleSystemSimulationSpace.Local;
            smoke.GetComponent<ParticleSystem>().Play();

            var firePS = fire.GetComponent<ParticleSystem>().main;
            firePS.playOnAwake = false;
            fire.GetComponent<ParticleSystem>().Stop();
            firePS.duration = 5f;
            firePS.loop = false;
            firePS.simulationSpace = ParticleSystemSimulationSpace.Local;
            firePS.stopAction = ParticleSystemStopAction.Destroy;
            smoke.GetComponent<ParticleSystem>().Play();
        }
    }
}
