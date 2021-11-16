using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVoices : MonoBehaviour
{
    public AudioClip[] sounds;
    private AudioSource source;

    public float volume = 1.0f;
    public float pitch = 1.0f;

    private int clipIndex = 0;
    float currTimeLeft = 0;

    public float minTime = 0f;
    public float maxTime = 35f;

    void Awake()
    {
        currTimeLeft = Random.Range(minTime, maxTime);
        clipIndex = Random.Range(0, sounds.Length);

        source = GetComponent<AudioSource>();
        source.clip = sounds[clipIndex];
        source.loop = false;

        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        currTimeLeft -= Time.deltaTime;
        if (currTimeLeft < 0)
        {
            currTimeLeft = Random.Range(minTime, maxTime);
            if (!source.isPlaying)
            {
                PlayVoice();
            }
        }
    }

    void PlayVoice()
    {
        clipIndex = Random.Range(0, sounds.Length);
        source.clip = sounds[clipIndex];
        source.Play();
    }
}
