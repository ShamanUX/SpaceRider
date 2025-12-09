using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public GameObject musicFilteredObject;
    public GameObject musicUnfilteredObject;

    private bool fadeoutRunning = false;

    private void Start()
    {
        musicFilteredObject.GetComponent<AudioSource>().Play();
        musicUnfilteredObject.GetComponent<AudioSource>().Play();   
    }
    public void PlayAudio(string audioObjectName)
    {
       AudioSource audioSource = transform.Find(audioObjectName).GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.Play();
    }

    public void PlayAudio(string audioObjectName, float pitch)
    {
        AudioSource audioSource = transform.Find(audioObjectName).GetComponent<AudioSource>();
        audioSource.pitch = pitch;
        audioSource.Stop();
        audioSource.Play();
    }

    public void StopAudio(string audioObjectName)
    {
        AudioSource source = transform.Find(audioObjectName).GetComponent<AudioSource>();
        if (source.loop && source.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(FadeSound(source, source.volume, 0, 0.3f));
            //Debug.Log("Start fadeout");
            fadeoutRunning = true;
        }
        else
        {
            source.Stop();
        }
    }

    public void PlayAudioLoop(string audioObjectName)
    {

        AudioSource audioSource = transform.Find(audioObjectName).GetComponent<AudioSource>();
        if (audioSource.isPlaying != true)
        {
            audioSource.Play();
        }
        StopAllCoroutines();
        fadeoutRunning = false;
        StartCoroutine(FadeSound(audioSource, audioSource.volume, 1, 0.3f));
        //Debug.Log("Start Fadein");

    }

    private IEnumerator FadeSound(AudioSource audioSource, float startVolume, float targetVolume, float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        // Stop sound if faded out completely
        if (targetVolume <= 0f)
        {
            //audioSource.Stop();
            //audioSource.volume = startVolume; // Reset volume
            Debug.Log("End fadeout");
            fadeoutRunning = false;
        }
    }

}