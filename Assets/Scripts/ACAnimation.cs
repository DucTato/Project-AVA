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
    
    private float engLightMaxIntensity;
    [SerializeField]
    private float abMaxVal;
    [SerializeField]
    private VisualEffect heatHaze;
    private void Awake()
    {
        engineLight = GetComponentsInChildren<Light>();
        //afterBurners = GetComponentsInChildren<ParticleSystem>();
        heatHaze = GetComponentInChildren<VisualEffect>();
#pragma warning disable CS0618 // Type or member is obsolete
        abMaxVal = afterBurners[0].startLifetime;
#pragma warning restore CS0618 // Type or member is obsolete
        engLightMaxIntensity = engineLight[0].intensity;
    }
    
    public void SetEnginePowerVisual(float powValue)
    {
        if (powValue >= 0.8f) heatHaze.enabled = true; 
        else heatHaze.enabled = false;
        // power value is the % of engine's max power
        Mathf.Clamp(powValue, 0.0f, 1.0f);
        foreach (Light light in engineLight) light.intensity = powValue * engLightMaxIntensity;
        //
        foreach (ParticleSystem ab in afterBurners)
        {
            if (powValue <= 0) ab.Stop();
            else ab.Play();
#pragma warning disable CS0618 // Type or member is obsolete
            ab.startLifetime = powValue * abMaxVal;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
