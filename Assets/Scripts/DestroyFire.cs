using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFire : MonoBehaviour {

    private float destroyTime;

	// Use this for initialization
	void Start () {
        destroyTime = Time.time + 3;
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Time.time > destroyTime)
        {
            Destroy(gameObject);
        }
		
	}
}
