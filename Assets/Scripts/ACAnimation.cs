using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ACAnimation : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem[] afterBurners;
    [SerializeField]
    private Light[] engineLight;
    [SerializeField]
    private float abMaxVal, engLightMaxVal;
    [SerializeField]
    private VisualEffect heatHaze;
    private void Awake()
    {
        engineLight = GetComponentsInChildren<Light>();
        //afterBurners = GetComponentsInChildren<ParticleSystem>();
        heatHaze = GetComponentInChildren<VisualEffect>();
        abMaxVal = afterBurners[0].startLifetime;
        engLightMaxVal = engineLight[0].intensity;
    }
    
    public void SetEnginePowerVisual(float powValue)
    {
        if (powValue >= 0.8f) heatHaze.enabled = true; 
        else heatHaze.enabled = false;
        // power value is the % of engine's max power
        Mathf.Clamp(powValue, 0.0f, 1.0f);
        foreach (Light light in engineLight) light.intensity = powValue * engLightMaxVal;
        //
        foreach (ParticleSystem ab in afterBurners) ab.startLifetime = powValue * abMaxVal;
    }
}
