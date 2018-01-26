using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CollisionHandler : MonoBehaviour {

    [SerializeField]
    private GameObject teamController;

    private int NPCDamage;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("COllision");
        NPCDamage = 10;
        teamController.GetComponent<TeamController>().DeductTowerHealth(NPCDamage);
        Destroy(other.gameObject);
    }

}
