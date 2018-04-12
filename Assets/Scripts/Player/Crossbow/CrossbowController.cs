using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CrossbowController : MonoBehaviour {


	[SerializeField]
	public GameObject arrow;

	private int speed = 150;
	private LineRenderer laserLine;

	private AudioSource shootAudio;
    private AudioClip arrowSound;
	private WaitForSeconds singleShotDuration = new WaitForSeconds(0.05f);
	private WaitForSeconds volleyShotDuration = new WaitForSeconds(0.0025f);

	void Start() {
		laserLine = GetComponent<LineRenderer>();
		shootAudio = GetComponent<AudioSource>();
        arrowSound = shootAudio.clip;
		laserLine.enabled = true;
	}

    public void HandleShoot()
    {
        LineRenderer laserLine = GetComponent<LineRenderer>();
        laserLine.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Params.Bolt.RANGE))
        {
            GameObject target = GameObject.Find(hit.collider.name);

            if (target != null && target.GetComponent<NPCHealth>())
            {
                Player player = GetComponentInParent<Player>();
                player.CmdApplyDamage(target);

                if (!target.GetComponent<NPCHealth>().IsAlive())
                {
                    player.CmdAddGold(Params.NPC_REWARD[target.GetComponentInParent<AIController>().TroopType]);
                }
            }

            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, transform.position + transform.forward * Params.Bolt.RANGE);
        }
        HandleArrow(laserLine.GetPosition(1));
    }


    public IEnumerator HandleVolley(GameObject[] troops) {
		Vector3 volleyLoc = new Vector3 (transform.position.x, transform.position.y + 2f, transform.position.z);
		List<Vector3> troopLocs = new List<Vector3> ();
		for (int i = 0; i < troops.Length; i++) {
            if (troops[i].GetComponent<NPCHealth>().IsAlive())
            {
                troopLocs.Add(troops[i].transform.position);
                troops[i].GetComponent<NPCHealth>().DeductHealth(Params.NPC_HEALTH[0]);
            }
		}
		foreach (Vector3 loc in troopLocs) {
			yield return volleyShotDuration;
			SendArrow(volleyLoc, loc);
		}
	}

	public int GetArrowSpeed()
	{
		return speed;
	}

    private void HandleArrow(Vector3 targetPosition)
    {
        shootAudio.PlayOneShot(arrowSound);
        SendArrow(transform.position, targetPosition);
    }

    private void SendArrow(Vector3 startPosition, Vector3 targetPosition) {
        GameObject arrow = Instantiate(this.arrow, startPosition, transform.rotation);
		arrow.GetComponent<ArrowController>().Init(targetPosition);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
		Vector3 direction = Vector3.Normalize(targetPosition - startPosition);
		rb.velocity = speed*direction;
	}

}