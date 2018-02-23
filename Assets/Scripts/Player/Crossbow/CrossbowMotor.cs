using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CrossbowMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 mousePos;

	private int activePath;

    void Start()
    {
    }

	void Update()
	{
		lookAtTroop ();
	}

    // Run every physics iteration
//    void FixedUpdate()
//    {
//        PerformRotation();
//    }
//
//    // Gets a rotational vector
//    public void Rotate(Vector3 mousePosition)
//    {
//        mousePos = mousePosition;
//        mousePos.z = 10;
//    }
//
//    //Perform rotation
//    void PerformRotation()
//    {
//        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
//        transform.LookAt(worldPos);
//    }

	void lookAtTroop(){
		GameObject[] troopsInLane = this.GetComponentInParent<Player> ().FindEnemyTroopsInLane ();
		if (troopsInLane.Length != 0) {
			GameObject nearestTroop = findNearestTroop (troopsInLane);
      if(nearestTroop != null) {
        transform.LookAt(nearestTroop.transform.position);
      }
		}
	}

	GameObject findNearestTroop(GameObject[] troopsInLane){
		float minDist=float.MaxValue;
		GameObject nearestTroop = null;

		for (int i = 0; i < troopsInLane.Length; i++) {
      if(troopsInLane[i].GetComponent<AIController> ().GetPath() == activePath){
        GameObject tempTroop = troopsInLane [i];
  			float tempDistance = Vector3.Distance (tempTroop.transform.position, transform.position);
  			if (tempDistance < minDist) {
  				minDist = tempDistance;
  				nearestTroop = tempTroop;
  			}
      }
		}
		return nearestTroop;
	}

	public void moveRight()
	{
		if (activePath != 2) {
			activePath += 1;
		}
	}

	public void moveLeft()
	{
		if (activePath != 0) {
			activePath -= 1;
		}
	}

}
