using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    public static MusicHandler Instance;

    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    void Awake()
    {
        // Make sure this object stays alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        musicSource.clip = gameMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
