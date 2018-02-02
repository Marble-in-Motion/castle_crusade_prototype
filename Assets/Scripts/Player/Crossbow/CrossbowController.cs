using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CrossbowController : NetworkBehaviour {

	public Bolt bolt;

    [SerializeField]
    private float lookSensitivity = 3f;

    CrossbowMotor motor;

    [SerializeField]
	private LayerMask mask;

	private AudioSource shootAudio;
	private LineRenderer laserLine;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	void Start () {
        motor = GetComponent<CrossbowMotor>();
		laserLine = GetComponent<LineRenderer>();
		shootAudio = GetComponent<AudioSource>();
		bolt = new Bolt ();
	}

	void Update () {
        UpdateMovement();
		if (Input.GetButtonDown("Fire1"))
		{
			Debug.Log ("Shot fired");
			Shoot();
		}
	}

	IEnumerator Wait() {
		shootAudio.Play();
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}

	[Client]
	void Shoot()
	{
		laserLine.SetPosition(0, transform.position);
		StartCoroutine(Wait());
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, bolt.range, mask)) {
			Debug.Log("4");
			CmdPlayerShot(hit.collider.name, bolt.damage);
			laserLine.SetPosition(1, hit.point);
		} else {
			//calculate max range of projectile
			Debug.Log("5");
			laserLine.SetPosition(1, transform.position + transform.forward * bolt.range);
		}
	}

	[Command]
	public void CmdPlayerShot(string id, float damage)
	{
		Debug.Log(id + " hit");
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage);
            if (!target.GetComponent<NPCHealth>().IsAlive())
            {
                this.GetComponentInParent<Player>().CmdAddGold(10);
            }
		}
		if (target.transform != null /*&& target.collider.tag == "NPC"*/) {
			target.transform.position = (target.transform.position /*- (normal of the hit)*/);
		}
	}

    private void UpdateMovement()
    {
        motor.Rotate(Input.mousePosition);
    }

}