using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField]
    private float lifeTime;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float trackingAngle;
    [SerializeField]
    private float damage;
    [SerializeField]
    private float damageRadius;
    [SerializeField]
    float turningGForce;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private new MeshRenderer renderer;
    public Target target;
    private Rigidbody rb;

    private GameObject owner;
    private bool exploded;
    private Vector3 lastPosition;
    private float timer;


    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        timer = Mathf.Max(0, timer - Time.fixedDeltaTime);
        // the missile will explode at the end of its life time
        // the timer is recycled for explosion FX
        if (timer == 0)
        {
            if (exploded) Destroy(gameObject);
            else Explode();
        }
        if (exploded) return;
        CheckCollision();
        TrackTarget(Time.fixedDeltaTime);
        // set speed to the direction of travel
        //rb.velocity = rb.rotation * new Vector3(0, 0, speed);
        rb.velocity = rb.rotation * new Vector3(0, 0, speed);
        
        
    }
    public void Launch(GameObject owner, Target target)
    {
        this.owner = owner;
        this.target = target;
        rb = GetComponent<Rigidbody>();
        lastPosition = rb.position;
        timer = lifeTime;
        // Notify the target
        //if (target != null)
        //{
        //    target.
        //}
    }
    private void TrackTarget(float dt)
    {
        if (target == null) return;

        var targetPosition = Utilities.FirstOrderIntercept(rb.position, Vector3.zero, speed, target.Position, target.Velocity);
        var error = targetPosition - rb.position;
        var targetDir = error.normalized;
        var currentDir = rb.rotation * Vector3.forward;
        //Debug.Log("Target Pos: " +targetPosition);
        // if angle to target is too large, explode
        if (Vector3.Angle(currentDir, targetDir) > trackingAngle)
        {
            Explode();
            return;
        }
        // calculate the turning rate from G Force and speed
        float maxTurnRate = (turningGForce * 9.81f) / speed;    // radian / s
        var dir = Vector3.RotateTowards(currentDir, targetDir, maxTurnRate * dt, 0);
        rb.rotation = Quaternion.LookRotation(dir);
    }
    private void Explode()
    {
        if (exploded) return;

        timer = lifeTime;
        rb.isKinematic = true;
        renderer.enabled = false;
        exploded = true;
        // Explosion FX
        Debug.Log("BOOM!");
        var hits = Physics.OverlapSphere(rb.position, damageRadius, collisionMask.value);
        foreach (var hit in hits)
        {
            Target other = hit.gameObject.GetComponent<Target>();
            if (other != null && other.gameObject != owner)
            {
                // Deal damage
            }
        }
        // Stop notifying the HUD of this missile
        //if (target!= null) target.
    }
    private void CheckCollision()
    {
        // using raycast to check for collisions
        var currentPosition = rb.position;
        var error = currentPosition - lastPosition;
        // ray is pointed in the direction that the missile is flying
        var ray = new Ray(lastPosition, error.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, error.magnitude, collisionMask.value))
        {
            Target other = hit.collider.gameObject.GetComponent<Target>();
            if (other == null || other.gameObject != owner)
            {
                rb.position = hit.point;
                Explode();
            }
        }
        lastPosition = currentPosition;
    }
}
