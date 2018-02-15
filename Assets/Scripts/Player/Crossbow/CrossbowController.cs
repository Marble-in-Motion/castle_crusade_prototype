﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CrossbowController : MonoBehaviour {

	//    [SerializeField]
	//    private float lookSensitivity = 3f;
	[SerializeField]
	public GameObject arrow;

	private int speed = 150;
	private LineRenderer laserLine;

	CrossbowMotor motor;

	private AudioSource shootAudio;
	private WaitForSeconds singleShotDuration = new WaitForSeconds(0.05f);
	private WaitForSeconds volleyShotDuration = new WaitForSeconds(0.0025f);

	void Start() {
		motor = GetComponent<CrossbowMotor>();
		laserLine = GetComponent<LineRenderer>();
		shootAudio = GetComponent<AudioSource>();
		laserLine.enabled = true;
	}


	public IEnumerator HandleShoot() {
		shootAudio.Play();
		laserLine.enabled = true;
		yield return singleShotDuration;
		laserLine.enabled = false;
	}

	public void HandleArrow(Vector3 point) {
		SendArrow(new Vector3 (transform.position.x, transform.position.y, transform.position.z), speed, point);
	}

	public IEnumerator HandleVolley(GameObject[] troops) {
		Vector3 volleyLoc = new Vector3 (transform.position.x, transform.position.y + 2f, transform.position.z);
		List<Vector3> troopLocs = new List<Vector3> ();
		for (int i = 0; i < troops.Length; i++) {
			troopLocs.Add(troops[i].transform.position);
			troops[i].GetComponent<NPCHealth>().DeductHealth(troops[i].GetComponent<NPCHealth>().GetHealth(), speed, volleyLoc); 
		}
		foreach(Vector3 loc in troopLocs) {
			yield return volleyShotDuration;
			SendArrow (volleyLoc, speed, loc);
		}
	}

	public int GetArrowSpeed()
	{
		return speed;
	}

	private void SendArrow(Vector3 startLoc, int speed, Vector3 targetLoc) {
		GameObject bul = Instantiate (arrow, startLoc, transform.rotation);
		bul.GetComponent<ArrowController> ().Init (targetLoc);
		Rigidbody rb = bul.GetComponent<Rigidbody> ();
		Vector3 direction = targetLoc - transform.position;
		direction = Vector3.Normalize(direction);
		rb.velocity = speed*direction;
	}

	void Update () {
		UpdateMovement();
	}

	private void UpdateMovement()
	{
		motor.Rotate(Input.mousePosition);
	}

}