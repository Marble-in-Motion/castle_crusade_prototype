using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CannonHandler : MonoBehaviour {

	private Bolt bolt;
	int level;
	float fireRate;
	float nextFire;

	[SerializeField]
	private LayerMask mask;

	private LineRenderer laserLine;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.05f);

	// Use this for initialization
	void Start () {
		level = 0;
		fireRate = 0.5f;
		bolt = new Bolt ();
		laserLine = GetComponent<LineRenderer> ();
		laserLine.enabled = true;
	}

	IEnumerator Wait() {
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.U)) {
			upgrade();
		}
		if (Time.time > nextFire) {
		    nextFire = Time.time + fireRate;
            int teamNum = this.GetComponentInParent<Player>().GetTeamId();
            int opposition = (teamNum == 1) ? 2 : 1;
            GameObject[] enemies = GameObject.FindGameObjectsWithTag ("NPCT"+ opposition);
            GameObject target = GetClosestEnemy (enemies);
			if (target != null) {
				shoot (target);
			}
		}
	}

	GameObject GetClosestEnemy (GameObject[] enemies)
	{
		GameObject bestTarget = null;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 currentPosition = this.transform.position;
		foreach(GameObject gameObj in enemies)
		{
			Transform potentialTarget = gameObj.transform;
			Vector3 directionToTarget = potentialTarget.position - currentPosition;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if(dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				bestTarget = gameObj;
			}
		}
		return bestTarget;
	}

	void shoot(GameObject target) {
		laserLine.SetPosition(0, this.transform.position);

		StartCoroutine (Wait ());

		RaycastHit hit;
		if (Physics.Raycast (this.transform.position, target.transform.position - this.transform.position, out hit, bolt.range, mask)) {
			this.GetComponentInParent<PlayerShoot> ().CmdPlayerShot (hit.collider.name, bolt.damage);
			Debug.DrawRay(this.transform.position, target.transform.position, Color.green);
			laserLine.SetPosition (1, hit.point);
		}
	}

	void upgrade() {
		Debug.Log ("upgrade");
		level += 1;
		bolt.damage += 10;
		switch (level) {
		case 1:
			this.GetComponent<Renderer>().material.color = new Color(255, 0, 0);
			break;
		case 2:
			this.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
			break;
		case 3:
			this.GetComponent<Renderer>().material.color = new Color(0, 0, 255);
			break;
		}
	}
}
