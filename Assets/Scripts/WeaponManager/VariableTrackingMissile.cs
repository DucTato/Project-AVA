using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableTrackingMissile : Missile
{
    [SerializeField]
    private AnimationCurve trackingOverLifetime;
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
        // set speed to the direction of travel
        rb.velocity = rb.rotation * new Vector3(0, 0, speed);
        CheckCollision();
        TrackTarget(Time.fixedDeltaTime);

    }
    //public void Launch(GameObject owner, Target target)
    //{
    //    this.owner = owner;
    //    this.target = target;
    //    rb = GetComponent<Rigidbody>();
    //    lastPosition = rb.position;
    //    timer = lifeTime;
    //    // Notify the target
    //    if (target != null)
    //    {
    //        target.NotifyMissileLaunched(this, true);
    //    }
    //}
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
        float maxTurnRate = (trackingOverLifetime.Evaluate(timer/lifeTime)* turningGForce * 9.81f) / speed;    // radian / s
        var dir = Vector3.RotateTowards(currentDir, targetDir, maxTurnRate * dt, 0);
        rb.rotation = Quaternion.LookRotation(dir);
    }
    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        timer = lifeTime;
        rb.isKinematic = true;
        if (renderer != null) renderer.enabled = false;

        // Explosion FX
        //Debug.Log("BOOM!");
        explosionFX.SetActive(true);
        var hits = Physics.OverlapSphere(rb.position, damageRadius, collisionMask.value);
        foreach (var hit in hits)
        {
            Target other = hit.gameObject.GetComponent<Target>();
            if (other != null && other.gameObject != owner)
            {
                // Deal damage
                other.DealDamage(damage);
                other.ApplyBurns();
            }
        }
        // Stop notifying the HUD of this missile
        if (target != null) target.NotifyMissileLaunched(this, false);
    }
    private void CheckCollision()
    {
        // using raycast to check for collisions
        var currentPosition = rb.position;
        var error = currentPosition - lastPosition;
        // ray is pointed in the direction that the missile is flying
        var ray = new Ray(lastPosition, error.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, error.magnitude, collisionMask.value))
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
