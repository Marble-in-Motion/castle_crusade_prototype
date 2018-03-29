using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField]
    private AudioSource klaxon;

    [SerializeField]
    private AudioSource coins;

    [SerializeField]
    private AudioSource sword;

    [SerializeField]
    private AudioSource gong;

    [SerializeField]
    private AudioSource horn;

    [SerializeField]
    private AudioSource ambience;

    [SerializeField]
    private AudioSource volley;

    Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    public void BuildDict()
    {
        audioSources.Add("ambience", ambience);
        audioSources.Add("klaxon", klaxon);
        audioSources.Add("coins", coins);
        audioSources.Add("sword", sword);
        audioSources.Add("gong", gong);
        audioSources.Add("horn", horn);
        audioSources.Add("volley", volley);
    }

    public void PlaySound(string audioSourceName)
    {
        AudioSource audioSource = audioSources[audioSourceName];
        AudioClip audioClip = audioSource.clip;
        audioSource.PlayOneShot(audioClip);
    }

}
