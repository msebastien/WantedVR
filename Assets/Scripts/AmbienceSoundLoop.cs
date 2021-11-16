using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSoundLoop : MonoBehaviour
{
    public AudioClip[] sounds;
    private AudioSource source;

    public float volume = 1.0f;
    public float pitch = 1.0f;

    private int clipIndex = 0;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = sounds[clipIndex];
        source.loop = false;

        if (sounds.Length == 1)
            source.loop = true;

        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        clipIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        if (source.time == sounds[clipIndex - 1].length)
        {
            if (!source.isPlaying && sounds.Length > 0 && clipIndex < sounds.Length)
            {
                PlayMusic();
            }
            else if (clipIndex >= sounds.Length)
            {
                clipIndex = 0;
                PlayMusic();
            }
        } 
    }

    void PlayMusic()
    {
        source.clip = sounds[clipIndex];
        source.Play();
        clipIndex++;
    }


}
