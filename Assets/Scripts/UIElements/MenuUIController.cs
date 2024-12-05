using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject switchON, switchOFF;
    // Start is called before the first frame update
    void Start()
    {
        switchON.SetActive(false);
        switchOFF.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switchOFF.SetActive(false);
            switchON.SetActive(true);
        }
    }
}
