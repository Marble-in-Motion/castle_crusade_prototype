using UnityEngine;
using UnityEngine.Networking;

public class PlayerShoot : NetworkBehaviour {

    public Bolt bolt;

    [SerializeField]
    private Camera cam;

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
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, bolt.range, mask))
        {
            if (hit.collider.tag == PlayerController.PLAYER_TAG)
            {
                CmdPlayerShot(hit.collider.name);
            }
        }
    }

    [Command]
    void CmdPlayerShot(string id)
    {
        Debug.Log(id + "hit");
    }
}
