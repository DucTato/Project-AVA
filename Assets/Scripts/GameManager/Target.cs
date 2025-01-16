using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    public EventHandler<HealthChangeEvent> onHealthChange;
    //public EventHandler<DeathEvent> onDeathEvent;
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
    public int rewardPoint;
    public bool IsDead { get; private set; }
    public Vector3 Position {get { return rb.position; } }
    public Vector3 Velocity { get { return rb.velocity; } set { rb.velocity = value; } }

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
            onHealthChange?.Invoke(this, new HealthChangeEvent(health, maxHealth));
        }
    }
    private Rigidbody rb;
    private List<Missile> incomingMissiles;
    #region CallBacks
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = maxHealth;
        incomingMissiles = new List<Missile>();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Generate random position for a maximum of 4 damage points
        for (int i = 0; i < damagePoints.Length; i++)
        {
            float randomX = Random.Range(GetComponentInChildren<MeshFilter>().mesh.bounds.min.x, GetComponentInChildren<MeshFilter>().mesh.bounds.max.x);
            float randomZ = Random.Range(GetComponentInChildren<MeshFilter>().mesh.bounds.min.z, GetComponentInChildren<MeshFilter>().mesh.bounds.max.z);
            //damagePoints[i] = new Vector3(randomX, GetComponentInChildren<MeshFilter>().mesh.bounds.max.y, randomZ);
            damagePoints[i] = new Vector3(randomX, 0f, randomZ);
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
        if (IsDead)
        {
            if (rb.velocity == Vector3.zero)
            {
                // Object is dead and is staying still on the ground
                // Disable script and physic updates
                rb.isKinematic = true;
            }
        }
    }
    #endregion
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
        //Broadcast a death event 
        //onDeathEvent?.Invoke(this, new DeathEvent(gameObject));
        if (GameManager.instance!= null)
        {
            GameManager.instance.DeathEventHandler(gameObject);
        }
        
        IsDead = true;
        GameObject smokeFX = Instantiate(smokeCloudPrefab, transform.position + damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
        GameObject fireFX = Instantiate(fireCloudPrefab, transform.position + damagePoints[Random.Range(0, damagePoints.Length)], transform.rotation, transform);
        smokeFX.GetComponent<ParticleSystem>().Play();
        fireFX.GetComponent<ParticleSystem>().Play();

        tag = "Dead";

        // Increasing drag to make game object fall faster
        rb.drag = -0.2f;
        rb.angularDrag = -0.2f;
        GetComponent<PlaneHandler>()?.ToggleDeadState();
        Utilities.DisableAllScripts(gameObject);
    }
    public void DealDamage(float dmg)
    {
        Health -= dmg;      
    }
    public void Heal(float ammount)
    {
        Health += ammount;
    }
    public void ApplyBurns()
    {
        
        GameObject smoke = Instantiate(smokeCloudPrefab, transform.position + damagePoints[Random.Range(0, damagePoints.Length)], Quaternion.identity, transform);
        GameObject fire = Instantiate(fireCloudPrefab, transform.position + damagePoints[Random.Range(0, damagePoints.Length)], Quaternion.identity, transform);
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
        fire.GetComponent<ParticleSystem>().Play();
    }
}
public class HealthChangeEvent : EventArgs
{
    private float health;
    private float maxHealth;
    public HealthChangeEvent(float health, float maxHealth)
    {
        this.health = health;
        this.maxHealth = maxHealth;
    }   
    public float Health
    {
        get { return health; }
        set { health = value; }
    }
    public float MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }
}
//public class DeathEvent : EventArgs
//{
//    private GameObject deadObject;
//    public DeathEvent(GameObject deadObject)
//    {
//        this.deadObject = deadObject;
//    }
//    public GameObject BroadcastedObject
//    {
//        get { return deadObject; }
//        set { deadObject = value; }
//    }
//}
