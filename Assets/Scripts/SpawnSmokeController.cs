using UnityEngine;

/**
 * Smoke handler for spawning npc's
 **/
public class SpawnSmokeController : MonoBehaviour
{

    private float destroyTime;

    void Start()
    {
        destroyTime = Time.time + 5;
    }

    void Update()
    {
        if (Time.time > destroyTime)
        {
            Destroy(gameObject);
        }
    }
}
