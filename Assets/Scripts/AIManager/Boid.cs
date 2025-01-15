using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public int SwarmIndex { get; set; }
    public float NoClumpingRadius { get; set; }
    public float LocalAreaRadius { get; set; }
    public float Speed { get; set; }
    public float SteeringSpeed { get; set; }
    public LayerMask castMask;
    #region Callbacks
    private void OnEnable()
    {
        Transform child = transform.GetChild(0);
        child.rotation = Random.rotation;
    }
    #endregion

    public void SimulateMovement(List<Boid> other, float time, Vector3 center)
    {
        //default vars
        var steering = Vector3.zero;

        var separationDirection = center;
        var separationCount = 0;
        var alignmentDirection = center;
        var alignmentCount = 0;
        var cohesionDirection = center;
        var cohesionCount = 0;

        var leaderBoid = other[0];
        var leaderAngle = 180f;

        foreach (Boid boid in other)
        {
            //skip self
            if (boid == this)
                continue;

            var distance = Vector3.Distance(boid.transform.position, this.transform.position);

            //identify local neighbour
            if (distance < NoClumpingRadius)
            {
                separationDirection += boid.transform.position - transform.position;
                separationCount++;
            }

            //identify local neighbour
            if (distance < LocalAreaRadius && boid.SwarmIndex == this.SwarmIndex)
            {
                alignmentDirection += boid.transform.forward;
                alignmentCount++;

                //cohesionDirection += boid.transform.position - transform.position;
                //cohesionCount++;

                //identify leader
                var angle = Vector3.Angle(boid.transform.position - transform.position, transform.forward);
                if (angle < leaderAngle && angle < 90f)
                {
                    leaderBoid = boid;
                    leaderAngle = angle;
                }
            }
        }

        if (separationCount > 0)
            separationDirection /= separationCount;

        //flip
        separationDirection = -separationDirection;

        if (alignmentCount > 0)
            alignmentDirection /= alignmentCount;

        //if (cohesionCount > 0)
        //    cohesionDirection /= cohesionCount;

        //get direction to center of mass
        cohesionDirection -= transform.position;

        //weighted rules
        steering += separationDirection.normalized;
        steering += alignmentDirection.normalized;
        steering += cohesionDirection.normalized;

        //local leader
        if (leaderBoid != null)
            steering += (leaderBoid.transform.position - transform.position).normalized;

        //obstacle avoidance

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, LocalAreaRadius, castMask.value))
            steering = ((hitInfo.point + hitInfo.normal) - transform.position).normalized;

        //apply steering
        if (steering != Vector3.zero)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(steering), SteeringSpeed * time);

        //move 
        transform.position += transform.TransformDirection(new Vector3(0, 0, Speed)) * time;
    }
    public void Launch(Target _target)
    {
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<VariableTrackingMissile>().enabled = true;
        GetComponent<VariableTrackingMissile>().Launch(gameObject, _target);
        GetComponent<Boid>().enabled = false;
    }
}
