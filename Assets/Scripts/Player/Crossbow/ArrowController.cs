using UnityEngine;

/**
 * Controller for crossbow's arrow object
 **/
public class ArrowController : MonoBehaviour
{

    private float travelTime;
    private float startTime;

    private Vector3 target;

    public void Init(Vector3 t)
    {
        startTime = Time.time;
        target = t;
        transform.LookAt(target);
        transform.Rotate(0, 90, 0);
    }

    // Update is called once per frame
    void Update()
    {
        travelTime = Time.time - startTime;
        if (travelTime >= 2)
        {
            Destroy(gameObject);
        }
    }
}
