using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour {

    public Bolt bolt;

    [SerializeField]
    private Camera cam;

	[SerializeField]
	private GameObject crossbow;

    [SerializeField]
    private LayerMask mask;

	void Update () {
		if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
	}

    [Client]
    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(crossbow.transform.position, crossbow.transform.forward, out hit, bolt.range, mask))
        {
            CmdPlayerShot(hit.collider.name, bolt.damage);
			Debug.DrawRay(crossbow.transform.position, crossbow.transform.forward, Color.green);
        }
    }

    [Command]
	void CmdPlayerShot(string id, float damage)
    {
        Debug.Log(id + " hit");
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage);
		}
    }
}
