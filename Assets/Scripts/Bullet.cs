using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private float lifeTime;
    [SerializeField]
    private float speed;
    [SerializeField]
    private LayerMask collisionMask;
    [SerializeField]
    private float width;

    private GameObject owner;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Time.time > startTime + lifeTime)
        {
            Destroy(gameObject);
            return;
        }
        var diff = rb.position - lastPosition;  
        lastPosition = rb.position;
        Ray ray = new Ray(lastPosition, diff.normalized);
        RaycastHit hit;
        if (Physics.SphereCast(ray, width,out hit, diff.magnitude, collisionMask.value))
        {
            Target other = hit.collider.GetComponent<Target>();
            if (other != null && other.gameObject != owner)
            {
                // apply damage
            }
            Destroy(gameObject);
        }
    }
    public void Fire (GameObject owner)
    {
        this.owner = owner;
        rb = GetComponent<Rigidbody>();
        startTime = Time.time;

        rb.AddRelativeForce(new Vector3(0, 0, speed), ForceMode.VelocityChange);
        rb.AddForce(owner.GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
        lastPosition = rb.position;
    }

}
