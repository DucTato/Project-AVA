using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("An array of transforms containing the camera positions")]
    Transform[] povs;
    [SerializeField]
    private float catchSpeed = 10f;

    private int camIndex = 0;
    private Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            // Cycling through the array of POVs
            camIndex++;
            if (camIndex >= povs.Length)
            {
                camIndex = 0;
            }
        }
        target = povs[camIndex].position;
    }
    private void FixedUpdate()
    {
        // Updating the camera position in this callback function to avoid jittering
        transform.position = Vector3.MoveTowards(transform.position, target, catchSpeed * Time.deltaTime);
        transform.forward = povs[camIndex].forward;
        //Debug.Log("Camera culling");
    }
}
