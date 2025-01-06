using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    [SerializeField]
    private AudioSource audioSource;
    private void Awake()
    {
        instance= this;
    }
    public void PlayOneSpatialSound(AudioClip audioClip, Vector3 sourcePos, float volume)
    {
        // spawn in a GameObject
        AudioSource audio = Instantiate(audioSource, sourcePos, Quaternion.identity);
        // assign an audio clip
        audio.clip = audioClip;
        // assign a volume
        audio.volume = volume;
        // play the audio
        audio.Play();
        // get clip length to destroy object afterwards
        Destroy(audio.gameObject, audioClip.length);
    }
}
