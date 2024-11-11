using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField]
    private float health, maxHealth;
    [SerializeField]
    private new string name;
    const float sortInterval = 0.5f;
    private float sortTimer;
    public bool IsDead { get; private set; }
    public Vector3 Position {get { return rb.position; } }
    public Vector3 Velocity {get { return rb.velocity; } }
    public bool Dead { get; private set; }
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

            // Damage FX
            //if (health <= MaxHealth * .5f && health > 0)
            //{
            //    damageEffect.SetActive(true);
            //}
            //else
            //{
            //    damageEffect.SetActive(false);
            //}

            if (health == 0 && maxHealth != 0 && !Dead)
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
        Dead = true;
        Destroy(gameObject);
    }
    public void DealDamage(float dmg)
    {
        Health -= dmg;
    }
}
