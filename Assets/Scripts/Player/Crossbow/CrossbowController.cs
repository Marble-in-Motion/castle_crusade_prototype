using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CrossbowController : MonoBehaviour {

//    [SerializeField]
//    private float lookSensitivity = 3f;

	private LineRenderer laserLine;

    CrossbowMotor motor;

	private AudioSource shootAudio;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	void Start () {
        motor = GetComponent<CrossbowMotor>();
		laserLine = GetComponent<LineRenderer>();
		shootAudio = GetComponent<AudioSource>();
		laserLine.enabled = true;
	}

	public IEnumerator HandleShoot() {
		shootAudio.Play();
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}

	void Update () {
        UpdateMovement();
	}

    private void UpdateMovement()
    {
        motor.Rotate(Input.mousePosition);
    }

}