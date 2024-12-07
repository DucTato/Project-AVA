using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class ACAnimation : MonoBehaviour
{
    [SerializeField]
    private float AC_LENGTH;
    [SerializeField]
    private ParticleSystem[] afterBurners;
    [SerializeField]
    private GameObject acBody, i2dBody;


    [SerializeField]
    private Material dissolveMat;

    private Light[] engineLight;
    private float engLightMaxIntensity;
    private float abMaxVal;
    private VisualEffect heatHaze;

    private List<Material> acMats, i2dMats, defaulMats;
    private List<Renderer> acBodyRenderers;
    private void Awake()
    {
        
        engineLight = GetComponentsInChildren<Light>();
        heatHaze = GetComponentInChildren<VisualEffect>();
#pragma warning disable CS0618 // Type or member is obsolete
        abMaxVal = afterBurners[0].startLifetime;
#pragma warning restore CS0618 // Type or member is obsolete
        engLightMaxIntensity = engineLight[0].intensity;

        // Handles the materials for body swapping
        if (acBody == null || i2dBody == null) return;
        acMats = new List<Material>();
        i2dMats = new List<Material>();
        defaulMats = new List<Material>();
        acBodyRenderers = acBody.GetComponentsInChildren<Renderer>().ToList();
        for (int i = 0; i < acBodyRenderers.Count; i++)
        {
            // renderers in the base aircraft body (including missiles,...)
            defaulMats.AddRange(acBodyRenderers[i].materials);
            acBodyRenderers[i].material = dissolveMat;
            acMats.AddRange(acBodyRenderers[i].materials);
        }
        // applies the "BaseMap" texture of dissolve with the texture from the main aircraft
        dissolveMat.SetTexture("_BaseMap",defaulMats[0].GetTexture("_MainTex"));
        foreach (var renderer in acBodyRenderers)
        {
            defaulMats.AddRange(renderer.materials);
            acMats.AddRange(renderer.materials);
        }
        foreach (var renderer2 in i2dBody.GetComponentsInChildren<Renderer>())
        {
            i2dMats.AddRange(renderer2.materials);
        }
        foreach (var mat in acMats)
        {
            mat.SetVector("_DissolveDirection", new Vector4(0f, 0f, -1f, 0f));
            mat.SetVector("_DissolveOffest", new Vector4(0f, 0f, -AC_LENGTH,0f));
        }
        foreach (var mat2 in i2dMats)
        {
            mat2.SetVector("_DissolveDirection", new Vector4(0f, 0f, 1f, 0f));
            mat2.SetVector("_DissolveOffest", new Vector4(0f, 0f, -AC_LENGTH,0f));
        }
        
    }
    
    public void SetEnginePowerVisual(float powValue)
    {
        if (i2dBody != null) powValue = 0f; // will not animate the light if the plane is in its I2D form
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

    public void StartTransformation()
    {
        Debug.Log("Start transformation");
        // Starts the transformation
        foreach (var mat in acMats)
        {
            mat.DOVector(new Vector4(0f, 0f, AC_LENGTH, 0f), "_DissolveOffest", 8f);
            mat.DOColor(Color.white, "_BaseColor",8f).OnComplete(Reapply);
        }
        foreach (var mat2 in i2dMats)
        {
            mat2.DOVector(new Vector4(0f, 0f, AC_LENGTH, 0f), "_DissolveOffest", 8f);
        }        
    }
    private void Reapply()
    {
        for (int i = 0; i < acBodyRenderers.Count; i++)
        {
            acBodyRenderers[i].material = defaulMats[i];
        }
        Destroy(i2dBody);
    }
}
