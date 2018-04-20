using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource[] moreTroops;

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

    Dictionary<string, AudioSource> singleAudioSources = new Dictionary<string, AudioSource>();

    Dictionary<string, AudioSource[]> arrayAudioSources = new Dictionary<string, AudioSource[]>();

    public void BuildDicts()
    {
        BuildArrayDict();
        BuildSingleDict();
    }
         
    public void BuildSingleDict()
    {
        singleAudioSources.Add(Params.AMBIENCE, ambience);
        singleAudioSources.Add(Params.KLAXON, klaxon);
        singleAudioSources.Add(Params.COINS, coins);
        singleAudioSources.Add(Params.SWORD, sword);
        singleAudioSources.Add(Params.GONG, gong);
        singleAudioSources.Add(Params.HORN, horn);
        singleAudioSources.Add(Params.VOLLEY, volley);
        singleAudioSources.Add(Params.MAIN_MUSIC, mainMusic);
    }

    public void BuildArrayDict()
    {
        arrayAudioSources.Add(Params.MORE_TROOPS, moreTroops);
    }

    public void PlaySingleSound(string audioSourceName)
    {
        AudioSource audioSource = singleAudioSources[audioSourceName];
        AudioClip audioClip = audioSource.clip;
        audioSource.PlayOneShot(audioClip);
    }

    public bool PlayArraySound(string audioSourceName, int index)
    {
        try
        {
            if(index == Params.PLAY_RANDOM)
            {
                index = UnityEngine.Random.Range(0, arrayAudioSources[audioSourceName].Length);
            }
            AudioSource audioSource = arrayAudioSources[audioSourceName][index];
            AudioClip audioClip = audioSource.clip;
            audioSource.PlayOneShot(audioClip);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
            return false;
        }
        return true;
    }
}
