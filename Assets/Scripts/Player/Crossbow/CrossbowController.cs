using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CrossbowController : MonoBehaviour {

	//    [SerializeField]
	//    private float lookSensitivity = 3f;
	[SerializeField]
	public GameObject arrow;

	private int speed = 150;
	private LineRenderer laserLine;

	CrossbowMotor motor;

	private AudioSource shootAudio;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	void Start() {
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

	public void HandleArrow(Vector3 point) {
		GameObject bul = Instantiate (arrow, new Vector3 (transform.position.x, transform.position.y, transform.position.z), transform.rotation);
		bul.GetComponent<ArrowController> ().Init (point);
		Rigidbody rb = bul.GetComponent<Rigidbody> ();
		Vector3 direction = point - transform.position;
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