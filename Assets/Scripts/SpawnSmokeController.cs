using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSmokeController : MonoBehaviour {

	private float destroyTime;

	// Use this for initialization
	void Start () {
		destroyTime = Time.time + 5;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > destroyTime)
		{
			Destroy(gameObject);
		}
	}
}
