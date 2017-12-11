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

	// Use this for initialization
	void Start () {
		level = 0;
		fireRate = 0.5f;
		bolt = new Bolt ();
	}

	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.U)) {
			upgrade();
		}
		if (Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			GameObject[] enemies = GameObject.FindGameObjectsWithTag ("NPC");
			GameObject target = GetClosestEnemy (enemies);
			shoot (target);
		}
	}

	GameObject GetClosestEnemy (GameObject[] enemies)
	{
		GameObject bestTarget = new GameObject();
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
		RaycastHit hit;
		if (Physics.Raycast (this.transform.position, target.transform.position - this.transform.position, out hit, bolt.range, mask)) {
			this.GetComponentInParent<PlayerShoot> ().CmdPlayerShot (hit.collider.name, bolt.damage);
			Debug.DrawRay(this.transform.position, target.transform.position, Color.green);
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
