using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mainMixer;
    
    public void OnMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
        MenuUIController.instance.OnUpdateSliderValues();
    }
    public void OnMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20f);
        MenuUIController.instance.OnUpdateSliderValues();
    }
    public void OnSfxVolume(float value)
    {
        mainMixer.SetFloat("SfxVolume", Mathf.Log10(value) * 20f);
        MenuUIController.instance.OnUpdateSliderValues();
    }
}
