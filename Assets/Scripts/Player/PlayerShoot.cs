using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerShoot : NetworkBehaviour {

    public Bolt bolt;

    [SerializeField]
    private Camera cam;

	[SerializeField]
	private GameObject crossbow;

    [SerializeField]
    private LayerMask mask;

	private AudioSource shootAudio;
	private LineRenderer laserLine;
	private float nextFire;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	void Start () {
		laserLine = GetComponent<LineRenderer> ();
		shootAudio = GetComponent<AudioSource> ();
	}

	void Update () {
		if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        {
			nextFire = Time.time + bolt.fireRate;
            Shoot();
        }
	}

	IEnumerator Wait() {
		shootAudio.Play ();
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;

	}

    [Client]
    void Shoot()
    {

		laserLine.SetPosition(0, crossbow.transform.position);

		StartCoroutine (Wait ());

        RaycastHit hit;
		if (Physics.Raycast (crossbow.transform.position, crossbow.transform.forward, out hit, bolt.range, mask)) {
			CmdPlayerShot (hit.collider.name, bolt.damage, hit.normal);
			laserLine.SetPosition (1, hit.point);
		} else {
			//maybe extract this too
			laserLine.SetPosition (1, crossbow.transform.position + crossbow.transform.forward * bolt.range);
		}

		//MOVE THIS THE FUCK OUT

    }

    [Command]
	void CmdPlayerShot(string id, float damage)
    {
        Debug.Log(id + " hit");
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage);
		}
		if (target.transform != null && target.collider.tag == "NPC") {
			target.transform.position = (target.transform.position -((2) * 1/*normal of the hit*/));
		}


    }
}
