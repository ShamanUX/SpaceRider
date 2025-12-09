using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundOptionsController : MonoBehaviour
{
    private bool musicEnabled = false;
    private bool sfxEnabled = true;
    public AudioMixerGroup masterGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetMusic(bool enabled)
    {
        musicEnabled = enabled;

        transform.Find("music").gameObject.SetActive(enabled);
        transform.Find("musicOff").gameObject.SetActive(!enabled);

        if (enabled)
        {
            masterGroup.audioMixer.SetFloat("AllMusicVolume", 0);
        } else
        {
            masterGroup.audioMixer.SetFloat("AllMusicVolume", -80);
        }
    }

    public void SetSfx(bool enabled)
    {
        sfxEnabled = enabled;
        transform.Find("sfx").gameObject.SetActive(enabled);
        transform.Find("sfxOff").gameObject.SetActive(!enabled);

        if (enabled)
        {
            masterGroup.audioMixer.SetFloat("AllSfxVolume", 0);
        }
        else
        {
            masterGroup.audioMixer.SetFloat("AllSfxVolume", -80);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
