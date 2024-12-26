using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererTrigger : MonoBehaviour
{
    private void OnBecameVisible()
    {
        transform.root.GetComponent<ACAnimation>().StartTransformation();
    }
    
}
