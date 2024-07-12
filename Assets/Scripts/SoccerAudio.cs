using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerAudio : MonoBehaviour
{
    public AudioSource audioSource;     // AudioSource component on this object
    public AudioClip[] audioClips;      // Sound files that this script can play

    private int headCenterIndex = 0;    // Play this sound when the user has centered their head in the "dark screen" stage

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Play a sound when the user has centered their head
    public void HeadCenterAudio()
    {
        audioSource.clip = audioClips[headCenterIndex];
        //Debug.Log("hit" + audioSource.clip.name);
        audioSource.Play();
    }

    // True when the attached object is currently playing an audioclip
    // Called by Manager
    public bool GetIsPlaying()
    {
        //Debug.Log(audioSource.isPlaying);
        return audioSource.isPlaying;
    }
}
