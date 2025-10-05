using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public void PlayAudio(string audioObjectName)
    {
       AudioSource audioSource = transform.Find(audioObjectName).GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.Play();
    }

    public void StopAudio(string audioObjectName)
    {
        transform.Find(audioObjectName).GetComponent<AudioSource>().Stop();
    }

    public void PlayAudioLoop(string audioObjectName)
    {
        AudioSource audioSource = transform.Find(audioObjectName).GetComponent<AudioSource>();
        if (audioSource.isPlaying != true)
        {
            audioSource.Play();
        }
    }

}