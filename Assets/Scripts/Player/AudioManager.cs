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

    [SerializeField]
    private AudioSource mainMusic;

    Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    public void BuildDict()
    {
        audioSources.Add(Params.AMBIENCE, ambience);
        audioSources.Add(Params.KLAXON, klaxon);
        audioSources.Add(Params.COINS, coins);
        audioSources.Add(Params.SWORD, sword);
        audioSources.Add(Params.GONG, gong);
        audioSources.Add(Params.HORN, horn);
        audioSources.Add(Params.VOLLEY, volley);
        audioSources.Add(Params.MAIN_MUSIC, mainMusic);
    }

    public void PlaySound(string audioSourceName)
    {
        AudioSource audioSource = audioSources[audioSourceName];
        AudioClip audioClip = audioSource.clip;
        audioSource.PlayOneShot(audioClip);
    }

}
