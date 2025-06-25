using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sfxmaneger : MonoBehaviour
{
    public AudioClip clicksound;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaycClickSound()
    {
     audioSource.PlayOneShot(clicksound);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

