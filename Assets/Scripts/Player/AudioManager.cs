using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	[SerializeField]
	private AudioSource klaxon;

	public void PlaySound(string audioSourceName) {
		AudioSource audioSource = klaxon;
		audioSource.Play ();
	}

}
