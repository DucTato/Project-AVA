using UnityEngine;

public class Blinking : MonoBehaviour
{
    [SerializeField]
    private float animationSpdMult;
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Animator>().speed = animationSpdMult;
    }

}
