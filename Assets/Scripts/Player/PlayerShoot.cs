using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerShoot : NetworkBehaviour {

	public Bolt bolt;

	[SerializeField]
	private GameObject crossbow;

	[SerializeField]
	private LayerMask mask;

	private AudioSource shootAudio;
	private LineRenderer laserLine;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	void Start () {
		laserLine = GetComponent<LineRenderer> ();
		shootAudio = GetComponent<AudioSource> ();
	}

	void Update () {
		if (Input.GetButtonDown("Fire1"))
		{
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
		laserLine.SetPosition(0, crossbow.transform.position);

		StartCoroutine(Wait());

		RaycastHit hit;
		if (Physics.Raycast(crossbow.transform.position, crossbow.transform.forward, out hit, bolt.range, mask)) {
			CmdPlayerShot(hit.collider.name, bolt.damage);
			laserLine.SetPosition(1, hit.point);
		} else {
			//calculate max range of projectile
			laserLine.SetPosition(1, crossbow.transform.position + crossbow.transform.forward * bolt.range);
		}

	}

	[Command]
	public void CmdPlayerShot(string id, float damage)
	{
		Debug.Log(id + " hit");
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage);
		}
		if (target.transform != null /*&& target.collider.tag == "NPC"*/) {
			target.transform.position = (target.transform.position /*- (normal of the hit)*/);
		}


	}
}