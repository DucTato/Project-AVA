using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public Vector3 Position {get { return rb.position; } }
    public Vector3 Velocity {get { return rb.velocity; } }
    private Rigidbody rb;
    private List<Missile> incomingMissiles;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
