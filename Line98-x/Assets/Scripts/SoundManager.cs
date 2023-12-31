﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] audioClips;

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> dicSound;

    private bool isMute = false;
    public bool IsMute {
        get => isMute; set {
            isMute = value;

            float volume = isMute ? 0 : 1f;
            this.audioSource.volume = volume;
            //PlayerPrefs.SetFloat("SoundVolume", volume);
        }
    }

    public static SoundManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Init()
    {
        this.audioSource = this.GetComponent<AudioSource>();

        this.dicSound = new Dictionary<string, AudioClip>(this.audioClips.Length);

        for (int i = 0; i < this.audioClips.Length; i++)
        {
            this.dicSound[this.audioClips[i].name] = this.audioClips[i];
        }

        float volume = 1;// PlayerPrefs.GetFloat("SoundVolume");
        this.IsMute = volume > 0 ? false : true;
    }

    public void PlaySound(string soundName, float volumeScale = 1f)
    {
        this.audioSource.PlayOneShot(this.dicSound[soundName], volumeScale);
    }

    public void PlayMusicBG()
    {
        this.audioSource.Play();
    }

    public void StopMusicBG()
    {
        this.audioSource.Stop();
    }
}
