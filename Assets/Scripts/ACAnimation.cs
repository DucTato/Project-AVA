using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACAnimation : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem[] afterBurners;
    [SerializeField]
    private Light[] engineLight;
    [SerializeField]
    private float abMaxVal, engLightMaxVal;
    private void Awake()
    {
        engineLight = GetComponentsInChildren<Light>();
        afterBurners = GetComponentsInChildren<ParticleSystem>();
        abMaxVal = afterBurners[0].startLifetime;
        engLightMaxVal = engineLight[0].intensity;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetEnginePowerVisual(float powValue)
    {
        // power value is the % of engine's max power
        Mathf.Clamp(powValue, 0.0f, 1.0f);
        foreach (Light light in engineLight) light.intensity = powValue * engLightMaxVal;
        //
        foreach (ParticleSystem ab in afterBurners) ab.startLifetime = powValue * abMaxVal;
    }
}
